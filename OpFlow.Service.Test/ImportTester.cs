using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpFlow.Service.Controllers;
using OpFlow.Service.DataAccess;
using OpFlow.Service.Models;

namespace OpFlow.Service.Test
{
    [TestClass]
    public class ImportTester
    {
        [TestMethod]
        public async Task TestImportFile()
        {
            var fileName = @"C:\temp\TestImportUNC.csv";
            var importTypeId = 1;

            var user = SqlHelper.GetSecureUser(null, 1);

            try
            {
                var fileContents = File.ReadAllBytes(fileName);

                var fileParser = new FileParser(fileName, fileContents);

                await fileParser.ParseFile(importTypeId, 1, 1);

                if (fileParser.Records != null)
                {
                    foreach (var record in fileParser.Records)
                    {

                        var secureId = await SecureSqlHelper.InsertStagingData(record, user.UserID, "TEST", "TEST", 1, user.DatabaseName);
                        await SqlHelper.InsertStagingData(user.ProviderID, user.LocationID, secureId, record, fileParser.Relations);
                    }

                    SqlHelper.InsertImportLog(user.ProviderID, user.LocationID, importTypeId, user.UserID, fileParser.Records.Count, fileName);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        [TestMethod]
        public async Task TestImportFlowImage()
        {
            try
            {

                var fileName = @"C:\temp\sample_image.jpg";
                var importTypeId = 5;
                var flowImageId = 0;

                var fileContents = File.ReadAllBytes(fileName);

                var user = SqlHelper.GetSecureUser(null, 1);

                flowImageId = SqlHelper.NewFlowImage(1, 1, 1, "1", 1, 1);
                    
                var folder = DataAccess.BlobStorageHelper.Folder(BlobStorageHelper.ImageType.FlowImages, 1);
                await DataAccess.BlobStorageHelper.PutBlobBytes(folder, flowImageId.ToString(), fileContents);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
