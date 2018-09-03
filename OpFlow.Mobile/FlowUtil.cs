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
        public static async Task<List<FlowStepTiming>> GetFlowSurgeryTimings(int surgeryId)
        {
            var command = string.Format("api/flow/surgerytimings?surgeryId={0}", surgeryId);
            var response = await WebUtility.WebRequest<List<FlowStepTiming>>(command, HttpMethod.Get);

            return response.OrderBy(r => r.StepID).ToList();
        }

        public static async Task<List<FlowStepInstructionResult>> GetFlowInstructions(int flowId, int surgeryId)
        {
			var command = string.Format("api/flow/instructions?flowId={0}&surgeryId={1}", flowId, surgeryId);
			var response = await WebUtility.WebRequest<List<FlowStepInstructionResult>>(command, HttpMethod.Get);

            return response.OrderBy(r => r.StepID).ToList();
        }

        public static async Task<FlowImageResult> GetFlowImages(int flowId, int surgeryId)
        {
            var command = string.Format("api/flow/images?flowId={0}&surgeryId={1}", flowId, surgeryId);
            var response = await WebUtility.WebRequest<FlowImageResult>(command, HttpMethod.Get);

            return response;
        }

        public static async Task<Flow> GetFlow(int flowId, int cardId)
        {
            var command = string.Format("api/flow?flowId={0}&cardId={1}", flowId, cardId);
            var response = await WebUtility.WebRequest<Flow>(command, HttpMethod.Get) ?? new Flow()
            {
                FlowID = -1,
                FlowDescription = "NO FLOW DEFINED"
            };

            return response;
        }

        public static async Task<List<Flow>> GetCardFlows(int cardId)
        {
            var command = string.Format("api/flow/cardFlowList?cardId={0}", cardId);
            var response = await WebUtility.WebRequest<List<Flow>>(command, HttpMethod.Get);

            return response;
        }

        public static async Task<List<FlowStepTiming>> GetFlowTimings(int flowId)
        {
            var command = $"api/flow/timings?flowId={flowId}";
            return await WebUtility.WebRequest<List<FlowStepTiming>>(command, HttpMethod.Get);
        }

        public static async Task<List<Step>> GetSteps()
        {
            var command = "api/flow/steps";
            return await WebUtility.WebRequest<List<Step>>(command, HttpMethod.Get);
        }

        public static async Task<byte[]> FlowImageBytes(int flowId, int flowImageId)
        {
            var command = string.Format("api/image/flowImage?flowId={0}&flowImageId={1}", flowId, flowImageId);
            var image = await WebUtility.WebRequest<SecureImage>(command,  HttpMethod.Get);

            if (image?.DocumentBytes == null)
                return null;

            return Convert.FromBase64String(image.DocumentBytes);
        }

        public static async Task<string> UploadFlowImage(int flowId, int stepId, int roleId, byte[] flowBytes)
        {
            var command = $"api/flow/flowImage?flowId={flowId}&stepId={stepId}&roleId={roleId}";
            var response = await WebUtility.FileRequest<string>(command, HttpMethod.Put, flowBytes);

            return response;
            
        }
    }
}
