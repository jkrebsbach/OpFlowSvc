using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
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
        public async Task TestReadPDF()
        {
            var path = @"F:\ColdStorage\Downloads\drive-download-20201221T151800Z-001\";

            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                var files = Directory.GetFiles(directory);

                var cardImport = new List<CardImport>();
                foreach (var file in files)
                {
                    var cardData = ProcessFile(file);
                    cardImport.AddRange(cardData);
                }

                var outputData = "CardName,Manufacturer,Description,ProductNbr,Quantity\r\n";
                foreach (var cardItem in cardImport)
                {
                    // double quote issue, quick hack
                    if (cardItem.ProductNbr == string.Empty)
                        cardItem.ProductNbr = " ";

                    if (cardItem.Quantity == "")
                        continue;

                    outputData += $"\"{cardItem.CardName}\",\"{cardItem.Manufacturer} \",\"{cardItem.Description.Replace("\"", "\"\"")}\",\"{cardItem.ProductNbr}\",{cardItem.Quantity}\r\n";
                }

                var filename = $"{Path.GetFileName(directory)}_CardData.csv";
                var outputFile = Path.Combine(path, filename);
                File.WriteAllText(outputFile, outputData);
            }
        }

        private List<CardImport> ProcessFile(string filePath)
        {
            var cardName = Path.GetFileNameWithoutExtension(filePath);

            var pdfPageText = new List<string>();
            var cardImport = new List<CardImport>();

            using (var doc = new Doc())
            {
                doc.Read(filePath);

                int theCount = doc.PageCount;
                for (int i = 1; i <= theCount; i++)
                {
                    doc.PageNumber = i;
                    pdfPageText.Add(doc.GetText(Page.TextType.Svg, false));
                }
            }

            foreach (var pageText in pdfPageText)
            {
                var document = XDocument.Parse(pageText);

                var cardNameDetail = cardName;
                var manufacturer = string.Empty;
                var productNbr = string.Empty;
                var description = string.Empty;
                var manufacturerDelim = string.Empty;
                var productNbrDelim = string.Empty;
                var descriptionDelim = string.Empty;
                var quantity = string.Empty;


                foreach (var element in document.Root.Elements())
                {
                    if (element.Name.LocalName == "g")
                    {
                        foreach (var graphic in element.Elements())
                        {
                            if (graphic.Name.LocalName == "text")
                            {
                                var xLoc = graphic.Attribute("x").Value;
                                var yLoc = graphic.Attribute("y").Value;

                                //if (xLoc == "208.436" && yLoc == "46.141")
                                if (yLoc == "46.141")
                                {
                                    cardNameDetail = graphic.Value;
                                    var cardNameRegex = Regex.Match(cardNameDetail, @"([A-Za-z\s]+) - 000");
                                    if (cardNameRegex.Success)
                                        cardNameDetail = cardNameRegex.Groups[1].Value;
                                }

                                if (xLoc == "36.915")
                                {
                                    manufacturer += $"{manufacturerDelim}{graphic.Value}";
                                    manufacturerDelim = " ";
                                }

                                if (xLoc == "176.16")
                                {
                                    description += $"{descriptionDelim}{graphic.Value}";
                                    descriptionDelim = " ";
                                }

                                if (xLoc == "74.19")
                                {
                                    productNbr += $"{productNbrDelim}{graphic.Value}";
                                    productNbrDelim = " ";
                                }

                                if (xLoc == "481.83")
                                {
                                    quantity = graphic.Value;

                                    cardImport.Add(new CardImport()
                                    {
                                        CardName = cardNameDetail,
                                        Manufacturer = manufacturer.Trim(),
                                        Description = description.Trim(),
                                        ProductNbr = productNbr.Trim(),
                                        Quantity = quantity
                                    });

                                    manufacturer = string.Empty;
                                    description = string.Empty;
                                    productNbr = string.Empty;
                                    manufacturerDelim = string.Empty;
                                    productNbrDelim = string.Empty;
                                    descriptionDelim = string.Empty;
                                }
                            }
                        }
                    }
                }
            }

            return cardImport;
        }

        private class CardImport
        {
            public string CardName { get; set; }
            public string Manufacturer { get; set; }
            public string Description { get; set; }
            public string ProductNbr { get; set; }
            public string Quantity { get; set; }
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
