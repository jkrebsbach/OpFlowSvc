using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;


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

               // ImageController cont = new ImageController();
               // var result = await cont.GetPatientPositionImage(patientId);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

    }
}
