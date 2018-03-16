using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpFlow.Service.Controllers;

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
                    var payload = new ImageController.ImagePost()
                    {
                        Payload = positionBinary
                    };

                    ImageController cont = new ImageController();
                    await cont.PutPatientPositionImage(image, payload);

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
    }
}
