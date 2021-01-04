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
            var path = @"F:\ColdStorage\Documents\OpFlow\Imports\PDFs\";

            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                var files = Directory.GetFiles(directory);

                foreach (var file in files)
                {
                    var cardImport = new List<CardImport>();
                
                    var cardData = ProcessFile(file);
                    cardImport.AddRange(cardData);
                
                    var outputData = "CardName,Manufacturer,Description,ProductNbr,Quantity\r\n";
                    foreach (var cardItem in cardImport)
                    {
                        // double quote issue, quick hack
                        if (cardItem.ProductNbr == string.Empty)
                            cardItem.ProductNbr = " ";

                        //if (cardItem.Quantity == "")
                        //    continue;

                        outputData += $"\"{cardItem.CardName}\",\"{cardItem.Manufacturer} \",\"{cardItem.Description.Replace("\"", "\"\"")}\",\"{cardItem.ProductNbr}\",{cardItem.Quantity}\r\n";
                    }

                    var filename = $"{Path.GetFileName(file)}_CardData.csv";
                    var outputFile = Path.Combine(path, filename);
                    File.WriteAllText(outputFile, outputData);
                }
            }
        }

        private List<CardImport> ProcessFile(string filePath)
        {
            var cardName = Path.GetFileNameWithoutExtension(filePath);

            var pdfPageText = new List<string>();
            var fileImport = new List<CardImport>();

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
                var cardImport = new List<CardImport>();
                var document = XDocument.Parse(pageText);

                var cardNameDetail = string.Empty;
                var manufacturer = string.Empty;
                var productNbr = string.Empty;
                var description = string.Empty;
                var quantity = string.Empty;
                var manufacturerDelim = string.Empty;
                var productNbrDelim = string.Empty;
                var descriptionDelim = string.Empty;
                var quantityDelim = string.Empty;

                decimal? descriptionPosition = null;
                decimal? manufacturerPosition = null;
                decimal? quantityPosition = null;

                foreach (var element in document.Root.Elements())
                {
                    if (element.Name.LocalName == "g")
                    {
                        foreach (var graphic in element.Elements())
                        {
                            if (graphic.Name.LocalName == "text")
                            {
                                var xLoc = decimal.Parse(graphic.Attribute("x").Value);
                                var yLoc = decimal.Parse(graphic.Attribute("y").Value);

                                var fontSize = graphic.Attribute("font-size").Value;
                                var fontFamily = graphic.Attribute("font-family").Value;

                                var data = ParseToken(graphic);

                                //if (xLoc == "208.436" && yLoc == "46.141")
                                if (fontFamily == "CIDFont+F1" && cardNameDetail == string.Empty)
                                {
                                    cardNameDetail = data;
                                }

                                if (descriptionPosition != null && xLoc == descriptionPosition)
                                {
                                    description += $"{descriptionDelim}{data}";
                                    descriptionDelim = " ";
                                }

                                if (data == "DESCRIPTION")
                                {
                                    descriptionPosition = xLoc;
                                }

                                if (manufacturerPosition != null && xLoc == manufacturerPosition)
                                {
                                    manufacturer += $"{manufacturerDelim}{data}";
                                    manufacturerDelim = " ";
                                }

                                if (data == "CATALOG")
                                {
                                    manufacturerPosition = xLoc;
                                }

                                if (xLoc == 999999.99M)
                                {
                                    productNbr += $"{productNbrDelim}{data}";
                                    productNbrDelim = " ";
                                }

                                if (xLoc > quantityPosition && description != string.Empty) 
                                {
                                    if (data == "CNT1" || data == "CNT2") continue;

                                    quantity = data;

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

                                if (data == "QTY")
                                {
                                    quantityPosition = xLoc;
                                }
                            }
                        }
                    }
                }

                // Some pages are items and not trays
                if (!cardImport.Any())
                {
                    if (cardNameDetail == string.Empty) throw new Exception("Unable to parse card" + filePath);

                    cardImport.Add(new CardImport()
                    {
                        CardName = cardNameDetail,
                        Manufacturer = manufacturer.Trim(),
                        Description = description.Trim(),
                        ProductNbr = productNbr.Trim(),
                        Quantity = quantity
                    });
                }

                fileImport.AddRange(cardImport);
            }

            return fileImport;
        }

        private string ParseToken(XElement root)
        {
            var result = string.Empty;

            var elements = root.Elements();
            if (elements.Count() == 0) return root.Value;

            foreach (var element in elements)
            {
                result += element.Value;
            }
            return result;
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
