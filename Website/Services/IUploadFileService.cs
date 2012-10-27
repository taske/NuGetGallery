using System.IO;
using NuGet;
using NuGetGallery.FileAsyncUpload;

namespace NuGetGallery
{
    public interface IUploadFileService
    {
        void DeleteUploadFile(int userKey);
        
        Stream GetUploadFile(int userKey);
        
        void SaveUploadFile(int userKey, Stream packageFileStream);

        AsyncFileUploadProgressDetails GetProgressDetails(string userKey);

        void RemoveProgressDetails(string key);

        void SetProgressDetails(string key, AsyncFileUploadProgressDetails progressDetails);
    }
}