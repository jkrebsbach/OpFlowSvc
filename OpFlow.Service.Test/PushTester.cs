using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpFlow.Service.Test
{
    [TestClass]
    public class PushTester
    {
        [TestMethod]
        public async Task TestPushMessage()
        {
            try
            {
               // await PushNotification.PostNotification("martyn@opflowtech.com", "mknowles", "role2@opflowtech.com", "This is a test");

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
