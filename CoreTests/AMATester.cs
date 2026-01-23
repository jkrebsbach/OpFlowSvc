using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpFlow.Service.Core.DataAccess;

namespace CoreTests
{
    public class AMATester
    {
        private AMAHelper _amaHelper;

        public AMATester()
        {
            _amaHelper = new AMAHelper(TestBase.GetConfiguration());
        }

        [Fact]
        public async Task DownloadCPT()
        {
            var httpClient = new HttpClient();

            try
            {
                var bytes = await _amaHelper.GetCPT();
                
                File.WriteAllBytes("cpt.zip", bytes);
            }
            catch (Exception ex)
            {
                var error = ex.Message;
            }
        }
    }
}
