using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using OpFlow.Data;

namespace OpFlow.Mobile
{
    public class FlowUtil
    {
        public static async Task<List<Flow>> GetFlowData(int cardId)
        {
            var userId = AppSettings.CurrentUser.UserID;

            var command = string.Format("api/flow?cardId={0}&userId={1}", cardId, userId);
            var response = await WebUtility.WebRequest<List<Flow>>(command, HttpMethod.Get);

            return response;
        }
        public static async Task<Flow> GetFlow(int flowId)
        {
            var command = string.Format("api/flow/{0}", flowId);
            var response = await WebUtility.WebRequest<Flow>(command, HttpMethod.Get);

            return response;
        }
    }
}
