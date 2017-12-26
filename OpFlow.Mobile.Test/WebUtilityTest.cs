using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpFlow.Mobile.Test
{
    [TestClass]
    public class WebUtilityTest
    {
        [TestMethod]
        public async Task TestSendMessage()
        {
            await WebUtility.GetSchedules();
        }
    }
}
