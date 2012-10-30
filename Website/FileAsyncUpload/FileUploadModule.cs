using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using Ninject;
using NuGetGallery.Helpers;

namespace NuGetGallery.FileAsyncUpload
{
    public class AsyncFileUploadModule : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication application)
        {
            var uploadFileService = Container.Kernel.Get<IUploadFileService>();

            application.AddOnPostAuthorizeRequestAsync(
                BeginPostAuthorizeRequest,
                EndPostAuthorizeRequest,
                uploadFileService);
        }

        private IAsyncResult BeginPostAuthorizeRequest(object sender, EventArgs e, AsyncCallback cb, object extraData)
        {
            var application = (HttpApplication)sender;
            var uploadFileServie = (IUploadFileService)extraData;

            return ReadUploadedFileStream(application.Context, uploadFileServie).ToApm(cb, extraData);
        }

        private void EndPostAuthorizeRequest(IAsyncResult ar)
        {
            ((Task<int>)ar).Wait();
        }

        private Task<int> ReadUploadedFileStream(HttpContext context, IUploadFileService uploadFileService)
        {
            if (!IsAsyncUploadRequest(context))
            {
                return Task.FromResult(0);
            }

            if (!context.User.Identity.IsAuthenticated)
            {
                return Task.FromResult(0);
            }

            var username = context.User.Identity.Name;
            if (String.IsNullOrEmpty(username))
            {
                return Task.FromResult(0);
            }

            HttpRequest request = context.Request;
            string contentType = request.ContentType;
            int boundaryIndex = contentType.IndexOf("boundary=", StringComparison.OrdinalIgnoreCase);
            string boundary = "--" + contentType.Substring(boundaryIndex + 9);
            var requestParser = new FileUploadRequestParser(boundary, request.ContentEncoding);

            var progress = new AsyncFileUploadProgressDetails(request.ContentLength, 0, String.Empty);
            uploadFileService.SetProgressDetails(username, progress);

            if (request.ReadEntityBodyMode != ReadEntityBodyMode.None)
            {
                return Task.FromResult(0);
            }

            Stream uploadStream = request.GetBufferedInputStream();
            Debug.Assert(uploadStream != null);

            return ReadStream(
                context, 
                uploadStream, 
                username, 
                progress, 
                requestParser, 
                uploadFileService);
        }

        private async Task<int> ReadStream(
            HttpContext context,
            Stream stream, 
            string userKey,
            AsyncFileUploadProgressDetails progress, 
            FileUploadRequestParser parser,
            IUploadFileService uploadFileService)
        {
            const int bufferSize = 1024 * 4; // in bytes

            var buffer = new byte[bufferSize];
            while (progress.BytesRemaining > 0)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, Math.Min(progress.BytesRemaining, bufferSize));
                int newBytesRead = bytesRead == 0
                                    ? progress.TotalBytes
                                    : (progress.BytesRead + bytesRead);

                string newFileName = progress.FileName;

                if (bytesRead > 0)
                {
                    parser.ParseNext(buffer, bytesRead);
                    newFileName = parser.CurrentFileName;
                }

                // After the 'await' call, this code may execute on a worker's thread.
                // in which case, an HttpContext may not be available.
                if (HttpContext.Current == null)
                {
                    HttpContext.Current = context;
                }
                
                progress = new AsyncFileUploadProgressDetails(progress.TotalBytes, newBytesRead, newFileName);
                uploadFileService.SetProgressDetails(userKey, progress);

#if DEBUG
                // for demo purpose only
                System.Threading.Thread.Sleep(300);
#endif
            }

            return 0;
        }

        private static bool IsAsyncUploadRequest(HttpContext context)
        {
            // not a POST request
            if (!context.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // not a multipart content type
            string contentType = context.Request.ContentType;
            if (!contentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (contentType.IndexOf("boundary=", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return false;
            }

            // Don't deal with transfer-encoding-chunked and less than 4KB
            if (context.Request.ContentLength < 4096)
            {
                return false;
            }

            return true;
        }
    }
}