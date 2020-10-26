using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpFlow.Data.Administration;
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
            var fileName = @"F:\ColdStorage\Documents\OpFlow\Imports\LomaLinda_Schedule1.csv";
            var importTypeId = 1; // schedule 
            //var importTypeId = 2; // item master
            //var importTypeId = 7; // schedule without card
            //var importTypeId = 5; // cards
            //var importTypeId = 3; // trays


            var sqlHelper = new SqlHelper();
            //var secureSqlHelper = new SecureSqlHelper("SecureConnection");
            var secureSqlHelper = new SecureSqlHelper("InvalidConnection");

            //var user = await sqlHelper.GetSecureUser(null, 4); // UNC
            //var user = await sqlHelper.GetSecureUser(null, 459); // LMC
            var user = await sqlHelper.GetSecureUser(null, 831); // UAB

            int? logId = null;


            var testA = ImportSurgeon.ParseSurgeon("Atallah, Ihab Nader Tawfik MD, PhD");
            var testB = ImportSurgeon.ParseSurgeon("Cho, Do-Yeon MD");

            try
            {
                var fileContents = File.ReadAllBytes(fileName);

                var fileParser = new FileParser(fileName, fileContents);

                await fileParser.ParseFile(sqlHelper, importTypeId, user.SelectedLocation);

                foreach (var record in fileParser.Records)
                {
                    if (record is ScheduleImport schedule)
                    {
                        var surgeon = schedule.PrimarySurgeon;

                        if (schedule.PrimarySurgeon.FirstName.Trim().Contains(' '))
                        {
                            schedule.PrimarySurgeon.FirstName = schedule.PrimarySurgeon.FirstName.Trim().Split(' ')[0];
                        }

                        foreach (var procedureCard in schedule.ProcedureCards)
                        {
                            var card = procedureCard.ImportSurgeon;
                        }
                    }
                }

                if (importTypeId == 5)
                {
                    await sqlHelper.UpdateCardItemImport(fileParser.Records.Select(r => r as CardImport).ToList(),
                        fileParser.Relations, user.SelectedLocation);
                }
                else
                {
                    logId = await sqlHelper.InsertImportLog(user.SelectedLocation, importTypeId, user.UserID, fileParser.Records.Count, fileName);

                    foreach (var record in fileParser.Records)
                    {
                        var secureId = await secureSqlHelper.InsertStagingData(record, user.UserID, "TEST", "TEST", 1);

                        var result = await sqlHelper.InsertStagingData(user.SelectedLocation, secureId, record, fileParser.Relations);
                        foreach (var message in result.Messages)
                        {
                            await sqlHelper.InsertImportMessage(user.SelectedLocation, logId.Value, "WARN", message,
                                (record as ScheduleImport)?.MRN, (record as ScheduleImport)?.ScheduleDate);
                        }
                    }
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
                var flowImageId = 0;

                var fileContents = File.ReadAllBytes(fileName);
                var sqlHelper = new SqlHelper();

                var user = await sqlHelper.GetSecureUser(null, 1);

                flowImageId = await sqlHelper.NewFlowImage(1, 1, 1, "1", 1, 1);
                    
                var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.FlowImages, 1);

                var storageHelper = BlobStorageHelper.GetHelper(user);

                await storageHelper.PutBlobBytes(folder, flowImageId.ToString(), fileContents);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
