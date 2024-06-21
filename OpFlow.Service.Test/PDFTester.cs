using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPOI.XWPF.UserModel;
using OpFlow.Data;
using OpFlow.Data.Administration;
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
            var path = @"D:\ColdStorage\Documents\OpFlow\Imports\20230701\";

            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                var files = Directory.GetFiles(directory, "*.pdf");

                var outputData = "CardName,Manufacturer,Description,ProductNbr,Quantity\r\n";

                foreach (var file in files)
                {
                    var cardImport = new List<CardImport>();
                
                    var cardData = ProcessFile(file);
                    cardImport.AddRange(cardData);
                
                    foreach (var cardItem in cardImport)
                    {
                        // double quote issue, quick hack
                        if (cardItem.ProductNbr == string.Empty)
                            cardItem.ProductNbr = " ";

                        //if (cardItem.Quantity == "")
                        //    continue;

                        outputData += $"\"{cardItem.CardName}\",\"{cardItem.Manufacturer} \",\"{cardItem.Description.Replace("\"", "\"\"")}\",\"{cardItem.ProductNbr}\",{cardItem.Quantity}\r\n";
                    }
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

                var tableHeader = 0.0M;

                bool newRow = false;
                decimal curPos = 0.0M;

                decimal? preCountPosition = null;
                decimal? descriptionPosition = null;
                decimal? manufacturerPosition = null;
                decimal? productNumPosition = null;
                decimal? quantityPosition = null;

                //  Multiple columns starting with "qty"
                var qtyCandidates = new List<decimal>();

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
                                if (fontFamily == "SegoeUI,Bold" && fontSize == "10")
                                {
                                    // instrument type grouping - skip row
                                    continue;
                                }

                                if (fontFamily == "SegoeUI,Bold" && fontSize == "11" && cardNameDetail == string.Empty)
                                {
                                    cardNameDetail = data.Replace(" - 000", "");
                                    continue;
                                }

                                if (CheckRange(xLoc, descriptionPosition) && yLoc > tableHeader)
                                {
                                    description += $"{descriptionDelim}{data}";
                                    descriptionDelim = " ";

                                    newRow = CheckRow(yLoc, curPos);
                                    curPos = yLoc;
                                }

                                if (CheckRange(xLoc, manufacturerPosition) && yLoc > tableHeader)
                                {
                                    manufacturer += $"{manufacturerDelim}{data}";
                                    manufacturerDelim = " ";

                                    newRow = CheckRow(yLoc, curPos);
                                    curPos = yLoc;
                                }

                                if (CheckRange(xLoc, productNumPosition) && yLoc > tableHeader)
                                {
                                    productNbr += $"{productNbrDelim}{data}";
                                    productNbrDelim = " ";

                                    newRow = CheckRow(yLoc, curPos);
                                    curPos = yLoc;
                                }

                                if (CheckRange(xLoc, preCountPosition) && yLoc > tableHeader)
                                {
                                    newRow = CheckRow(yLoc, curPos);
                                    curPos = yLoc;
                                }

                                if (fontFamily != "SegoeUI,Bold" && CheckRange(xLoc, quantityPosition) && yLoc > tableHeader)
                                {
                                    quantity = data;

                                    newRow = CheckRow(yLoc, curPos);
                                    curPos = yLoc;
                                }

                                // Crazy margin buffer issue?!  subtracting 2 to get in the right area...
                                if (data == "Description")
                                {
                                    tableHeader = yLoc;

                                    descriptionPosition = xLoc - 2;
                                }

                                if (data == "Manuf")
                                {
                                    manufacturerPosition = xLoc - 2;
                                }

                                if (data == "Prod #")
                                {
                                    productNumPosition = xLoc - 2;
                                }

                                if (data == "Pre")
                                {
                                    preCountPosition = xLoc - 2;
                                }

                                if (data == "Qty")
                                {
                                    qtyCandidates.Add(xLoc);
                                }

                                if (data == "Rqd" && qtyCandidates.Contains(xLoc))
                                {
                                    quantityPosition = xLoc - 2;
                                }

                                //if (xLoc > productNumPosition && yLoc > tableHeader && description != string.Empty) 
                                if (newRow)
                                {
                                    if (description == string.Empty)
                                    {
                                        newRow = false;
                                        continue;
                                    }

                                    if (data == "CNT1" || data == "CNT2") continue;

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

                                    newRow = false;
                                }
                            }
                        }
                    }
                }

                if (cardNameDetail == string.Empty) 
                    throw new Exception("Unable to parse card" + filePath);

                // add remaining data
                if (description != string.Empty)
                {
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


        [TestMethod]
        public async Task TestReadCensitracPDF()
        {
            var path = @"D:\ColdStorage\Documents\OpFlow\Imports\20240605\";

            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                var files = Directory.GetFiles(directory, "*.pdf");

                var outputData = "CardName,Description,Catalog,Qty,Cnt1,Cnt2\r\n";

                foreach (var file in files)
                {
                    var cardImport = ProcessCensitracFile(file);

                    foreach (var cardItem in cardImport)
                    {
                        // double quote issue, quick hack
                        if (cardItem.ProductNbr == string.Empty)
                            cardItem.ProductNbr = " ";

                        //if (cardItem.Quantity == "")
                        //    continue;

                        outputData += $"\"{cardItem.CardName.Replace("\"", "\"\"")}\",\"{cardItem.Description.Replace("\"", "\"\"")}\",\"{cardItem.ProductNbr}\",{cardItem.Quantity},{cardItem.Open},{cardItem.Prn}\r\n";
                    }
                }

                var filename = $"{Path.GetFileName(directory)}_CardData.csv";
                var outputFile = Path.Combine(path, filename);
                File.WriteAllText(outputFile, outputData);
            }
        }
        private List<CardImport> ProcessCensitracFile(string filePath)
        {
            var pdfPageText = new List<string>();

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

            var cardImport = new List<CardImport>();
            string cardName = string.Empty;


            foreach (var pageText in pdfPageText)
            {
                var document = XDocument.Parse(pageText);

                var rootElements = document.Root.Elements().ToList();
                var textElements = rootElements.SelectMany(re => re.Elements().ToList()).Where(el => el.Name.LocalName == "text").ToList();

                cardName = string.Empty;
                decimal headerRow = 0;
                decimal procedureX = 0;
                decimal catalogX = 0;
                decimal qtyX = 0;
                decimal cnt1 = 0;
                decimal cnt2 = 0;

                foreach (var graphic in textElements)
                {
                    var data = ParseToken(graphic);

                    if (data == "") continue;

                    var fontSize = graphic.Attribute("font-size").Value;
                    var fontFamily = graphic.Attribute("font-family").Value;
                    var yPos = decimal.Parse(graphic.Attribute("y").Value);

                    if (fontSize == "9" && (cardName == string.Empty || yPos == headerRow))
                    {
                        cardName += data;
                        headerRow = yPos;
                    }

                    if (data.Trim() == "DESCRIPTION")
                    {
                        procedureX = decimal.Parse(graphic.Attribute("x").Value);
                    }
                    if (data.Trim() == "CA")
                    {
                        catalogX = decimal.Parse(graphic.Attribute("x").Value);
                    }

                    if (data == "QTY")
                        qtyX = decimal.Parse(graphic.Attribute("x").Value);

                    if (data == "CNT1")
                        cnt1 = decimal.Parse(graphic.Attribute("x").Value);

                    if (data == "CNT2")
                    {
                        cnt2 = decimal.Parse(graphic.Attribute("x").Value);
                        cardImport.AddRange(ParseCensitracTable(textElements, cardName, procedureX, catalogX, qtyX, cnt1, cnt2, yPos + 0.05M));
                    }
                }

            }

            if (cardName == string.Empty) throw new Exception("Unable to parse card" + filePath);

            return cardImport;
        }

        private List<CardImport> ParseCensitracTable(List<XElement> textElements, string cardName, decimal procedureX, decimal catalogX, decimal qtyX, decimal cnt1, decimal cnt2, decimal yPos)
        {
            var cardImport = new List<CardImport>();

            var description = string.Empty;
            var catalog = string.Empty;
            var quantity = string.Empty;
            var openValue = string.Empty;
            var prnValue = string.Empty;

            foreach (var graphic in textElements)
            {
                var fontSize = graphic.Attribute("font-size").Value;
                var fontFamily = graphic.Attribute("font-family").Value;

                var xLoc = decimal.Parse(graphic.Attribute("x").Value);
                var yLoc = decimal.Parse(graphic.Attribute("y").Value);

                if (yLoc < yPos)
                {
                    continue;
                }
                else if (yLoc > yPos && xLoc != procedureX)
                {
                    // this will be footer data, not a new row
                    continue;
                }

                if (yLoc > yPos)
                {
                    if (description != string.Empty)
                    {
                        cardImport.Add(new CardImport()
                        {
                            CardName = cardName.Trim(),
                            Description = description.Trim(),
                            ProductNbr = catalog.Trim(),
                            Quantity = quantity,
                            Open = openValue,
                            Prn = prnValue
                        });
                    }

                    description = string.Empty;
                    catalog = string.Empty;
                    quantity = string.Empty;
                    openValue = string.Empty;
                    prnValue = string.Empty;

                    yPos = yLoc;
                }   

                var data = ParseToken(graphic);

                if (xLoc < catalogX)
                {
                    description += data;
                    continue;
                }

                if (xLoc < qtyX)
                {
                    catalog += data;
                    continue;
                }

                if (xLoc < cnt1)
                {
                    quantity += data;
                    continue;
                }

                if (xLoc < cnt2)
                {
                    openValue += data;
                    continue;
                }

                prnValue += data;
            }

            // add remaining data
            if (description != string.Empty)
            {
                cardImport.Add(new CardImport()
                {
                    CardName = cardName.Trim(),
                    Description = description.Trim(),
                    ProductNbr = catalog.Trim(),
                    Quantity = quantity,
                    Open = openValue,
                    Prn = prnValue
                });
            }

            return cardImport;
        }


        [TestMethod]
        public async Task TestReadSaintLukesPDF()
        {
            var path = @"D:\ColdStorage\Documents\OpFlow\Imports\20240605\";

            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                var files = Directory.GetFiles(directory, "*.pdf");

                var outputData = "CardName,Description,Manuf,Prod,Comment,QtyRqd,QtyMi,SpdCnt\r\n";

                foreach (var file in files)
                {
                    var cardImport = ProcessSaintLukesFile(file);

                    foreach (var cardItem in cardImport)
                    {
                        // double quote issue, quick hack
                        if (cardItem.ProductNbr == string.Empty)
                            cardItem.ProductNbr = " ";

                        //if (cardItem.Quantity == "")
                        //    continue;

                        outputData += $"\"{cardItem.CardName.Replace("\"", "\"\"")}\",\"{cardItem.Description.Replace("\"", "\"\"")}\",\"{cardItem.Manufacturer}\",\"{cardItem.ProductNbr}\",\"{cardItem.Comment.Replace("\"", "\"\"")}\",{cardItem.Quantity},{cardItem.Open},{cardItem.Prn}\r\n";
                    }
                }

                var filename = $"{Path.GetFileName(directory)}_CardData.csv";
                var outputFile = Path.Combine(path, filename);
                File.WriteAllText(outputFile, outputData);
            }
        }
        private List<CardImport> ProcessSaintLukesFile(string filePath)
        {
            try
            {
                var pdfPageText = new List<string>();

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

                var cardImport = new List<CardImport>();
                string cardName = string.Empty;


                foreach (var pageText in pdfPageText)
                {
                    var document = XDocument.Parse(pageText);

                    var rootElements = document.Root.Elements().ToList();
                    var textElements = rootElements.SelectMany(re => re.Elements().ToList()).Where(el => el.Name.LocalName == "text").ToList();

                    cardName = string.Empty;
                    decimal titleRow = 0;
                    decimal headerRow = 0;
                    decimal preCnt = 0;
                    decimal qty1 = 0;
                    decimal qty2 = 0;
                    decimal spd = 0;
                    decimal description = 0;
                    decimal manufacturer = 0;
                    decimal prodNbr = 0;
                    decimal comment = 0;

                    foreach (var graphic in textElements)
                    {
                        var data = ParseToken(graphic);

                        if (data == "") continue;

                        var fontSize = graphic.Attribute("font-size").Value;
                        var fontFamily = graphic.Attribute("font-family").Value;
                        var yPos = decimal.Parse(graphic.Attribute("y").Value);

                        if (fontSize == "11" && (cardName == string.Empty || yPos == titleRow))
                        {
                            cardName += data;
                            headerRow = yPos;
                        }

                        if (data.Trim() == "Pre")
                        {
                            preCnt = decimal.Parse(graphic.Attribute("x").Value);
                        }
                        if (data.Trim() == "Qty" && qty1 == 0)
                        {
                            qty1 = decimal.Parse(graphic.Attribute("x").Value);
                        }
                        else if (data == "Qty" && qty1 != 0)
                            qty2 = decimal.Parse(graphic.Attribute("x").Value);

                        if (data == "SPD")
                            spd = decimal.Parse(graphic.Attribute("x").Value);
                        if (data == "Description")
                            description = decimal.Parse(graphic.Attribute("x").Value);
                        if (data == "Manuf")
                            manufacturer = decimal.Parse(graphic.Attribute("x").Value);
                        if (data == "Prod #")
                            prodNbr = decimal.Parse(graphic.Attribute("x").Value);
                        if (data == "Comment")
                        {
                            titleRow = yPos;
                            comment = decimal.Parse(graphic.Attribute("x").Value);
                        }
                    }

                    if (comment != 0 && description != 0)
                        cardImport.AddRange(ParseSaintLukesTable(textElements, cardName, preCnt, qty1, qty2, spd, description, manufacturer, prodNbr, comment, titleRow + 15.0M));

                }

                if (cardName == string.Empty) throw new Exception("Unable to parse card" + filePath);

                return cardImport;
            }
            catch(Exception ex)
            {
                var tmpInt = 0;
                return new List<CardImport>();
            }

            return null;
        }

        private List<CardImport> ParseSaintLukesTable(List<XElement> textElements, string cardName, decimal preCntX, decimal qty1X, decimal qty2X, decimal spdX, decimal descriptionX, decimal manufacturerX, decimal prodNbrX, decimal commentX, decimal yPos)
        {
            var cardImport = new List<CardImport>();

            var preCnt = string.Empty;
            var qty1 = string.Empty;
            var qty2 = string.Empty;
            var spdCnt = string.Empty;
            var description = string.Empty;
            var manufacturer = string.Empty;
            var prodNbr = string.Empty;
            var comment = string.Empty;

            foreach (var graphic in textElements)
            {
                var fontSize = graphic.Attribute("font-size").Value;
                var fontFamily = graphic.Attribute("font-family").Value;

                var xLoc = decimal.Parse(graphic.Attribute("x").Value);
                var yLoc = decimal.Parse(graphic.Attribute("y").Value);

                // ignore summary rows
                if (fontFamily == "SegoeUI,Bold")
                    continue;

                if (yLoc < yPos)
                {
                    continue;
                }
                
                if (yLoc > yPos + 12)
                {
                    if (description != string.Empty)
                    {
                        cardImport.Add(new CardImport()
                        {
                            CardName = cardName.Trim(),
                            Description = description.Trim(),
                            Manufacturer = manufacturer.Trim(),
                            Comment = comment.Trim(),
                            ProductNbr = prodNbr.Trim(),
                            Quantity = qty1,
                            Open = qty2,
                            Prn = spdCnt
                        });
                    }

                    preCnt = string.Empty;
                    qty1 = string.Empty;
                    qty2 = string.Empty;
                    spdCnt = string.Empty;
                    description = string.Empty;
                    manufacturer = string.Empty;
                    prodNbr = string.Empty;
                    comment = string.Empty;

                    yPos = yLoc;
                }

                var data = ParseToken(graphic);

                if (xLoc < preCntX && xLoc > preCntX - 5)
                {
                    preCnt += data;
                    continue;
                }

                if (xLoc < qty1X && xLoc > qty1X - 5)
                {
                    qty1 += data;
                    continue;
                }

                if (xLoc < qty2X && xLoc > qty2X - 5)
                {
                    qty2 += data;
                    continue;
                }

                if (xLoc < spdX && xLoc > spdX - 5)
                {
                    spdCnt += data;
                    continue;
                }

                if (xLoc < descriptionX && xLoc > descriptionX - 5)
                {
                    description += data;
                    continue;
                }

                if (xLoc < manufacturerX && xLoc > manufacturerX - 5)
                {
                    manufacturer += data;
                    continue;
                }

                if (xLoc < prodNbrX && xLoc > prodNbrX - 5)
                {
                    prodNbr += data;
                    continue;
                }

                if (xLoc < commentX && xLoc > commentX - 5)
                {
                    comment += data;
                }
            }

            // add remaining data
            if (description != string.Empty)
            {
                cardImport.Add(new CardImport()
                {
                    CardName = cardName.Trim(),
                    Description = description.Trim(),
                    Manufacturer = manufacturer.Trim(),
                    Comment = comment.Trim(),
                    ProductNbr = prodNbr.Trim(),
                    Quantity = qty1,
                    Open = qty2,
                    Prn = spdCnt
                });
            }

            return cardImport;
        }



        [TestMethod]
        public async Task TestReadCMCPDF()
        {
            var path = @"D:\ColdStorage\Documents\OpFlow\Imports\20240516\";

            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                var files = Directory.GetFiles(directory, "*.pdf");

                var outputData = "Surgeon,CardName,Manufacturer,Description,ProductNbr,Open,Prn\r\n";

                foreach (var file in files)
                {
                    var cardImport = ProcessCMCFile(file);

                    foreach (var cardItem in cardImport)
                    {
                        // double quote issue, quick hack
                        if (cardItem.ProductNbr == string.Empty)
                            cardItem.ProductNbr = " ";

                        //if (cardItem.Quantity == "")
                        //    continue;

                        outputData += $"\"{cardItem.SurgeonName}\",\"{cardItem.CardName}\",\"{cardItem.Manufacturer} \",\"{cardItem.Description.Replace("\"", "\"\"")}\",\"{cardItem.ProductNbr}\",{cardItem.Open},{cardItem.Prn}\r\n";
                    }
                }

                var filename = $"{Path.GetFileName(directory)}_CardData.csv";
                var outputFile = Path.Combine(path, filename);
                File.WriteAllText(outputFile, outputData);
            }
        }
        private List<CardImport> ProcessCMCFile(string filePath)
        {
            var pdfPageText = new List<string>();

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

            var cardImport = new List<CardImport>();

            var cardNameDetail = string.Empty;
            var surgeonName = string.Empty;

            var supplies = false;
            var instruments = false;

            foreach (var pageText in pdfPageText)
            {
                var document = XDocument.Parse(pageText);

                foreach (var graphic in document.Root.Elements())
                {
                    if (graphic.Name.LocalName == "text")
                    {

                        var fontSize = graphic.Attribute("font-size").Value;
                        var fontFamily = graphic.Attribute("font-family").Value;

                        var data = ParseToken(graphic);

                        if (data.Trim() == "Surgeon:")
                        {
                            surgeonName = string.Empty;
                            var surgeonX = graphic.Attribute("x").Value;
                            var surgeonY = graphic.Attribute("y").Value;

                            foreach (var testCell in document.Root.Elements())
                            {
                                var testRowX = testCell.Attribute("x")?.Value;
                                var testRowY = testCell.Attribute("y")?.Value;
                                if (surgeonY == testRowY && surgeonX != testRowX)
                                    surgeonName += testCell.Value;
                            }
                        }
                        if (data.Trim() == "Procedures:")
                        {
                            cardNameDetail = string.Empty;
                            var procedureCell = (graphic.NextNode as XElement).Attribute("y").Value;

                            foreach (var testCell in document.Root.Elements())
                            {
                                var testRow = testCell.Attribute("y")?.Value;
                                if (procedureCell == testRow)
                                    cardNameDetail += testCell.Value;
                            }
                        }

                        if (data == "Supplies")
                            supplies = true;

                        if (data == "Drugs")
                            supplies = false;

                        if (data == "Bin Location" && supplies)
                            cardImport.AddRange(ParseSupplyTable(graphic, surgeonName, cardNameDetail));

                        if (data == "Instruments")
                            instruments = true;

                        if (data == "Bin Location" && instruments)
                            cardImport.AddRange(ParseInstrumentTable(graphic, surgeonName, cardNameDetail));

                        if (data == "Implant Trays")
                            instruments = false;

                    }
                }

            }

            if (cardNameDetail == string.Empty) throw new Exception("Unable to parse card" + filePath);

            return cardImport;
        }

        private List<CardImport> ParseSupplyTable(XElement headerElement, string surgeonName, string cardNameDetail)
        {
            var cardImport = new List<CardImport>();

            var tableHeader = decimal.Parse(headerElement.Attribute("y").Value);
            
            bool newRow = false;
            decimal curPos = 0.0M;

            decimal? descriptionPosition = null;
            decimal? openPosition = null;
            decimal? prnPosition = null;

            var tableName = "Supplies";
            var productNbr = string.Empty;
            var description = string.Empty;
            var openValue = string.Empty;
            var prnValue = string.Empty;

            foreach (var graphic in headerElement.Document.Root.Elements())
            {
                if (graphic.Name.LocalName == "text")
                {


                    var fontSize = graphic.Attribute("font-size").Value;
                    var fontFamily = graphic.Attribute("font-family").Value;

                    var xLoc = decimal.Parse(graphic.Attribute("x").Value);
                    var yLoc = decimal.Parse(graphic.Attribute("y").Value);

                    if (tableHeader > yLoc)
                        continue;

                    var data = ParseToken(graphic);

                    if (tableHeader == yLoc)
                    {
                        if (data == "Item Name")
                        {
                            descriptionPosition = xLoc;
                        }
                        if (data == "Open")
                        {
                            openPosition = xLoc;
                        }
                        if (data == "PRN")
                        {
                            prnPosition = xLoc;
                        }

                        continue;
                    }

                    if (data == "Drugs")
                        break;

                    if (data == "Mfg. #: ")
                    {
                        productNbr = (graphic.NextNode as XElement).Value;
                        newRow = true;
                    }

                    if (xLoc == descriptionPosition)
                    {
                        curPos = yLoc;

                        if (description != string.Empty) 
                            description = $"{description.Trim()} ";
                    }

                    if (xLoc >= descriptionPosition && xLoc < openPosition && yLoc == curPos)
                    {
                        description += data;
                    }

                    if (xLoc == openPosition && yLoc > tableHeader)
                    {
                        openValue = data;
                    }

                    if (xLoc == prnPosition && yLoc > tableHeader)
                    {
                        prnValue = data;
                    }

                    //if (xLoc > productNumPosition && yLoc > tableHeader && description != string.Empty) 
                    if (newRow)
                    {
                        if (description == string.Empty)
                        {
                            newRow = false;
                            continue;
                        }

                        if (data == "Drugs") return cardImport;

                        cardImport.Add(new CardImport()
                        {
                            SurgeonName = surgeonName,
                            CardName = cardNameDetail,
                            Manufacturer = tableName.Trim(),
                            Description = description.Trim(),
                            ProductNbr = productNbr.Trim(),
                            Open = openValue,
                            Prn = prnValue
                        });

                        description = string.Empty;
                        productNbr = string.Empty;
                        openValue = string.Empty;
                        prnValue = string.Empty;

                        newRow = false;
                    }
                }
            }

            // add remaining data
            if (description != string.Empty)
            {
                cardImport.Add(new CardImport()
                {
                    SurgeonName = surgeonName,
                    CardName = cardNameDetail,
                    Manufacturer = tableName.Trim(),
                    Description = description.Trim(),
                    ProductNbr = productNbr.Trim(),
                    Open = openValue,
                    Prn = prnValue
                });
            }

            return cardImport;
        }

        private List<CardImport> ParseInstrumentTable(XElement headerElement, string surgeonName, string cardNameDetail)
        {
            var cardImport = new List<CardImport>();
            var tableHeader = decimal.Parse(headerElement.Attribute("y").Value);

            bool newRow = false;
            decimal curPos = 0.0M;

            decimal? binLocationPosition = null;
            decimal? descriptionPosition = null;
            decimal? openPosition = null;

            var tableName = "Instruments";
            var description = string.Empty;
            var openValue = string.Empty;

            foreach (var graphic in headerElement.Document.Root.Elements())
            {
                if (graphic.Name.LocalName == "text")
                {
                    var fontSize = graphic.Attribute("font-size").Value;
                    var fontFamily = graphic.Attribute("font-family").Value;

                    var xLoc = decimal.Parse(graphic.Attribute("x").Value);
                    var yLoc = decimal.Parse(graphic.Attribute("y").Value);

                    if (tableHeader > yLoc)
                        continue;

                    var data = ParseToken(graphic);

                    if (tableHeader == yLoc)
                    {
                        if (data == "Bin Location")
                        {
                            binLocationPosition = xLoc;
                        }
                        if (data == "Item Name")
                        {
                            descriptionPosition = xLoc;
                        }
                        if (data == "Open")
                        {
                            openPosition = xLoc;
                        }

                        continue;
                    }

                    if (data == "Implant Trays")
                        break;


                    if (xLoc == binLocationPosition)
                    {
                        newRow = curPos > 0;
                        curPos = yLoc;
                    }

                    if (xLoc == descriptionPosition)
                    {
                        curPos = yLoc;

                        if (description != string.Empty)
                            description = $"{description.Trim()} ";
                    }

                    if (xLoc >= descriptionPosition && xLoc < openPosition && yLoc == curPos)
                    {
                        description += data;
                    }

                    if (xLoc == openPosition && yLoc > tableHeader)
                    {
                        openValue = data;
                    }

                    //if (xLoc > productNumPosition && yLoc > tableHeader && description != string.Empty) 
                    if (newRow)
                    {
                        if (description == string.Empty)
                        {
                            newRow = false;
                            continue;
                        }

                        if (data == "Drugs") return cardImport;

                        cardImport.Add(new CardImport()
                        {
                            SurgeonName = surgeonName,
                            CardName = cardNameDetail,
                            Manufacturer = tableName.Trim(),
                            Description = description.Trim(),
                            Open = openValue,
                        });

                        description = string.Empty;
                        openValue = string.Empty;

                        newRow = false;
                    }
                }
            }

            // add remaining data
            if (description != string.Empty)
            {
                cardImport.Add(new CardImport()
                {
                    SurgeonName = surgeonName,
                    CardName = cardNameDetail,
                    Manufacturer = tableName.Trim(),
                    Description = description.Trim(),
                    Open = openValue,
                });
            }

            return cardImport;
        }

        private bool CheckRange(decimal xLoc, decimal? target)
        {
            if (target == null) return false;
            return xLoc > target - 4M && xLoc < target + 1M;
        }

        private bool CheckRow(decimal yLoc, decimal curRow)
        {
            return yLoc > curRow + 7M;
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
            public string SurgeonName { get; set; }
            public string Manufacturer { get; set; }
            public string Description { get; set; }
            public string Comment { get; set; }
            public string ProductNbr { get; set; }
            public string Quantity { get; set; }
            public string Open { get; set; }
            public string Prn { get; set; }
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
