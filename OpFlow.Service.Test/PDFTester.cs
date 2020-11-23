using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpFlow.Service.DataAccess;
using WebSupergoo.ABCpdf11;
using WebSupergoo.ABCpdf11.Objects;

namespace OpFlow.Service.Test
{
    [TestClass]
    public class PDFTester
    {
        [TestInitialize]
        public void Initialize()
        {
            WebSupergoo.ABCpdf11.XSettings.InstallLicense(Licensing.ABCPDF);
        }


        [TestMethod]
        public async Task ReadPDF()
        {
            var path = @"F:\ColdStorage\Documents\OpFlow\Inova\TOR_Stryker_RemB_Power_Set.pdf";

            var pdfText = string.Empty;

            using (var doc = new Doc())
            {
                doc.Read(path);

                int theCount = doc.PageCount;
                for (int i = 1; i <= theCount; i++)
                {
                    doc.PageNumber = i;
                    pdfText += doc.GetText(Page.TextType.Svg, true);
                }
            }

            foreach (var itemLine in pdfText.Split('\n'))
            {
                var itemMatch = Regex.Match(itemLine, @"([A-Za-z\s]), ([A-Za-z]), ([A-Za-z#\s0-9])\s+[01]\s[01]\s[01]");

                if (itemMatch.Success)
                {
                    var abc = itemMatch.Groups[1];
                }
            }
        }

        [TestMethod]
        public async Task TestTraySummary()
        {
            try
            {
                var sqlHelper = new SqlHelper();

                var trayProposalId = 1;
                var user = new Data.UserSecurity()
                {
                    ProviderID = 1,
                    LocationID = 1
                };

                var proposedTray = (await sqlHelper.GetProposedTrays(trayProposalId, user.SelectedLocation)).FirstOrDefault();
                var instruments = await sqlHelper.GetProposedTrayInstruments(trayProposalId, user.SelectedLocation);
                var audits = await sqlHelper.GetProposedTrayAudits(trayProposalId, null, null, user.SelectedLocation);
                var counts = await sqlHelper.GetProposedTrayCounts(trayProposalId, null, null, user.SelectedLocation);
                var sourceTrays = await sqlHelper.GetSourceTraySummary(trayProposalId, user.SelectedLocation);
                var cardOverlaps = await sqlHelper.GetProposedTrayCardOverlap(trayProposalId, user.SelectedLocation);

                //var traySummary = new TraySummary()
                //{
                //    ProposedTray = proposedTray,
                //    Audits = audits,
                //    Counts = counts,
                //    Instruments = instruments,
                //    SourceTrays = sourceTrays,
                //    Cards = cardOverlaps
                //};
                //var logoImage = @"C:\temp\opflow_logo.png";
                //var imageBytes = ApprovalSummary.GenerateSummaryPDF(traySummary, logoImage);

                byte[] imageBytes = new byte[0];
                File.WriteAllBytes(@"C:\temp\test.pdf", imageBytes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        [TestMethod]
        public async Task TestChratPDF()
        {
            try
            {
                var memoryStream = new MemoryStream();

                using (Doc pdfDoc = new Doc())
                {
                    pdfDoc.HtmlOptions.Engine = EngineType.Gecko;
                    pdfDoc.FontSize = 12;

                    pdfDoc.Rect.Left = 40;
                    pdfDoc.Rect.Bottom = 40;
                    pdfDoc.Rect.Width = 550;
                    pdfDoc.Rect.Height = 650;

                    var chartUrl = "https://www.chartjs.org/samples/latest/charts/bar/horizontal.html";
                    var chainId = pdfDoc.AddImageUrl(chartUrl);
                    //pdfDoc.AddImageHtml(imageHtml, true, 700, false);

                    while (true)
                    {
                        //pdfDoc.FrameRect();
                        if (!pdfDoc.Chainable(chainId))
                            break;

                        pdfDoc.Page = pdfDoc.AddPage();
                        chainId = pdfDoc.AddImageToChain(chainId);
                    }

                    pdfDoc.Save(memoryStream);
                    pdfDoc.Clear();
                }

                var imageBytes = memoryStream.GetBuffer();
                
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
