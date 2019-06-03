using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpFlow.PDF;
using OpFlow.Service.Controllers;
using OpFlow.Service.DataAccess;

namespace OpFlow.Service.Test
{
    [TestClass]
    public class ImagesTester
    {
        [TestMethod]
        public async Task TestUploadPatientPosition()
        {
            try
            {
                Dictionary<int, string> images = new Dictionary<int, string>();
                images[19] = "Trendelenburg";
                images[20] = "Wilson Frame";

                foreach (int image in images.Keys)
                {
                    var positionBinary = File.ReadAllBytes($@"C:\temp\OpFlow\{images[image]}.png");
                    

                    //ImageController cont = new ImageController();
                    //await cont.GetPatientPositionImage(image);

                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        [TestMethod]
        public async Task TestGetPatientPosition()
        {
            try
            {
                var patientId = 1;

                ImageController cont = new ImageController();
                var result = await cont.GetPatientPositionImage(patientId);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        [TestMethod]
        public async Task TestTraySummary()
        {
            try
            {
                WebSupergoo.ABCpdf11.XSettings.InstallLicense(Licensing.ABCPDF);
                var patientId = 1;

                var sqlHelper = new SqlHelper("OpFlowConnection");

                var trayProposalId = 1;
                var user = new Data.UserSecurity()
                {
                    ProviderID = 1,
                    LocationID = 1
                };

                var proposedTray = (await sqlHelper.GetProposedTrays(trayProposalId, user.ProviderID, user.LocationID)).FirstOrDefault();
                var instruments = await sqlHelper.GetProposedTrayInstruments(trayProposalId, true, user.ProviderID, user.LocationID);
                var audits = await sqlHelper.GetProposedTrayAudits(trayProposalId, null, null, user.ProviderID, user.LocationID);
                var counts = await sqlHelper.GetProposedTrayCounts(trayProposalId, null, null, user.ProviderID, user.LocationID);
                var sourceTrays = await sqlHelper.GetSourceTraySummary(trayProposalId, user.ProviderID, user.LocationID);
                var cardOverlaps = await sqlHelper.GetProposedTrayCardOverlap(trayProposalId, user.ProviderID, user.LocationID);

                var traySummary = new TraySummary()
                {
                    ProposedTray = proposedTray,
                    Audits = audits,
                    Counts = counts,
                    Instruments = instruments,
                    SourceTrays = sourceTrays,
                    Cards = cardOverlaps
                };
                var logoImage = @"C:\temp\opflow_logo.png";
                var imageBytes = ApprovalSummary.GenerateSummaryPDF(traySummary, logoImage);

                File.WriteAllBytes(@"C:\temp\test.pdf", imageBytes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
