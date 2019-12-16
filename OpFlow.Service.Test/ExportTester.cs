using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpFlow.Service.Test
{
    [TestClass]
    public class ExportTester
    {
        [TestMethod]
        public async Task TestExportCsv()
        {
            var user = new User()
            {
                ProviderID = 1,
                LocationID = 1
            };
            var sqlHelper = new SqlHelper();

            var proposedTrays = await sqlHelper.GetProposedTrays(null, user.ProviderID, user.LocationID);

            var extract = "Tray Name, Source Tray, # Instruments, Service Line, Categories\r\n";

            foreach (var proposedTray in proposedTrays)
            {
                var proposedInstruments = await sqlHelper.GetProposedTrayInstruments(proposedTray.TrayProposalID, user.ProviderID, user.LocationID);
                var sourceTrays = await sqlHelper.GetSourceTraySummary(proposedTray.TrayProposalID, user.ProviderID, user.LocationID);

                foreach (var sourceTray in sourceTrays)
                {
                    extract += $"\"{proposedTray.TrayName?.Trim().Replace("\"", "\"\"")}\",{sourceTray.TrayName},{proposedInstruments.Sum(p => p.Quantity)},{proposedTray.Specialty},\"{sourceTray.CardCategories?.Trim().Replace("\"", "\"\"")}\"\r\n";
                }
            }
            
            File.WriteAllText(@"C:\temp\export.csv", extract);
        }
    }
}
