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
            var fileName = @"C:\temp\test.csv";
            var importTypeId = 5;

            var user = SqlHelper.GetSecureUser(null, 1);

            try
            {
                var fileContents = File.ReadAllBytes(fileName);

                var fileParser = new FileParser(fileName, fileContents);

                var records = fileParser.ParseFile(importTypeId);

                if (records != null)
                {
                    foreach (var record in records)
                    {

                        var secureId = await SecureSqlHelper.InsertStagingData(record, user.DatabaseName);
                        SqlHelper.InsertStagingData(user.ProviderID, user.LocationID, secureId, record);
                    }

                    SqlHelper.InsertImportLog(user.ProviderID, user.LocationID, importTypeId, user.UserID, records.Count, fileName);
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

                var user = SqlHelper.GetSecureUser("ben@opflowtech.com");

                flowImageId = SqlHelper.NewFlowImage(1, 1, 1, 1, 1);
                    
                var folder = DataAccess.BlobStorageHelper.Folder(user.ProviderID, 1, 0, 0, 0);
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
