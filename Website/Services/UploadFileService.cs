using System;
using System.Globalization;
using System.IO;
using NuGetGallery.FileAsyncUpload;

namespace NuGetGallery
{
    public class UploadFileService : IUploadFileService
    {
        readonly IFileStorageService fileStorageService;
        private readonly ICacheService cacheService;
        
        public UploadFileService(IFileStorageService fileStorageService, ICacheService cacheService)
        {
            this.fileStorageService = fileStorageService;
            this.cacheService = cacheService;
        }

        static string BuildFileName(int userKey)
        {
            return String.Format(CultureInfo.InvariantCulture, Constants.UploadFileNameTemplate, userKey, Constants.NuGetPackageFileExtension);
        }

        public void DeleteUploadFile(int userKey)
        {
            if (userKey < 1)
                throw new ArgumentException("A user key is required.", "userKey");

            var uploadFileName = BuildFileName(userKey);

            fileStorageService.DeleteFile(Constants.UploadsFolderName, uploadFileName);
        }

        public Stream GetUploadFile(int userKey)
        {
            if (userKey < 1)
                throw new ArgumentException("A user key is required.", "userKey");

            var uploadFileName = BuildFileName(userKey);

            return fileStorageService.GetFile(Constants.UploadsFolderName, uploadFileName);
        }

        public void SaveUploadFile(
            int userKey,
            Stream packageFileStream)
        {
            if (userKey < 1)
                throw new ArgumentException("A user key is required.", "userKey");
            if (packageFileStream == null)
                throw new ArgumentNullException("packageFileStream");

            var uploadFileName = BuildFileName(userKey);

            fileStorageService.SaveFile(Constants.UploadsFolderName, uploadFileName, packageFileStream);
        }

        public AsyncFileUploadProgressDetails GetProgressDetails(string userKey)
        {
            string cacheKey = GetFileUploadCacheKey(userKey);
            return (AsyncFileUploadProgressDetails)cacheService.GetItem(cacheKey);
        }

        public void RemoveProgressDetails(string userKey)
        {
            string cacheKey = GetFileUploadCacheKey(userKey);
            cacheService.RemoveItem(cacheKey);
        }

        public void SetProgressDetails(string userKey, AsyncFileUploadProgressDetails progressDetails)
        {
            string cacheKey = GetFileUploadCacheKey(userKey);
            cacheService.SetItem(cacheKey, progressDetails, TimeSpan.FromDays(1));
        }

        private string GetFileUploadCacheKey(string userkey)
        {
            return "upload:" + userkey;
        }
    }
}