using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using System;
using System.Data.Common; 

namespace EaglesJungscharen.MediaLibrary.Services {

    public abstract class BlobStorageService {
        private BlobContainerClient _blobContainerClient;
        private StorageSharedKeyCredential _storageSharedKeyCredential;
        private string _blobContainerName;
        public BlobStorageService(string connectionString, string blobContainerName) {
            _blobContainerName = blobContainerName;
             _blobContainerClient = new BlobContainerClient(connectionString , _blobContainerName);
             _blobContainerClient.CreateIfNotExists(PublicAccessType.Blob);
             var conBuilder = new DbConnectionStringBuilder();
            conBuilder.ConnectionString = connectionString;
            _storageSharedKeyCredential = new StorageSharedKeyCredential(conBuilder["AccountName"] as string, conBuilder["AccountKey"] as string);

        }
        
        public string BuildUploadUrl(string mediaItemId, string mediaItemFileName, string mediaKey) {
            string blobUrlPart = $"{mediaItemId}/{mediaKey}/{mediaItemFileName}";
            var blobSasBuilder = new BlobSasBuilder
            {
                StartsOn = DateTime.UtcNow - TimeSpan.FromMinutes(5),
                ExpiresOn = DateTime.UtcNow.AddHours(2),
                BlobContainerName = _blobContainerName,
                BlobName = blobUrlPart
            };

            //  Defines the type of permission.
            blobSasBuilder.SetPermissions(BlobSasPermissions.Write);
            Uri _uri = new Uri(_blobContainerClient.Uri,_blobContainerName +"/"+ blobUrlPart);
            BlobUriBuilder sasUri = new BlobUriBuilder(_uri)
            {
                Query = blobSasBuilder.ToSasQueryParameters(_storageSharedKeyCredential).ToString()
            };
            return sasUri.ToUri().ToString();
        }

        public bool DeleteMediaItemContent(string mediaItemId, string mediaItemFileName, string mediaKey) {
            string blobUrlPart = $"{mediaItemId}/{mediaKey}/{mediaItemFileName}";
            return  _blobContainerClient.DeleteBlobIfExists(blobUrlPart).Value;
        }
        public string BuildDownloadUrl(string mediaItemId, string mediaItemFileName, string mediaKey) {
            string blobUrlPart = $"/{mediaItemId}/{mediaKey}/{mediaItemFileName}";
            return _blobContainerClient.Uri.AbsoluteUri + blobUrlPart;
        }
   }

   public class MediaBlobStorageService:BlobStorageService {

       public MediaBlobStorageService(string connectionString):base(connectionString, "media") {

       }
   }
      public class PictureBlobStorageService:BlobStorageService {

       public PictureBlobStorageService(string connectionString):base(connectionString, "picture") {
           
       }
   }
}