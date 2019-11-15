using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace OpFlow.Service.DataAccess
{
    public class BlobStorageHelper
    {
        private string _containerName;
        private CloudStorageAccount _storageAccount;

        public enum ImageType
        {
            PatientPosition,
            ApprovalImages,
            FlowImages,
            RoomSetupImages,
            SurgeryImages,
            TrayProposalImages,
            ImplementationImages
        }

        private BlobStorageHelper(string connectionString, string containerName)
        {
            _storageAccount = CloudStorageAccount.Parse(connectionString);
            _containerName = containerName;
        }

        public static BlobStorageHelper GetHelper(Data.UserSecurity secureUser)
        {
            var result = new BlobStorageHelper(secureUser.BlobKey, secureUser.BlobContainer);

            return result;
        }

        public async Task<byte[]> GetBlobBytes(string folder, string filename)
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

        public async Task<bool> BlobExists(string folder, string filename)
        {
            var filepath = Path.Combine(folder, filename);

            try
            {
                var blockBlob = await Container.GetBlobReferenceFromServerAsync(filepath);
                return await blockBlob.ExistsAsync();
            }
            catch (StorageException se)
            {
                if (se.Message.Contains("404") || se.Message.Contains("Not Found"))
                {
                    return false;
                }

                throw;
            }
        }

        public async Task RotateImage(string folder, string filename, int direction)
        {
            var filepath = Path.Combine(folder, filename);

            var memStream = new MemoryStream();
            ICloudBlob blockBlob;

            try
            {
                blockBlob = await Container.GetBlobReferenceFromServerAsync(filepath);
                blockBlob.DownloadToStream(memStream);
            }
            catch (StorageException se)
            {
                if (se.Message.Contains("404") || se.Message.Contains("Not Found"))
                {
                    return;
                }

                throw;
            }

            var outStream = new MemoryStream();
            memStream.Position = 0;

            using (var img = Image.FromStream(memStream))
            {
                switch (direction)
                {
                    case 1:
                        img.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case -1:
                        img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                }

                img.Save(outStream, ImageFormat.Png);
            }

            outStream.Position = 0;
            if (outStream.Length > 0)
            {
                await blockBlob.UploadFromStreamAsync(outStream);
            }
        }

        public async Task PutBlobBytes(string folder, string filename, byte[] bytes)
        {
            var filepath = Path.Combine(folder, filename);

            var blockBlob = Container.GetBlockBlobReference(filepath);

            var memStream = new MemoryStream(bytes);
            await blockBlob.UploadFromStreamAsync(memStream);
        }

        public async Task DeleteBlob(string folder, string filename)
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

        private CloudBlobContainer Container 
        {
            get
            {
                var blobClient = _storageAccount.CreateCloudBlobClient();
                return blobClient.GetContainerReference(_containerName);
            }
        }
    

        public static string Folder(ImageType imageType, int containerId)
        {
            return $"{imageType}/{containerId}";
        }

        public static byte[] CompressImage(byte[] sourceImage)
        {
            var memStream = new MemoryStream(sourceImage);
            var outStream = new MemoryStream();
            memStream.Position = 0;

            using (var img = Image.FromStream(memStream))
            {
                var jpgEncoder = GetEncoder(ImageFormat.Jpeg);

                // Create an Encoder object based on the GUID
                // for the Quality parameter category.
                var myEncoder = Encoder.Quality;
                var encoderParams = new EncoderParameters(1);
                var encoderParameter = new EncoderParameter(myEncoder, 25L);
                encoderParams.Param[0] = encoderParameter;

                img.Save(outStream, jpgEncoder, encoderParams);
            }

            outStream.Position = 0;

            return outStream.GetBuffer();
        }
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();

            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}