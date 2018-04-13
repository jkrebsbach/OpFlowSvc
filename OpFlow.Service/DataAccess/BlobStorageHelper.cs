using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace OpFlow.Service.DataAccess
{
    public static class BlobStorageHelper
    {
        private const string CONTAINER_NAME = "images";
        private static CloudStorageAccount _storageAccount;

        private static CloudStorageAccount StorageAccount => _storageAccount ?? (_storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.ConnectionStrings["BlobStorageConnection"].ConnectionString));


        public static async Task<List<string>> ListBlobs(string folder)
        {
            BlobContinuationToken continuationToken = null;
            var results = new List<IListBlobItem>();
            do
            {
                var response = await Container.ListBlobsSegmentedAsync(folder, continuationToken);
                continuationToken = response.ContinuationToken;
                results.AddRange(response.Results);
            }
            while (continuationToken != null);


            var filenames = new List<string>();
            foreach (var response in results)
            {
                filenames.Add(response.Uri.ToString());
            }

            return filenames;
        }

        public static async Task<byte[]> GetBlobBytes(string folder, string filename)
        {
            var filepath = Path.Combine(folder, filename);

            var memStream = new MemoryStream();

            try
            {
                var blockBlob = await Container.GetBlobReferenceFromServerAsync(filepath);
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

        public static async Task PutBlobBytes(string folder, string filename, byte[] bytes)
        {
            var filepath = Path.Combine(folder, filename);

            var blockBlob = Container.GetBlockBlobReference(filepath);

            var memStream = new MemoryStream(bytes);
            await blockBlob.UploadFromStreamAsync(memStream);
        }

        public static async Task DeleteBlob(string folder, string filename)
        {
            var filepath = Path.Combine(folder, filename);

            try
            {
                var blockBlob = await Container.GetBlobReferenceFromServerAsync(filepath);
                await blockBlob.DeleteAsync();
            }
            catch (StorageException se)
            {
                // If already deleted, or no matching file, take no action
                if (se.Message.Contains("404") || se.Message.Contains("Not Found"))
                {
                    return;
                }

                throw;
            }
        }

        private static CloudBlobContainer Container 
        {
            get
            {
                var blobClient = StorageAccount.CreateCloudBlobClient();
                return blobClient.GetContainerReference(CONTAINER_NAME);
            }
        }
    

        public static string Folder(int providerId, int cardId, int flowId, int stepId, int roleId)
        {
            var folder = string.Format($"{providerId}_{cardId}_{flowId}_{stepId}_{roleId}");

            return folder;
        }
    }
}