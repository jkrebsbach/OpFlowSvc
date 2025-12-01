using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Data.SqlClient;
using OpFlow.Data;
using OpFlow.Data.Debrief;
using OpFlow.Service.DataAccess;

namespace OpFlow.Service.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FlowController : OpFlowController
    {
        private BlobStorageHelper _blobStorageHelper;

        public FlowController(
            BlobStorageHelper blobStorageHelper,
            IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
            _blobStorageHelper = blobStorageHelper;
        }

        // GET api/values/5
        public async Task<ActionResult> GetFlow(int flowId, int? cardId = null)
        {
            var user = await GetUserSecurity();
            

            var flow = await _sqlHelper.GetFlow(flowId, user.SelectedLocation);

            return flow == null ? 
                NotFound() : 
                Ok(flow);
        }

        // POST api/values
        [Route("api/flow/flowPhrase", Name = "AddFlowPhrase")]
        [HttpPost]
        public async Task<ActionResult> AddFlowPhrase(int flowId, int smartPhraseId, int? stepId = null)
        {
            var user = await GetUserSecurity();
            

            try
            {
                await _sqlHelper.AddFlowSmartPhrase(flowId, smartPhraseId, stepId, user.SelectedLocation);
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Message.ToUpper().Contains("DUPLICATE KEY"))
                {
                    return Conflict(
                        "Cannot assign instruction to step multiple times");
                }

                throw;
            }

            return Ok(200);
        }

        // POST api/values
        [Route("api/flow/flowPhraseBatch", Name = "BulkAddFlowPhrase")]
        [HttpPost]
        public async Task<ActionResult> AddFlowPhraseBatch(int flowId, int stepId, [FromBody]BatchEditModel batch)
        {
            var user = await GetUserSecurity();
            

            foreach (var smartPhraseId in batch.IDList)
            {
                try
                {
                    await _sqlHelper.AddFlowSmartPhrase(flowId, smartPhraseId, stepId, user.SelectedLocation);
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

            return Ok(200);
        }

        // POST api/values
        [Route("api/flow/copyImage", Name = "CopyFlowImage")]
        [HttpPost]
        public async Task<ActionResult> CopyFlowImage(int flowImageId, int flowId,int stepId)
        {
            var user = await GetUserSecurity();
            

            var flow = await _sqlHelper.GetFlow(flowId, user.SelectedLocation);

            if (flow == null)
                return NotFound();

            await CopyFlowImageBinary(user, flowImageId, flowId, stepId);

            return Ok(200);
        }

        // POST api/values
        [Route("api/flow/copyImageBatch", Name = "BulkCopyFlowImage")]
        [HttpPost]
        public async Task<ActionResult> CopyFlowImageBatch(int flowId, int stepId, [FromBody]BatchEditModel batch)
        {
            var user = await GetUserSecurity();
            

            var flow = await _sqlHelper.GetFlow(flowId, user.SelectedLocation);

            if (flow == null)
                return NotFound();

            foreach (var flowImageId in batch.IDList)
            {
                await CopyFlowImageBinary(user, flowImageId, flowId, stepId);
            }

            return Ok(200);
        }

        private async Task CopyFlowImageBinary(UserSecurity user, int sourceFlowImageId, int targetFlowId, int targetStepId)
        {
            

            var sourceImage = await _sqlHelper.GetFlowImage(sourceFlowImageId, user.SelectedLocation);
            var sourceFolder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.FlowImages, sourceImage.FlowID);
            var targetFolder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.FlowImages, targetFlowId);

            var binary = await _blobStorageHelper.GetBlobBytes(sourceFolder, sourceFlowImageId.ToString());

            var targetFlowImageId = await _sqlHelper.NewFlowImage(targetFlowId, targetStepId, sourceImage.RoleID, sourceImage.ImageComment,
                user.ProviderID, user.LocationID);

            await _blobStorageHelper.PutBlobBytes(targetFolder, targetFlowImageId.ToString(), binary);
        }

        // POST api/values
        [Route("api/flow/flowPhrase", Name = "UpdateFlowPhrase")]
        [HttpPut]
        public async Task<ActionResult> UpdateFlowPhrase(int flowId, int smartPhraseId, [FromBody]PhraseUpdatePost debriefUpdate)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.UpdateFlowPhrase(flowId, smartPhraseId,
                debriefUpdate.Comments, debriefUpdate.StepID, debriefUpdate.RoleID, user.SelectedLocation);

            return Ok();
        }

        // POST api/values
        [Route("api/flow/flowPhrase", Name = "DeleteFlowPhrase")]
        [HttpDelete]
        public async Task<ActionResult> DeleteFlowPhrase(int flowId, int smartPhraseId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.DeleteFlowPhrase(flowId, smartPhraseId, user.SelectedLocation);

            return Ok();
        }

        // POST api/values
        [Route("api/flow/smartPhrase", Name = "NewSmartPhrase")]
        [HttpPost]
        public async Task<ActionResult> NewSmartPhrase([FromBody]NewSmartPhrasePost smartPhrase)
        {
            var user = await GetUserSecurity();
            

            var smartPhraseId = await _sqlHelper.NewSmartPhrase(smartPhrase.Phrase, smartPhrase.CategoryID, smartPhrase.StepID, smartPhrase.RoleID,
                smartPhrase.UserID ?? user.UserID, user.SelectedLocation);

            if (smartPhrase.FlowID.HasValue)
            {
                await _sqlHelper.AddFlowSmartPhrase(smartPhrase.FlowID.Value, smartPhraseId, smartPhrase.StepID, user.SelectedLocation);
            }
            else if (smartPhrase.SurgeryID.HasValue)
            {
                await _sqlHelper.AddSurgerySmartPhrase(smartPhrase.SurgeryID.Value, smartPhraseId, user.SelectedLocation);
            }

            return Ok();
        }

        // POST api/values
        [Route("api/flow/smartPhrase", Name = "EditSmartPhrase")]
        [HttpPut]
        public async Task<ActionResult> EditSmartPhrase(int smartPhraseId, [FromBody]SmartPhrasePost smartPhrase)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.EditSmartPhrase(smartPhraseId, smartPhrase.Phrase, smartPhrase.CategoryID, smartPhrase.UserID ??  user.UserID, smartPhrase.StepID, smartPhrase.RoleID, user.SelectedLocation);

            return Ok();
        }

        // POST api/values
        [Route("api/flow/smartPhrase", Name = "DeleteSmartPhrase")]
        [HttpDelete]
        public async Task<ActionResult> DeleteSmartPhrase(int smartPhraseId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.DeleteSmartPhrase(smartPhraseId, user.SelectedLocation);

            return Ok();
        }

        // POST api/values
        [Route("api/flow/flowImage", Name = "NewFlowImage")]
        [HttpPut]
        public async Task<ActionResult> NewFlowImage(IFormFile file, int flowId, int stepId, int roleId, string comment = null)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var user = await GetUserSecurity();

            // extract file name and file contents
            var fileName = file.FileName;
            var fileExtension = Path.GetExtension(fileName);
            byte[] fileContents;
            using (var stream = file.OpenReadStream())
            using (var memStream = new MemoryStream())
            {
                stream.CopyTo(memStream);

                fileContents = memStream.ToArray();
            }

            var flowImageId = await _sqlHelper.NewFlowImage(flowId, stepId, roleId, comment,
                user.ProviderID, user.LocationID);

            if (fileExtension == ".png" ||
                fileExtension == ".jpg" ||
                fileExtension == ".jpeg")
            {
                fileContents = BlobStorageHelper.CompressImage(fileContents);
            }


            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.FlowImages, flowId);
            await _blobStorageHelper.PutBlobBytes(folder, flowImageId.ToString(), fileContents);

            return Ok();
        }

        // POST api/values
        [Route("api/flow/flowImage", Name = "UpdateFlowImage")]
        [HttpPost]
        public async Task<ActionResult> UpdateFlowImage(int flowImageId, [FromBody]FlowImagePost flowImage)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.UpdateFlowImage(flowImageId, flowImage.Comment, flowImage.StepID, flowImage.RoleID, user.SelectedLocation);

            return Ok();
        }

        // POST api/values
        [Route("api/flow/rotateFlowImage", Name = "RotateFlowImage")]
        [HttpPut]
        public async Task<ActionResult> RotateFlowImage(int flowImageId, int flowId, int direction)
        {
            var user = await GetUserSecurity();
            

            // Make sure valid rotation direction
            if (direction != 1 && direction != -1)
                return Ok();


            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.FlowImages, flowId);


            await _blobStorageHelper.RotateImage(folder, flowImageId.ToString(), direction);

            return Ok();
        }

        // POST api/values
        [Route("api/flow/flowImage", Name = "DeleteFlowImage")]
        [HttpDelete]
        public async Task<ActionResult> DeleteFlowImage(int flowImageId, int flowId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.DeleteFlowImage(flowImageId, user.SelectedLocation);

            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.FlowImages, flowId);


            await _blobStorageHelper.DeleteBlob(folder, flowImageId.ToString());

            return Ok();
        }


        // POST api/values
        [Route("api/flow/flowFeedback", Name = "AddFlowFeedback")]
        [HttpPost]
        public async Task<ActionResult> AddFlowFeedback(int flowId, [FromBody]FlowFeedbackPost flowFeedback)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.AddFlowFeedback(flowId, flowFeedback.Feedback, user.UserID, user.SelectedLocation);

            return Ok();
        }


        // POST api/values
        [Route("api/flow/flowFeedback", Name = "DeleteFlowFeedback")]
        [HttpDelete]
        public async Task<ActionResult> DeleteFlowFeedback(int flowFeedbackId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.DeleteFlowFeedback(flowFeedbackId, user.SelectedLocation);

            return Ok();
        }

        // GET api/values/5
        public async Task<ActionResult> GetCardFlowList(int cardId)
        {
            var user = await GetUserSecurity();
            

            var flowList = await _sqlHelper.GetCardFlowList(cardId, user.SelectedLocation);

            return Ok(flowList);
        }

        // GET api/values/5
        public async Task<ActionResult> GetFlowInstructions(int flowId, int? surgeryId = null)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetFlowInstructions(flowId, surgeryId, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("api/flow/images")]
        public async Task<ActionResult> GetFlowImages(int flowId, int? surgeryId = null)
        {
            var user = await GetUserSecurity();
            

            var result = new FlowImageResult
            {
                FlowImages = await _sqlHelper.GetFlowImages(flowId, user.SelectedLocation)
            };

            if (surgeryId.HasValue)
            {
                result.SurgeryImages = await _sqlHelper.GetSurgeryImages(surgeryId.Value, user.SelectedLocation);
            }

            return Ok(result);
        }

        // GET api/values/5
        [Route("api/flow/timings")]
        public async Task<ActionResult> GetFlowTimings(int flowId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetFlowTimings(flowId, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("api/flow/surgerytimings")]
        public async Task<ActionResult> GetFlowSurgeryTimings(int surgeryId, int? flowId = null)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetFlowSurgeryTimings(surgeryId, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("api/flow/comments")]
        public async Task<ActionResult> GetFlowComments(int flowId, int surgeryId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetFlowComments(flowId, surgeryId, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("api/flow/messaging")]
        public async Task<ActionResult> GetFlowMessaging(int flowId, int stepId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetFlowMessaging(flowId, stepId, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("api/flow/details")]
        public async Task<ActionResult> GetFlowDetails(int flowId, int? surgeryId = null)
        {
            var user = await GetUserSecurity();
            

            var flow = await _sqlHelper.GetFlow(flowId, user.SelectedLocation);

            if (flow == null)
                return NotFound();

            var feedback = await _sqlHelper.GetFlowFeedback(flowId, user.SelectedLocation);
            var notifications = await _sqlHelper.GetFlowNotifications(flowId, null, user.SelectedLocation);
            var flowInstructions = await _sqlHelper.GetFlowInstructions(flowId, surgeryId, user.SelectedLocation);
            var content = await _sqlHelper.GetFlowContent(flowId, 1, user.SelectedLocation);
            var timings = await _sqlHelper.GetFlowTimings(flowId, user.SelectedLocation);
            var flowImages = await _sqlHelper.GetFlowImages(flowId, user.SelectedLocation);
            var surgeryDelays = await _sqlHelper.GetFlowSurgeryDelays(flowId, user.SelectedLocation);
            var surgeryImages = new List<SurgeryImage>();

            if (surgeryId.HasValue)
            {
                surgeryImages = await _sqlHelper.GetSurgeryImages(surgeryId.Value, user.SelectedLocation);
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

            return Ok(flowDetail);
        }

        // GET api/values/5
        [Route("api/flow/smartPhraseAdmin")]
        public async Task<ActionResult> GetSmartPhraseAdmin()
        {
            var user = await GetUserSecurity();
            

            var result = new SmartPhraseAdmin()
            {
                Categories = await _sqlHelper.GetSmartPhraseCategories(user.SelectedLocation),
                Specialties = await _sqlHelper.GetSpecialties(user.SelectedLocation),
                Users = await _sqlHelper.SearchUsers(null, 1, null, user.SelectedLocation)
            };

            return Ok(result);
        }

        // GET api/values/5
        [Route("api/flow/smartPhrases")]
        public async Task<ActionResult> GetSmartPhrases(int? categoryId = null, int? specialtyId = null, int? userId = null)
        {
            var user = await GetUserSecurity();
            

            var categories = await _sqlHelper.GetSmartPhrases(categoryId, specialtyId, null, user.SelectedLocation);

            if (userId.HasValue)
                categories = categories.Where(c => c.UserID == userId.Value).ToList();

            return Ok(categories);
        }

        // GET api/values/5
        [Route("api/flow/content")]
        public async Task<ActionResult> GetFlowContent(int flowId, int surgeryId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetFlowContent(flowId, surgeryId, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("api/flow/notifications")]
        public async Task<ActionResult> GetFlowNotifications(int flowId, int stepId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetFlowNotifications(flowId, stepId, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("api/flow/steps")]
        public async Task<ActionResult> GetSteps()
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetSteps(user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("api/flow/feedback")]
        public async Task<ActionResult> GetFlowFeedback(int flowId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetFlowFeedback(flowId, user.SelectedLocation);

            return Ok(result);
        }

        // PUT api/values/5
        [Route("api/flow/steps")]
        [HttpPut]
        public async Task<ActionResult> UpdateFlowSteps(int flowId, [FromBody]FlowStepPost flowStepDetail)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.DeleteFlowStep(flowId, user.SelectedLocation);

            for (var index = 0; index < flowStepDetail.FlowSteps.Count; index++)
            {
                var flowStep = flowStepDetail.FlowSteps[index];

                await _sqlHelper.InsertFlowStep(flowId, flowStep.StepID, index + 1, flowStep.StepDuration, user.ProviderID, user.LocationID);
            }

            return Ok();
        }

        // PUT api/values/5
        [Route("api/flow/surgerySteps")]
        [HttpPut]
        public async Task<ActionResult> UpdateSurgerySteps(int stepId, [FromBody]StepPost flowStepDetail)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.UpdateSurgeryStep(stepId, 
                flowStepDetail.StepName, flowStepDetail.StepNotificationType, flowStepDetail.StepTiming, user.SelectedLocation);

            return Ok();
        }

        // PUT api/values/5
        [Route("api/flow/surgerySteps")]
        [HttpPost]
        public async Task<ActionResult> AddSurgerySteps([FromBody]StepPost flowStepDetail)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.AddSurgeryStep(
                flowStepDetail.StepName, flowStepDetail.StepNotificationType, flowStepDetail.StepTiming, user.SelectedLocation);

            return Ok();
        }

        // PUT api/values/5
        [Route("api/flow/surgerySteps")]
        [HttpDelete]
        public async Task<ActionResult> DeleteSurgerySteps(int stepId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.DeleteSurgeryStep(stepId, user.SelectedLocation);

            return Ok();
        }

        // POST api/values
        [Route("api/flow/notification")]
        [HttpPost]
        public async Task<ActionResult> CreateFlowNotification(int flowId, [FromBody]FlowNotificationPost value)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.InsertFlowNotification(flowId, value.StepID, value.NotificationType, value.Message, value.SmsNumber, 
                value.EmailAddress, value.MessagingUserID, value.MessagingRoleID, user.SelectedLocation);

            return CreatedAtAction("CreateFlowNotification", result);
        }

        // POST api/values
        [Route("api/flow/notification")]
        [HttpPut]
        public async Task<ActionResult> EditFlowNotification(int flowNotificationId, [FromBody]FlowNotificationPost value)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.EditFlowNotification(flowNotificationId, value.Message, value.StepID, value.SmsNumber,
                value.EmailAddress, value.MessagingUserID, value.MessagingRoleID, user.SelectedLocation);

            return Ok(result);
        }

        // POST api/values
        [Route("api/flow/notification")]
        [HttpDelete]
        public async Task<ActionResult> DeleteFlowNotification(int flowNotificationId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.DeleteFlowNotification(flowNotificationId, user.SelectedLocation);

            return Ok(result);
        }

        // POST api/values
        public async Task<ActionResult> Post([FromBody]FlowPost value)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.NewFlow(value.CardID, value.RoomSetupID, value.FlowDescription, user.UserID, value.DefaultFlow, user.SelectedLocation);

            return Ok(result);
        }

        // PUT api/values/5
        public async Task<ActionResult> Put(int id, [FromBody]FlowPost value)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.UpdateFlow(id, value.CardID, value.RoomSetupID, value.FlowDescription, user.UserID, value.DefaultFlow, user.SelectedLocation);

            return Ok();
        }

        // DELETE api/values/5
        public async Task<ActionResult> Delete(int id)
        {
            var user = await GetUserSecurity();

            var result = await _sqlHelper.DeleteFlow(id, user.SelectedLocation);

            return Ok();
        }
    }
}