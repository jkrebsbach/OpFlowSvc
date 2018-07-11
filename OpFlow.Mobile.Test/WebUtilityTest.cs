using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpFlow.Data;

namespace OpFlow.Mobile.Test
{
    [TestClass]
    public class WebUtilityTest
    {
        [TestInitialize]
        public void Init()
        {
            WebUtility.EnableTesting();
        }

        [TestMethod]
        public async Task TestSendMessage()
        {
            await SurgeryUtil.GetSurgeryUserSchedule(DateTime.Today);
        }

        [TestMethod]
        public async Task TestSendFile()
        {
            var fileBytes = File.ReadAllBytes(@"C:\temp\sample_image.jpg");

            await AppSettings.AuthenticateUser("info@opflowtech.com", "OpFlow1!");
            await FlowUtil.UploadFlowImage(1, fileBytes);
        }
    }
}
