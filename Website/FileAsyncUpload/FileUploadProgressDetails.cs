using System.Web.Script.Serialization;

namespace NuGetGallery.FileAsyncUpload
{
    public class AsyncFileUploadProgressDetails
    {
        internal AsyncFileUploadProgressDetails(int totalBytes, int bytesRead, string fileName)
        {
            TotalBytes = totalBytes;
            BytesRead = bytesRead;
            FileName = fileName;
        }

        [ScriptIgnore]
        public int TotalBytes
        {
            get;
            private set;
        }

        [ScriptIgnore]
        public int BytesRead
        {
            get;
            private set;
        }

        public string FileName
        {
            get;
            private set;
        }

        public int Progress
        {
            get
            {
                return (int)((long)BytesRead * 100 / TotalBytes);
            }
        }

        [ScriptIgnore]
        public int BytesRemaining
        {
            get
            {
                return TotalBytes - BytesRead;
            }
        }
    }
}