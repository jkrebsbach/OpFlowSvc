using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using OpFlow.Data;

namespace OpFlow.Mobile
{
    public abstract class FlowUtil
    {
        public static async Task<List<FlowTiming>> GetFlowTimings(int flowId)
        {
            var providerId = AppSettings.CurrentUser.ProviderID;
            var locationId = AppSettings.CurrentUser.LocationID;

            var command = string.Format("api/flow/timings?flowId={0}&providerId={1}&locationId={2}", flowId, providerId, locationId);
            var response = await WebUtility.WebRequest<List<FlowTiming>>(command, HttpMethod.Get);

            return response.OrderBy(r => r.StepID).ToList();
        }

        public static async Task<List<FlowTiming>> GetFlowInstructions(int flowId)
        {
            var providerId = AppSettings.CurrentUser.ProviderID;
            var locationId = AppSettings.CurrentUser.LocationID;

            var command = string.Format("api/flow/instructions?flowId={0}&providerId={1}&locationId={2}", flowId, providerId, locationId);
            var response = await WebUtility.WebRequest<List<FlowTiming>>(command, HttpMethod.Get);

            return response.OrderBy(r => r.StepID).ToList();
        }

        public static async Task<Flow> GetFlow(int flowId, int cardId)
        {
            var providerId = AppSettings.CurrentUser.ProviderID;
            var locationId = AppSettings.CurrentUser.LocationID;

            var command = string.Format("api/flow?flowId={0}&cardId={1}&providerId={2}&locationId={3}", 
                flowId, cardId, providerId, locationId);
            var response = await WebUtility.WebRequest<Flow>(command, HttpMethod.Get) ?? new Flow()
            {
                FlowID = -1,
                Description = "NO FLOW DEFINED"
            };

            return response;
        }
    }
}
