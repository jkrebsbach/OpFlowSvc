using System;
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
            
        }

        [TestMethod]
        public async Task TestSendMessage()
        {
            await SurgeryUtil.GetSurgeryUserSchedule(DateTime.Today);
        }
    }
}
