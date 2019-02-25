using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using OpFlow.Data;
using OpFlow.Data.Debrief;
using OpFlow.Service.DataAccess;
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
        public async Task<HttpResponseMessage> GetFlow(int flowId, int? cardId = null, int? providerId = null, int? locationId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var flow = await SqlHelper.GetFlow(flowId, user.ProviderID, user.LocationID);

            return flow == null ? 
                Request.CreateResponse(HttpStatusCode.NotFound) : 
                Request.CreateResponse(HttpStatusCode.OK, flow);
        }

        // POST api/values
        [SwaggerOperation("AddFlowPhrase")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [Route("api/flow/flowPhrase", Name = "AddFlowPhrase")]
        [HttpPost]
        public async Task<HttpResponseMessage> AddFlowPhrase(int flowId, int smartPhraseId, int? stepId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            try
            {
                await SqlHelper.AddFlowSmartPhrase(flowId, smartPhraseId, stepId, user.ProviderID, user.LocationID);
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Message.ToUpper().Contains("DUPLICATE KEY"))
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict,
                        "Cannot assign instruction to step multiple times");
                }

                throw;
            }

            return Request.CreateResponse(HttpStatusCode.OK, 200);
        }

        // POST api/values
        [SwaggerOperation("BulkAddFlowPhrase")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/flow/flowPhraseBatch", Name = "BulkAddFlowPhrase")]
        [HttpPost]
        public async Task<HttpResponseMessage> AddFlowPhraseBatch(int flowId, int stepId, [FromBody]BatchEditModel batch)
        {
            var user = await CacheUtil.GetUserSecurity();

            foreach (var smartPhraseId in batch.SmartPhraseIDList)
            {
                try
                {
                    await SqlHelper.AddFlowSmartPhrase(flowId, smartPhraseId, stepId, user.ProviderID, user.LocationID);
                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.Message.ToUpper().Contains("DUPLICATE KEY"))
                    {
                        // Ignore issue
                        continue;
                    }

                    throw;
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, 200);
        }

        // POST api/values
        [SwaggerOperation("UpdateFlowPhrase")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/flow/flowPhrase", Name = "UpdateFlowPhrase")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateFlowPhrase(int flowId, int smartPhraseId, [FromBody]PhraseUpdatePost debriefUpdate)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.UpdateFlowPhrase(flowId, smartPhraseId,
                debriefUpdate.Comments, debriefUpdate.StepID, debriefUpdate.RoleID, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("DeleteFlowPhrase")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/flow/flowPhrase", Name = "DeleteFlowPhrase")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteFlowPhrase(int flowId, int smartPhraseId)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.DeleteFlowPhrase(flowId, smartPhraseId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("NewSmartPhrase")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/flow/smartPhrase", Name = "NewSmartPhrase")]
        [HttpPost]
        public async Task<IHttpActionResult> NewSmartPhrase([FromBody]NewSmartPhrasePost smartPhrase)
        {
            var user = await CacheUtil.GetUserSecurity();

            var smartPhraseId = await SqlHelper.NewSmartPhrase(smartPhrase.Phrase, smartPhrase.CategoryID, smartPhrase.StepID, smartPhrase.RoleID,
                smartPhrase.UserID ?? user.UserID, user.ProviderID, user.LocationID);

            if (smartPhrase.FlowID.HasValue)
            {
                await SqlHelper.AddFlowSmartPhrase(smartPhrase.FlowID.Value, smartPhraseId, smartPhrase.StepID, user.ProviderID, user.LocationID);
            }
            else if (smartPhrase.SurgeryID.HasValue)
            {
                await SqlHelper.AddSurgerySmartPhrase(smartPhrase.SurgeryID.Value, smartPhraseId, user.ProviderID,
                    user.LocationID);
            }

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("EditSmartPhrase")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/flow/smartPhrase", Name = "EditSmartPhrase")]
        [HttpPut]
        public async Task<IHttpActionResult> EditSmartPhrase(int smartPhraseId, [FromBody]SmartPhrasePost smartPhrase)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.EditSmartPhrase(smartPhraseId, smartPhrase.Phrase, smartPhrase.CategoryID, smartPhrase.UserID ??  user.UserID, smartPhrase.StepID, smartPhrase.RoleID, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("DeleteSmartPhrase")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/flow/smartPhrase", Name = "DeleteSmartPhrase")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteSmartPhrase(int smartPhraseId)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.DeleteSmartPhrase(smartPhraseId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("NewFlowImage")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/flow/flowImage", Name = "NewFlowImage")]
        [HttpPut]
        public async Task<IHttpActionResult> NewFlowImage(int flowId, int stepId, int roleId, string comment = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // extract file name and file contents
            var fileNameParam = provider.Contents[0].Headers.ContentDisposition.Parameters
                .FirstOrDefault(p => p.Name.ToLower() == "filename");
            var fileName = fileNameParam?.Value.Trim('"') ?? "";
            var fileExtension = Path.GetExtension(fileName);
            var fileContents = await provider.Contents[0].ReadAsByteArrayAsync();

            var flowImageId = await SqlHelper.NewFlowImage(flowId, stepId, roleId, comment,
                user.ProviderID, user.LocationID);

            if (fileExtension == ".png" ||
                fileExtension == ".jpg" ||
                fileExtension == ".jpeg")
            {
                fileContents = BlobStorageHelper.CompressImage(fileContents);
            }

            var storageHelper = BlobStorageHelper.GetHelper(user);

            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.FlowImages, flowId);
            await storageHelper.PutBlobBytes(folder, flowImageId.ToString(), fileContents);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("UpdateFlowImage")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/flow/flowImage", Name = "UpdateFlowImage")]
        [HttpPost]
        public async Task<IHttpActionResult> UpdateFlowImage(int flowImageId, [FromBody]FlowImagePost flowImage)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.UpdateFlowImage(flowImageId, flowImage.Comment, flowImage.StepID, flowImage.RoleID, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("RotateFlowImage")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/flow/rotateFlowImage", Name = "RotateFlowImage")]
        [HttpPut]
        public async Task<IHttpActionResult> RotateFlowImage(int flowImageId, int flowId, int direction)
        {
            var user = await CacheUtil.GetUserSecurity();

            // Make sure valid rotation direction
            if (direction != 1 && direction != -1)
                return Ok();


            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.FlowImages, flowId);

            var storageHelper = BlobStorageHelper.GetHelper(user);

            await storageHelper.RotateImage(folder, flowImageId.ToString(), direction);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("DeleteFlowImage")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/flow/flowImage", Name = "DeleteFlowImage")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteFlowImage(int flowImageId, int flowId)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.DeleteFlowImage(flowImageId, user.ProviderID, user.LocationID);

            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.FlowImages, flowId);

            var storageHelper = BlobStorageHelper.GetHelper(user);

            await storageHelper.DeleteBlob(folder, flowImageId.ToString());

            return Ok();
        }


        // POST api/values
        [SwaggerOperation("AddFlowFeedback")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/flow/flowFeedback", Name = "AddFlowFeedback")]
        [HttpPost]
        public async Task<IHttpActionResult> AddFlowFeedback(int flowId, [FromBody]FlowFeedbackPost flowFeedback)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.AddFlowFeedback(flowId, flowFeedback.Feedback, user.UserID, user.ProviderID, user.LocationID);

            return Ok();
        }


        // POST api/values
        [SwaggerOperation("DeleteFlowFeedback")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/flow/flowFeedback", Name = "DeleteFlowFeedback")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteFlowFeedback(int flowFeedbackId)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.DeleteFlowFeedback(flowFeedbackId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // GET api/values/5
        [SwaggerOperation("GetCardFlowList")]
        [Route("api/flow/cardFlowList")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Flow>))]
        public async Task<HttpResponseMessage> GetCardFlowList(int cardId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var flowList = await SqlHelper.GetCardFlowList(cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, flowList);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowInstructions")]
        [Route("api/flow/instructions")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowStepInstructionResult>))]
        public async Task<HttpResponseMessage> GetFlowInstructions(int flowId, int? surgeryId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.GetFlowInstructions(flowId, surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowImages")]
        [Route("api/flow/images")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(FlowImageResult))]
        public async Task<HttpResponseMessage> GetFlowImages(int flowId, int? surgeryId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = new FlowImageResult
            {
                FlowImages = await SqlHelper.GetFlowImages(flowId, user.ProviderID, user.LocationID)
            };

            if (surgeryId.HasValue)
            {
                result.SurgeryImages = await SqlHelper.GetSurgeryImages(surgeryId.Value, user.ProviderID, user.LocationID);
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowTimings")]
        [Route("api/flow/timings")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowStepTiming>))]
        public async Task<HttpResponseMessage> GetFlowTimings(int flowId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.GetFlowTimings(flowId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowSurgeryTimings")]
        [Route("api/flow/surgerytimings")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowStepSurgeryTiming>))]
        public async Task<HttpResponseMessage> GetFlowSurgeryTimings(int surgeryId, int? flowId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.GetFlowSurgeryTimings(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowComments")]
        [Route("api/flow/comments")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowStep>))]
        public async Task<HttpResponseMessage> GetFlowComments(int flowId, int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.GetFlowComments(flowId, surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowMessaging")]
        [Route("api/flow/messaging")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowMessaging>))]
        public async Task<HttpResponseMessage> GetFlowMessaging(int flowId, int stepId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.GetFlowMessaging(flowId, user.ProviderID, user.LocationID, stepId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowDetails")]
        [Route("api/flow/details")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(FlowDetail))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> GetFlowDetails(int flowId, int? surgeryId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var flow = await SqlHelper.GetFlow(flowId, user.ProviderID, user.LocationID);

            if (flow == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var feedback = await SqlHelper.GetFlowFeedback(flowId, user.ProviderID, user.LocationID);
            var notifications = await SqlHelper.GetFlowNotifications(flowId, null, user.ProviderID, user.LocationID);
            var flowInstructions = await SqlHelper.GetFlowInstructions(flowId, surgeryId, user.ProviderID, user.LocationID);
            var content = await SqlHelper.GetFlowContent(flowId, 1, user.ProviderID, user.LocationID);
            var timings = await SqlHelper.GetFlowTimings(flowId, user.ProviderID, user.LocationID);
            var flowImages = await SqlHelper.GetFlowImages(flowId, user.ProviderID, user.LocationID);
            var surgeryDelays = await SqlHelper.GetFlowSurgeryDelays(flowId, user.ProviderID, user.LocationID);
            var surgeryImages = new List<SurgeryImage>();

            if (surgeryId.HasValue)
            {
                surgeryImages = await SqlHelper.GetSurgeryImages(surgeryId.Value, user.ProviderID, user.LocationID);
            }

            foreach (var timing in timings)
            {
                timing.RoleInstructions = flowInstructions.FirstOrDefault(i => i.StepID == timing.StepID)?.FlowRoleInstructions;
                timing.StepNotifications = notifications.Where(n => n.StepID == timing.StepID).ToList();

                timing.FlowImages = flowImages.Where(n => n.FlowStepID == timing.StepID).ToList();
                timing.SurgeryImages = surgeryImages.Where(n => n.FlowStepID == timing.StepID).ToList();
            }

            var flowDetail = new FlowDetail()
            {
                Flow = flow,
                Steps = timings,
                Feedback = feedback,
                Content = content,
                SurgeryDelays = surgeryDelays
            };

            return Request.CreateResponse(HttpStatusCode.OK, flowDetail);
        }

        // GET api/values/5
        [SwaggerOperation("GetSmartPhraseAdmin")]
        [Route("api/flow/smartPhraseAdmin")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(SmartPhraseAdmin))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> GetSmartPhraseAdmin()
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = new SmartPhraseAdmin()
            {
                Categories = await SqlHelper.GetSmartPhraseCategories(user.ProviderID, user.LocationID),
                Specialties = await SqlHelper.GetSpecialties(user.ProviderID, user.LocationID),
                Users = await SqlHelper.SearchUsers(null, 1, null, user.ProviderID, user.LocationID)
            };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetSmartPhrases")]
        [Route("api/flow/smartPhrases")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SmartPhrase>))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> GetSmartPhrases(int? categoryId = null, int? specialtyId = null, int? userId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var categories = await SqlHelper.GetSmartPhrases(categoryId, specialtyId, null, user.ProviderID, user.LocationID);

            if (userId.HasValue)
                categories = categories.Where(c => c.UserID == userId.Value).ToList();

            return Request.CreateResponse(HttpStatusCode.OK, categories);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowContent")]
        [Route("api/flow/content")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowContent>))]
        public async Task<HttpResponseMessage> GetFlowContent(int flowId, int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.GetFlowContent(flowId, surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowNotifications")]
        [Route("api/flow/notifications")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowNotification>))]
        public async Task<HttpResponseMessage> GetFlowNotifications(int flowId, int stepId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.GetFlowNotifications(flowId, stepId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetSteps")]
        [Route("api/flow/steps")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Step>))]
        public async Task<HttpResponseMessage> GetSteps()
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.GetSteps(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowFeedback")]
        [Route("api/flow/feedback")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowFeedback>))]
        public async Task<HttpResponseMessage> GetFlowFeedback(int flowId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.GetFlowFeedback(flowId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // PUT api/values/5
        [SwaggerOperation("UpdateFlowSteps")]
        [Route("api/flow/steps")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> UpdateFlowSteps(int flowId, [FromBody]FlowStepPost flowStepDetail)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.DeleteFlowStep(flowId, user.ProviderID, user.LocationID);

            for (var index = 0; index < flowStepDetail.FlowSteps.Count; index++)
            {
                var flowStep = flowStepDetail.FlowSteps[index];

                await SqlHelper.InsertFlowStep(flowId, flowStep.StepID, index + 1, flowStep.StepDuration, user.ProviderID, user.LocationID);
            }

            return Ok();
        }

        // PUT api/values/5
        [SwaggerOperation("UpdateSurgerySteps")]
        [Route("api/flow/surgerySteps")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> UpdateSurgerySteps(int stepId, [FromBody]StepPost flowStepDetail)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.UpdateSurgeryStep(stepId, 
                flowStepDetail.StepName, flowStepDetail.StepNotificationType, flowStepDetail.StepTiming, user.ProviderID, user.LocationID);

            return Ok();
        }

        // PUT api/values/5
        [SwaggerOperation("AddSurgerySteps")]
        [Route("api/flow/surgerySteps")]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> AddSurgerySteps([FromBody]StepPost flowStepDetail)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.AddSurgeryStep(
                flowStepDetail.StepName, flowStepDetail.StepNotificationType, flowStepDetail.StepTiming, user.ProviderID, user.LocationID);

            return Ok();
        }

        // PUT api/values/5
        [SwaggerOperation("DeleteSurgerySteps")]
        [Route("api/flow/surgerySteps")]
        [HttpDelete]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> DeleteSurgerySteps(int stepId)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.DeleteSurgeryStep(stepId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("CreateFlowNotification")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/flow/notification")]
        [HttpPost]
        public async Task<HttpResponseMessage> CreateFlowNotification(int flowId, [FromBody]FlowNotificationPost value)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.InsertFlowNotification(flowId, value.StepID, value.NotificationType, value.Message, value.SmsNumber, 
                value.EmailAddress, value.MessagingUserID, value.MessagingRoleID, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.Created, result);
        }

        // POST api/values
        [SwaggerOperation("EditFlowNotification")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/flow/notification")]
        [HttpPut]
        public async Task<HttpResponseMessage> EditFlowNotification(int flowNotificationId, [FromBody]FlowNotificationPost value)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.EditFlowNotification(flowNotificationId, value.Message, value.StepID, value.SmsNumber,
                value.EmailAddress, value.MessagingUserID, value.MessagingRoleID, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // POST api/values
        [SwaggerOperation("DeleteFlowNotification")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/flow/notification")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteFlowNotification(int flowNotificationId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.DeleteFlowNotification(flowNotificationId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public async Task<HttpResponseMessage> Post([FromBody]FlowPost value)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.NewFlow(value.CardID, value.RoomSetupID, value.FlowDescription, user.UserID, value.DefaultFlow, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // PUT api/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Put(int id, [FromBody]FlowPost value)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.UpdateFlow(id, value.CardID, value.RoomSetupID, value.FlowDescription, user.UserID, value.DefaultFlow, user.ProviderID, user.LocationID);

            return Ok();
        }

        // DELETE api/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Delete(int id)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.DeleteFlow(id, user.ProviderID, user.LocationID);

            return Ok();
        }
    }
}