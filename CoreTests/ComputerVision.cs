using Azure;
using Azure.AI.Vision.ImageAnalysis;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CoreTests
{
    public class ComputerVision
    {
        [Fact]
        public async Task ExtractPDF()
        {
            var directory = @"D:\ColdStorage\Documents\OpFlow\Imports\AdventHealth\20250802\Zeeshan DPC";

            var files = Directory.GetFiles(directory, "*.png");

            foreach (var file in files)
            {
                var fileBytes = File.ReadAllBytes(file);

                var results = await ExecuteComputerVision(fileBytes);

                var jsonContent = JsonSerializer.Serialize(results.Read);
                File.WriteAllText(file.Replace(".png", ".txt"), jsonContent);
            }

        }

        private async Task<ImageAnalysisResult> ExecuteComputerVision(byte[] bytes)
        {
            string endpoint = "https://opflowcomputervision.cognitiveservices.azure.com/";
            string key = "EKaj66tBQR7pAzr1XG7RAYXdivKTRrUPkPuJKPF0thmhfKSwxHqhJQQJ99BHACYeBjFXJ3w3AAAFACOG58kU";

            // Create an Image Analysis client.
            ImageAnalysisClient client = new ImageAnalysisClient(new Uri(endpoint), new AzureKeyCredential(key));

            // Get a caption for the image.
            ImageAnalysisResult result = client.Analyze(
                BinaryData.FromBytes(bytes),
                VisualFeatures.Read,
                new ImageAnalysisOptions { GenderNeutralCaption = true });

            // Print caption results to the console
            // Process the extracted text
            //if (result.Read != null)
            //{
            //    foreach (var block in result.Read.Blocks)
            //    {
            //        foreach (var line in block.Lines)
            //        {
            //            Console.WriteLine($"Line: '{line.Text}'");
            //            // You can also access line.BoundingPolygon and individual words within the line
            //        }
            //    }
            //}


            return result;
        }

        string trayName = "";

        [Fact]
        public async Task ProcessResults()
        {
            var path = @"D:\ColdStorage\Documents\OpFlow\Imports\AdventHealth\20250802\";

            foreach (var directory in Directory.GetDirectories(path))
            {
                var fullPath = Path.Combine(path, directory);

                var cardImport = new List<CardImport>();
                var procedureName = string.Empty;
                var surgeonName = string.Empty;
                var serialNumber = string.Empty;

                var targetFiles = Directory.GetFiles(directory, "*.txt");
                var pageRoot = "";
                var minPage = 0;
                var maxPage = 0;
                foreach (var targetFile in targetFiles)
                {
                    pageRoot = targetFile.Split('_')[0];

                    var match = Regex.Match(targetFile, "([0-9]+).txt");
                    var tmpInt = int.Parse(match.Groups[1].Value);
                    if (tmpInt > maxPage) maxPage = tmpInt;
                }

                for (var index = 0; index <= maxPage; index++)
                {
                    try
                    {

                        var file = Path.Combine(path, $"{pageRoot}_{index}.txt");
                        var textJson = File.ReadAllText(file);

                        var readResults = JsonSerializer.Deserialize<DummyResults>(textJson);

                        List<string> pageText = new List<string>();

                        if (readResults.Blocks.Count > 1) throw new Exception("Multiple blocks?");
                        var block = readResults.Blocks[0];

                        var lineIndex = 0;
                        while (lineIndex < block.Lines.Count)
                        {
                            var line = block.Lines[lineIndex];
                            if (lineIndex == 0 && line.Text.Contains(";"))
                            {
                                surgeonName = line.Text.Split(";")[0];
                                procedureName = line.Text.Split(";")[1];

                                // invalid data screwing up parser
                                if (surgeonName == "NEW CARD CREATED FOR DR. GUEL 11/26/2024")
                                {
                                    lineIndex = block.Lines.Count;
                                    continue;
                                }

                                lineIndex++;
                                line = block.Lines[lineIndex];
                                if (line.Text.StartsWith("M-") || line.Text.StartsWith("M -"))
                                    serialNumber = line.Text;
                                else
                                {
                                    throw new Exception("unexpected format");
                                }

                                lineIndex++;
                                line = block.Lines[lineIndex];
                                procedureName += line.Text;
                            }

                            // surgeon: ABC
                            // Locations: ZZ

                            if (line.Text == "Supplies")
                            {
                                lineIndex++;
                            }

                            line = block.Lines[lineIndex];
                            if (line.Text == "Bin Loc")
                            {
                                var supplyTable = ParseSupplyTable(lineIndex, block.Lines);
                                supplyTable.ForEach(cd =>
                                {
                                    cd.SurgeonName = surgeonName;
                                    cd.CardName = procedureName;
                                    cd.SerialNumber = serialNumber;
                                });

                                cardImport.AddRange(supplyTable);
                                lineIndex = block.Lines.Count;
                            }

                            lineIndex++;
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.Write(ex.ToString());
                    }

                }

                var outputData = "CardName,SurgeonName,SerialNumber,TrayName,Comment,ProductNbr,Manufacturer,Description,Quantity,Open,Prn,Cost\r\n";
                foreach (var cardItem in cardImport)
                {
                    outputData += $"\"{cardItem.CardName.Replace("\"", "\"\"")}\",\"{cardItem.SurgeonName.Replace("\"", "\"\"")}\",\"{cardItem.SerialNumber}\",\"{cardItem.TrayName}\"," +
                        $"\"{cardItem.Comment}\",\"{cardItem.ProductNbr}\",\"{cardItem.Manufacturer}\",\"{cardItem.Description}\",\"{cardItem.Quantity}\",\"{cardItem.Open}\"," +
                        $"\"{cardItem.Prn}\",\"{cardItem.Cost}\"\r\n";
                }

                var filename = $"{pageRoot}_CardData.csv";
                var outputFile = Path.Combine(fullPath, filename);
                File.WriteAllText(outputFile, outputData);
            }
        }

        private List<CardImport> ParseSupplyTable(int lineIndex, List<DummyLine> lines)
        {
            var result = new List<CardImport>();

            var foundCost = false;
            var foundTrayName = false;

            while (lineIndex < lines.Count)
            {
                var currLine = lines[lineIndex];
                var text = currLine.Text;

                // new page, won't repeat tray name, assume from prev page
                if (foundCost && currLine.MinX > 210 - 5 && currLine.MinX < 210 + 10)
                {
                    foundTrayName = true;
                    foundCost = false;
                }

                if (foundTrayName)
                {
                    // have we completed the tray instrument table? - exit
                    if (text == "Item Name")
                        return result;

                    var yPos = currLine.MinY;


                    var nextBinLocs = lines
                        .OrderBy(l => l.MinY)
                        .Where(l => 
                            210 - 5 <= l.MinX && 210 + 10 > l.MinX &&
                            yPos + 20 < l.MinY && 
                            l.Width < 300).ToList();

                    var nextBinLoc = nextBinLocs.FirstOrDefault();
                    var nextBinY = nextBinLoc == null ? int.MaxValue : nextBinLoc.BoundingPolygon.Min(bp => bp.Y);

                    var nextTrayLocs = lines
                        .OrderBy(l => l.MinY)
                        .Where(l => 
                            197 - 5 <= l.MinX && 197 + 5 > l.MinX &&
                            yPos + 20 < l.MinY).ToList();

                    var nextTrayLoc = nextTrayLocs.FirstOrDefault();
                    var nextTrayY = nextTrayLoc == null ? int.MaxValue : nextTrayLoc.BoundingPolygon.Min(bp => bp.Y);

                    var nextY = nextBinY;

                    CardImport lineData = ReadLine(lines, yPos, nextY);
                    lineData.TrayName = trayName;
                    result.Add(lineData);

                    lineIndex = lines.IndexOf(nextBinLoc);
                    if (nextTrayY < nextBinY)
                    {
                        lineIndex = lines.IndexOf(nextTrayLoc);
                        foundTrayName = false;
                        foundCost = true;
                    }

                    if (lineIndex == -1) return result;
                    continue;
                }

                if (foundCost)
                {
                    trayName = text;
                    foundCost = false;
                    foundTrayName = true;
                }

                if (text == "Cost")
                {
                    foundCost = true;
                }

                lineIndex++;
            }

            return result;
        }

        private CardImport ReadLine(List<DummyLine> lines, int yPos, int nextY)
        {
            var binLoc = FindOverlap(lines, yPos, nextY, 210);
            var psId = FindOverlap(lines, yPos, nextY, 440);
            var mfrNumber = FindOverlap(lines, yPos, nextY, 658);
            var description = FindOverlap(lines, yPos, nextY, 970);
            var open = FindOverlap(lines, yPos, nextY, 1999);
            var prn = FindOverlap(lines, yPos, nextY, 2136);
            var cost = FindOverlap(lines, yPos, nextY, 2267);

            return new CardImport()
            {
                Comment = binLoc,
                ProductNbr = psId,
                Manufacturer = mfrNumber,
                Description = description,
                Open = open,
                Prn = prn,
                Cost = cost
            };
        }

        private string FindOverlap(List<DummyLine> lines, int yPos, int nextY, int xPos)
        {
            var matching = lines
                .OrderBy(l => l.MinY)
                .Where(l => 
                xPos - 10 <= l.MinX && xPos + 10 > l.MinX &&
                yPos - 5 <= l.MinY && nextY > l.MinY + 30).ToList();



            var matchingYY = lines.Where(l => l.BoundingPolygon.Any(bp =>
                bp.Y - 10 < yPos && bp.Y + 10 > yPos)).ToList();

            if (matching.Any())
                return string.Join(" ", matching.Select(m => m.Text));

            return "";
        }

        public class DummyResults
        {
            public List<DummyBlock> Blocks { get; set; }
        }

        public class DummyBlock
        {
            public List<DummyLine> Lines { get; set; }
        }

        public class DummyLine
        {
            public string Text { get; set; }
            public List<DummyPolygon> BoundingPolygon { get; set; }
            public List<DummyWord> Words { get; set; }
            public int MinX => BoundingPolygon.Min(bp => bp.X);
            public int MinY => BoundingPolygon.Min(bp => bp.Y);
            public int Width => BoundingPolygon.Max(bp => bp.X) - BoundingPolygon.Min(bp => bp.X);
        }

        public class DummyPolygon
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        public class DummyWord
        {
            public string Text { get; set; }
            public List<DummyPolygon> BoundingPolygon { get; set; }
            public decimal Confidence { get; set; }
            public int MinX => BoundingPolygon.Min(bp => bp.X);
            public int MinY => BoundingPolygon.Min(bp => bp.Y);
        }

        private class CardImport
        {
            public string CardName { get; set; }
            public string SurgeonName { get; set; }
            public string SerialNumber { get; set; }
            public string TrayName { get; set; }
            public string Comment { get; set; }
            public string ProductNbr { get; set; }
            public string Manufacturer { get; set; }
            public string Description { get; set; }
            public string Quantity { get; set; }
            public string Open { get; set; }
            public string Prn { get; set; }
            public string Cost { get; set; }
        }
    }
}