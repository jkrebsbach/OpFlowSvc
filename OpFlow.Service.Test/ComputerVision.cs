using Azure;
using Azure.AI.Vision.ImageAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpFlow.Service.Test
{
    [TestClass]
    public class ComputerVision
    {

        [TestMethod]
        public async Task ExtractPDFFolder()
        {
            var path = @"D:\ColdStorage\Documents\OpFlow\Imports\AdventHealth\";

            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                var files = Directory.GetFiles(directory, "*.pdf");

                foreach (var file in files)
                {
                    var fileBytes = File.ReadAllBytes(file);

                    var results = await ExecuteComputerVision(fileBytes);

                }
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
            if (result.Read != null)
            {
                foreach (var block in result.Read.Blocks)
                {
                    foreach (var line in block.Lines)
                    {
                        Console.WriteLine($"Line: '{line.Text}'");
                        // You can also access line.BoundingPolygon and individual words within the line
                    }
                }
            }


            return result;
        }
    }
}
