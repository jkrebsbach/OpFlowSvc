using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using OpFlow.Data;
using OpFlow.Data.Debrief;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    public class FlowController : ApiController
    {
        // GET api/values/5
        [SwaggerOperation("Get")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Flow))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetFlow(int flowId, int? cardId = null, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var flow = DataAccess.SqlHelper.GetFlow(flowId, user.ProviderID, user.LocationID);

            return flow == null ? 
                Request.CreateResponse(HttpStatusCode.NotFound) : 
                Request.CreateResponse(HttpStatusCode.OK, flow);
        }

        // POST api/values
        [SwaggerOperation("AddSurgerySmartPhrase")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/flow/surgerySmartPhrase", Name = "AddSurgerySmartPhrase")]
        [HttpPut]
        public async Task<IHttpActionResult> AddSurgerySmartPhrase(int surgeryId, int smartPhraseId)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.AddSurgerySmartPhrase(surgeryId, smartPhraseId,
                user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("NewSmartPhrase")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/flow/smartPhrase", Name = "NewSmartPhrase")]
        [HttpPut]
        public async Task<IHttpActionResult> NewSmartPhrase([FromBody]SmartPhrasePost smartPhrase)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.NewSmartPhrase(smartPhrase.Phrase, smartPhrase.CategoryID, smartPhrase.StepID, smartPhrase.RoleID,
                user.UserID, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("EditSmartPhrase")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/flow/smartPhrase", Name = "EditSmartPhrase")]
        [HttpPost]
        public async Task<IHttpActionResult> EditSmartPhrase(int smartPhraseId, [FromBody]SmartPhrasePost smartPhrase)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.EditSmartPhrase(smartPhraseId, smartPhrase.Phrase, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("DeleteSmartPhrase")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/flow/smartPhrase", Name = "DeleteSmartPhrase")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteSmartPhrase(int smartPhraseId)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.DeleteSmartPhrase(smartPhraseId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("NewSurgeonNote")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/flow/surgeonNote", Name = "NewSurgeonNote")]
        [HttpPut]
        public async Task<IHttpActionResult> NewSurgeonNote([FromBody]SurgeonNotePost smartPhrase)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.NewSurgeonNote(smartPhrase.Phrase, smartPhrase.FlowID, smartPhrase.StepID, smartPhrase.RoleID,
                user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("DeleteSurgeonNote")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/flow/surgeonNote", Name = "DeleteSurgeonNote")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteSurgeonNote(int surgeonNoteId)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.DeleteSurgeonNote(surgeonNoteId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("NewFlowImage")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/flow/flowImage", Name = "NewFlowImage")]
        [HttpPut]
        [AllowAnonymous]
        public async Task<IHttpActionResult> NewFlowImage(int flowId, int stepId, int roleId, string comment)
        {
            var user = CacheUtil.GetUserSecurity();

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            var fileContents = await provider.Contents[0].ReadAsByteArrayAsync();

            var flowImageId = DataAccess.SqlHelper.NewFlowImage(flowId, stepId, roleId, comment,
                user.ProviderID, user.LocationID);

            var folder = DataAccess.BlobStorageHelper.Folder(flowId, 0, 0, 0, 0);
            await DataAccess.BlobStorageHelper.PutBlobBytes(folder, flowImageId.ToString(), fileContents);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("EditFlowImage")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/flow/flowImage", Name = "EditFlowImage")]
        [HttpPost]
        public async Task<IHttpActionResult> EditFlowImage(int flowImageId, [FromBody]FlowImagePost flowImage)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.EditFlowImage(flowImageId, flowImage.StepID, flowImage.RoleID, flowImage.Comment, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("DeleteFlowImage")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/flow/flowImage", Name = "DeleteFlowImage")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteFlowImage(int flowImageId, int flowId)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.DeleteFlowImage(flowImageId, user.ProviderID, user.LocationID);

            var folder = DataAccess.BlobStorageHelper.Folder(flowId, 0, 0, 0, 0);
            await DataAccess.BlobStorageHelper.DeleteBlob(folder, flowImageId.ToString());

            return Ok();
        }


        // POST api/values
        [SwaggerOperation("AddFlowFeedback")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/flow/flowFeedback", Name = "AddFlowFeedback")]
        [HttpPost]
        public async Task<IHttpActionResult> AddFlowFeedback(int flowId, [FromBody]FlowFeedbackPost flowFeedback)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.AddFlowFeedback(flowId, flowFeedback.Feedback, user.UserID, user.ProviderID, user.LocationID);

            return Ok();
        }

        // GET api/values/5
        [SwaggerOperation("GetCardFlowList")]
        [Route("api/flow/cardFlowList")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Flow>))]
        public HttpResponseMessage GetCardFlowList(int cardId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var flowList = DataAccess.SqlHelper.GetCardFlowList(cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, flowList);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowInstructions")]
        [Route("api/flow/instructions")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowStepInstructionResult>))]
        public HttpResponseMessage GetFlowInstructions(int flowId, int? surgeryId = null, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetFlowInstructions(flowId, surgeryId ?? 0, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowTimings")]
        [Route("api/flow/timings")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowStepTiming>))]
        public HttpResponseMessage GetFlowTimings(int flowId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetFlowTimings(flowId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowSurgeryTimings")]
        [Route("api/flow/surgerytimings")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowStepSurgeryTiming>))]
        public HttpResponseMessage GetFlowSurgeryTimings(int surgeryId, int? flowId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetFlowSurgeryTimings(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowComments")]
        [Route("api/flow/comments")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowStep>))]
        public HttpResponseMessage GetFlowComments(int flowId, int surgeryId)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetFlowComments(flowId, surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowMessaging")]
        [Route("api/flow/messaging")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowMessaging>))]
        public HttpResponseMessage GetFlowMessaging(int flowId, int stepId)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetFlowMessaging(flowId, user.ProviderID, user.LocationID, stepId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowDetails")]
        [Route("api/flow/details")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowFeedback>))]
        public HttpResponseMessage GetFlowDetails(int flowId, int? surgeryId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var flow = DataAccess.SqlHelper.GetFlow(flowId, user.ProviderID, user.LocationID);
            var feedback = DataAccess.SqlHelper.GetFlowFeedback(flowId, user.ProviderID, user.LocationID);
            var notifications = DataAccess.SqlHelper.GetFlowNotifications(flowId, null, user.ProviderID, user.LocationID);
            var flowInstructions = DataAccess.SqlHelper.GetFlowInstructions(flowId, surgeryId ?? 0, user.ProviderID, user.LocationID);
            var content = DataAccess.SqlHelper.GetFlowContent(flowId, 1, user.ProviderID, user.LocationID);
            var timings = DataAccess.SqlHelper.GetFlowTimings(flowId, user.ProviderID, user.LocationID);
            var images = DataAccess.SqlHelper.GetFlowImages(flowId, user.ProviderID, user.LocationID);

            foreach (var timing in timings)
            {
                timing.RoleInstructions = flowInstructions.FirstOrDefault(i => i.StepID == timing.StepID)?.FlowRoleInstructions;
                timing.StepNotifications = notifications.Where(n => n.StepID == timing.StepID).ToList();

                timing.FlowImages = images.Where(n => n.FlowStepID == timing.StepID).ToList();
            }

            var flowDetail = new FlowDetail()
            {
                Flow = flow,
                Steps = timings,
                Feedback = feedback,
                Notifications = notifications,
                Instructions = flowInstructions,
                Content = content
            };

            return Request.CreateResponse(HttpStatusCode.OK, flowDetail);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowContent")]
        [Route("api/flow/content")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowContent>))]
        public HttpResponseMessage GetFlowContent(int flowId, int surgeryId)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetFlowContent(flowId, surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowNotifications")]
        [Route("api/flow/notifications")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowNotification>))]
        public HttpResponseMessage GetFlowNotifications(int flowId, int stepId)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetFlowNotifications(flowId, stepId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowFeedback")]
        [Route("api/flow/feedback")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowFeedback>))]
        public HttpResponseMessage GetFlowFeedback(int flowId)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetFlowFeedback(flowId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // PUT api/values/5
        [SwaggerOperation("UpdateFlowSteps")]
        [Route("api/flow/steps")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult UpdateFlowSteps(int flowId, [FromBody]List<FlowStepPost> value)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.DeleteFlowStep(flowId, user.ProviderID, user.LocationID);

            foreach (var flowStep in value)
            {
                var result = DataAccess.SqlHelper.InsertFlowStep(flowId, flowStep.StepID, flowStep.StepDuration, flowStep.StepDescription, user.ProviderID, user.LocationID);
            }

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("CreateFlowNotification")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/flow/notification")]
        [HttpPost]
        public HttpResponseMessage CreateFlowNotification(int flowId, [FromBody]FlowNotificationPost value)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.InsertFlowNotification(flowId, value.StepID, value.NotificationType, value.Message, value.SmsNumber, 
                value.EmailAddress, value.MessagingUserID, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.Created, result);
        }

        // POST api/values
        [SwaggerOperation("EditFlowNotification")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/flow/notification")]
        [HttpPut]
        public HttpResponseMessage EditFlowNotification(int flowNotificationId, [FromBody]FlowNotificationPost value)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.EditFlowNotification(flowNotificationId, value.Message, value.SmsNumber,
                value.EmailAddress, value.MessagingUserID, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // POST api/values
        [SwaggerOperation("DeleteFlowNotification")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/flow/notification")]
        [HttpDelete]
        public HttpResponseMessage DeleteFlowNotification(int flowNotificationId)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.DeleteFlowNotification(flowNotificationId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public HttpResponseMessage Post([FromBody]FlowPost value)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.NewFlow(value.CardID, value.RoomSetupID, value.Description, user.UserID, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // PUT api/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Put(int id, [FromBody]FlowPost value)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.UpdateFlow(id, value.CardID, value.RoomSetupID, value.Description, user.UserID, user.ProviderID, user.LocationID);

            return Ok();
        }

        // DELETE api/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Delete(int id)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.DeleteFlow(id, user.ProviderID, user.LocationID);

            return Ok();
        }
    }
}