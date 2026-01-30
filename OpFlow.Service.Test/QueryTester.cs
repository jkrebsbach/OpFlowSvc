using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpFlow.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpFlow.Service.Test
{
    [TestClass]
    public class QueryTester
    {
        [TestMethod]
        public async Task TestMethod()
        {
            //var user = new UserSecurity()
            //{
            //    ProviderID = 1,
            //    LocationID = 1
            //};

            //var sqlHelper = new SqlHelper();

            //var profile = await sqlHelper.GetProcedureProfile(2, null);

            //var comparableTrays = new List<int>();


            //foreach (var instrument in profile.TrayItems.OrderBy(ti => ti.Category).ThenBy(ti => ti.ItemDescription))
            //{
            //    var comparables = instrument.ComparableInstruments.Where(ci => ci.LocationID == 9);
            //    comparableTrays.AddRange(comparables.Select(c => c.RelatedTrayItemID));
            //}

            //var result = "Comparable Tray,Unmapped Instrument, Quantity\r\n";
            //var trayInstruments = await sqlHelper.GetTrayItemsInternal(comparableTrays);
            //foreach (var instrument in trayInstruments.OrderBy(ti => ti.TrayName).ThenBy(ti => ti.InstrumentName))
            //{
            //    if (!profile.TrayItems.Any(ti => ti.ComparableInstruments.Any(i => i.RelatedInstrumentID == instrument.InstrumentID)))
            //    {
            //        result += $"\"{instrument.TrayName.Replace("\"", "\"\"")}\",\"{instrument.ItemDescription.Replace("\"", "\"\"")}\",{instrument.Quantity}\r\n";
            //    }
            //}

        }
    }
}
