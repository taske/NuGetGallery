using System.Collections.Concurrent;

namespace NuGetGallery.FileAsyncUpload
{
    public static class FileUploadManager
    {
        private static readonly ConcurrentDictionary<string, AsyncFileUploadProgressDetails> _Progress =
            new ConcurrentDictionary<string, AsyncFileUploadProgressDetails>();

        public static void SetProgressDetails(string key, AsyncFileUploadProgressDetails progressDetails)
        {
            _Progress[key] = progressDetails;
        }

        public static AsyncFileUploadProgressDetails GetProgressDetails(string key)
        {
            AsyncFileUploadProgressDetails progressDetails;
            if (!_Progress.TryGetValue(key, out progressDetails))
            {
                progressDetails = null;
            }

            return progressDetails;
        }

        public static void RemoveProgressDetails(string key)
        {
            AsyncFileUploadProgressDetails details;
            _Progress.TryRemove(key, out details);
        }
    }
}