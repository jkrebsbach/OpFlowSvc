using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.WindowsAzure.Storage;

namespace OpFlow.Service.DataAccess
{
    public static class BlobStorageHelper
    {
        private const string CONTAINER_NAME = "images";
        private static CloudStorageAccount _storageAccount;

        private static CloudStorageAccount StorageAccount => _storageAccount ?? (_storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.ConnectionStrings["BlobStorageConnection"].ConnectionString));

        public static async Task<byte[]> GetBlobBytes(int providerId, int surgeryId, int cardId, int flowId)
        {
            var blobClient = StorageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(CONTAINER_NAME);

            var filename = string.Format($"{providerId}_{surgeryId}_{cardId}_{flowId}");

            var memStream = new MemoryStream();

            try
            {
                var blockBlob = await container.GetBlobReferenceFromServerAsync(filename);
                blockBlob.DownloadToStream(memStream);
            }
            catch (StorageException se)
            {
                if (se.Message.Contains("404") || se.Message.Contains("Not Found"))
                {
                    return null;
                }

                throw;
            }


            return memStream.ToArray();
        }

        public static async Task PutBlobBytes(int providerId, int surgeryId, int cardId, int flowId, byte[] bytes)
        {
            var memStream = new MemoryStream(bytes);

            var blobClient = StorageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(CONTAINER_NAME);

            var filename = string.Format($"{providerId}_{surgeryId}_{cardId}_{flowId}");

            var blockBlob = await container.GetBlobReferenceFromServerAsync(filename);

            await blockBlob.UploadFromStreamAsync(memStream);
        }
    }
}