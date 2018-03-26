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
    }
}
