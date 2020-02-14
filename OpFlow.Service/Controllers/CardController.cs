using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Mindscape.Raygun4Net;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    [RoutePrefix("api/card")]
    public class CardController : ApiController
    {

        // GET api/values/5
        [SwaggerOperation("GetById")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        public async Task<HttpResponseMessage> Get(int cardId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var cards = await sqlHelper.GetCardData(cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, cards);
        }

        [SwaggerOperation("GetById")]
        [Route("details")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardDetail))]
        public async Task<HttpResponseMessage> GetDetails(int cardId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var cards = await sqlHelper.GetCardData(cardId, user.ProviderID, user.LocationID);

            var card = cards.FirstOrDefault();
            if (card == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            card.CardUsers = await sqlHelper.GetCardUsers(cardId, user.ProviderID, user.LocationID, 1);
            card.CardItems = await sqlHelper.GetCardItems(cardId, user.ProviderID, user.LocationID);
            card.SurgeryAdditionalItems = await sqlHelper.GetCardAdditionalItems(cardId, user.ProviderID, user.LocationID);
            card.CardProcedures = await sqlHelper.GetCardProcedures(cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, card);
        }

        // GET api/values/5
        [SwaggerOperation("GetUsageHistory")]
        [Route("usageHistory")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardUsageHistory))]
        public async Task<HttpResponseMessage> GetUsageHistory(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetCardUsageHistory(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardItems")]
        [Route("carditems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardItem>))]
        public async Task<HttpResponseMessage> GetCardItems(int cardId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetCardItems(cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardSurgeryDelays")]
        [Route("delays")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        public async Task<HttpResponseMessage> GetCardSurgeryDelays(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetCardSurgeryDelays(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardList")]
        [Route("list")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        public async Task<HttpResponseMessage> GetCardList(int? userId = null, int? specialtyId = null, int? procedureId = null, int? bundleId = null, bool? defaultFilter = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var defaultCardOnly = defaultFilter ?? false;
            var cardList = await sqlHelper.GetCardList(userId, specialtyId, procedureId, bundleId, defaultCardOnly,
                user.ProviderID, user.LocationID);
            var result = cardList.OrderBy(c => c.CardDescription).ToList();

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("UsedCardList")]
        [Route("listUsed")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        [HttpPost]
        public async Task<HttpResponseMessage> UsedCardList([FromBody] UsedCardSearchPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var cardList = await sqlHelper.GetUsedCardList(post.UserIDs, post.TrayIDs, post.CategoryIDs,
                user.ProviderID, user.LocationID);
            var result = cardList.OrderBy(c => c.CardDescription).ToList();

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetEditFeedback")]
        [Route("feedback")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardItemFeedback>))]
        [HttpGet]
        public async Task<HttpResponseMessage> GetEditFeedback(int? specialtyId = null, int? userId = null, int? cardId = null, DateTime? beginDate = null, DateTime? endDate = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetCardFeedback(specialtyId, userId, cardId, beginDate, endDate,
                user.ProviderID, user.LocationID);
            
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardCategories")]
        [Route("cardCategories")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardCategory>))]
        public async Task<HttpResponseMessage> GetCardCategories()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetCardCategories(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCptCodes")]
        [Route("cptCode")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Procedure>))]
        public async Task<HttpResponseMessage> GetCptCodes()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetCptCodes(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetProcedureProfile")]
        [Route("procedureProfile")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ProcedureProfile))]
        public async Task<HttpResponseMessage> GetProcedureProfile(int cardCategoryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetProcedureProfile(cardCategoryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PostEditFeedback")]
        [Route("feedback")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        public async Task<HttpResponseMessage> PostEditFeedback(int feedbackId, [FromBody]FeedbackRequest post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateCardFeedback(feedbackId, post.Response, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("users")]
        [SwaggerOperation("GetCardUsers")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardUser>))]
        public async Task<HttpResponseMessage> GetCardUsers(int cardId, int? typeId = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetCardUsers(cardId, user.ProviderID, user.LocationID, typeId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("bundledefault")]
        [SwaggerOperation("GetBundleDefaultCardFlowRoom")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardFlowRoom))]
        public async Task<HttpResponseMessage> GetBundleDefaultCardFlowRoom(int bundleId, int? userId = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetBundleDefaultCardFlowRoom(bundleId, userId ?? user.UserID, user.ProviderID, user.LocationID) ??
                new CardFlowRoom()
                {
                    CardDescription = "None",
                    FlowDescription = "None",
                    RoomDescription = "None"
                };

            result.Cards = await sqlHelper.GetCardList(userId, null, null, bundleId, false, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("importSurgeons")]
        [SwaggerOperation("GetImportSurgeons")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Surgeon>))]
        public async Task<HttpResponseMessage> GetImportSurgeons()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetImportSurgeons(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("importProcedures")]
        [SwaggerOperation("GetImportProcedures")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Procedure>))]
        public async Task<HttpResponseMessage> GetImportProcedures(string importSurgeon)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetImportProcedures(importSurgeon, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("proceduredefault")]
        [SwaggerOperation("GetProcedureDefaultCardFlowRoom")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardFlowRoom))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> GetProcedureDefaultCardFlowRoom(string cptCode)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var cards = await sqlHelper.GetProcedureDefaultCardFlowRoom(
                user.ProviderID, user.LocationID, cptCode);
            var result = cards.FirstOrDefault();

            return result == null ?
                Request.CreateResponse(HttpStatusCode.NotFound) :
                Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("specialtyproceduredefault")]
        [SwaggerOperation("GetSpecialtyProcedureDefaultCardFlowRoom")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardFlowRoom))]
        public async Task<HttpResponseMessage> GetSpecialtyProcedureDefaultCardFlowRoom(string cptCode)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetSpecialtyProcedureDefaultCardFlowRoom(user.ProviderID, user.LocationID, cptCode);

            return Request.CreateResponse(HttpStatusCode.OK, result.FirstOrDefault());
        }

        // GET api/values/5
        [Route("multipleproceduresdefault")]
        [SwaggerOperation("GetMultipleProceduresDefaultCardFlowRoom")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardFlowRoom>))]
        public async Task<HttpResponseMessage> GetMultipleProceduresDefaultCardFlowRoom(List<string> cptCodes)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = new List<CardFlowRoom>();

            foreach (var cptCode in cptCodes)
            {
                var procedures = await sqlHelper.GetProcedureDefaultCardFlowRoom(user.ProviderID, user.LocationID, cptCode);

                result.AddRange(procedures);
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("specialtymultipleproceduresdefault")]
        [SwaggerOperation("GetSpecialtyMultipleProceduresDefaultCardFlowRoom")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardFlowRoom>))]
        public async Task<HttpResponseMessage> GetSpecialtyMultipleProceduresDefaultCardFlowRoom(int specialtyId, string cptCodes)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetSpecialtyMultipleProceduresDefaultCardFlowRoom(user.ProviderID, user.LocationID, specialtyId, cptCodes);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // PUT api/values
        [SwaggerOperation("UpdateQuantity")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("updateQuantity", Name = "UpdateQuantity")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateItemQty(int cardId, [FromBody]CardQuantityEdit cardQuantity)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.UpdateCardQuantity(cardId, cardQuantity, user.ProviderID, user.LocationID);

            return Ok();
        }

        // PUT api/values
        [SwaggerOperation("UpdateQuantityRequest")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("updateQuantityRequest", Name = "UpdateQuantityRequest")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateItemQtyRequest(int cardId, int surgeryId, [FromBody]CardQuantityEditRequest cardQuantity)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            foreach (var editRequest in cardQuantity.EditData)
            {
                if (cardQuantity.Target == "C")
                    await sqlHelper.UpdateCardQuantityRequest(cardId, editRequest, user.ProviderID, user.LocationID);
                else if (editRequest.TrayID.HasValue)
                    await sqlHelper.AddCustomSurgeryTrayItem(surgeryId, editRequest.TrayID.Value, editRequest.ItemID, editRequest.OpenQty ?? 0, user.ProviderID, user.LocationID);
                else
                    await sqlHelper.UpdateSurgeryItemQuantity(surgeryId, editRequest, user.ProviderID, user.LocationID);
            }

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("AssignFlow")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("assignFlow", Name = "AssignFlowCard")]
        public async Task<HttpResponseMessage> AssignToCard(int cardId, int flowId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.AssignFlowToCard(flowId, cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, 418);
        }

        // POST api/values
        [SwaggerOperation("AssignRoomSetup")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("assignRoomSetup", Name = "AssignRoomSetupCard")]
        public async Task<HttpResponseMessage> AssignRoomSetupToCard(int cardId, int roomSetupId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.AssignRoomSetupToCard(roomSetupId, cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, 418);
        }

        [SwaggerOperation("AssignCardUser")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("cardUser", Name = "AssignCardUser")]
        [HttpPost]
        public async Task<IHttpActionResult> AssignCardUser(int cardId, int userId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.AssignUserToCard(cardId, userId, user.ProviderID, user.LocationID);

            return Ok();
        }

        [SwaggerOperation("DeleteCardUser")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("cardUser", Name = "DeleteCardUser")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteCardUser(int cardId, int userId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.RemoveUserFromCard(cardId, userId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // PUT api/values
        [SwaggerOperation("DeleteCardItem")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(int))]
        [Route("cardItem", Name = "DeleteCardItem")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteCardItem(int cardId, int itemId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.DeleteCardItem(cardId, itemId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // PUT api/values
        [SwaggerOperation("AssignCardItem")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(int))]
        [Route("cardItem", Name = "AssignCardItem")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutCardItem(int cardId, int itemId, [FromBody]CardItemPost value)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateCardItem(cardId, itemId, value.OpenQty, value.HoldQty, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // PUT api/values
        [SwaggerOperation("AssignCardProcedure")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(int))]
        [Route("cardProcedure", Name = "AssignCardProcedure")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> PutCardProcedure(int cardId, int procedureId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateCardProcedure(cardId, procedureId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // PUT api/values
        [SwaggerOperation("DeleteCardProcedure")]
        [Route("cardProcedure", Name = "DeleteCardProcedure")]
        [HttpDelete]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> DeleteCardProcedure(int cardId, int procedureId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.DeleteCardProcedure(cardId, procedureId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(int))]
        public async Task<HttpResponseMessage> Post([FromBody]CardPost value)
        {
            try
            {
                var user = await CacheUtil.GetUserSecurity();
                var sqlHelper = new SqlHelper();

                var cardCategoryId = await sqlHelper.ParseCardCategory(value.CardCategory, user.ProviderID, user.LocationID);

                var cardId = await sqlHelper.InsertCard(value.Description, value.OwnerUserID,
                    value.SpecialtyID, value.ProcedureID, value.TemplateFlowID, value.TemplateRoomSetupID,
                    value.BundleID, value.BundleFlag, cardCategoryId,
                    value.DefaultFlag == "1", value.SpecialtyDefaultFlag == "1",
                    user.ProviderID, user.LocationID);

                await InitializeCardProcedures(sqlHelper, cardId, user, value.Procedures);

                return Request.CreateResponse(HttpStatusCode.Created, cardId);
            }
            catch (Exception ex)
            {
                Models.LogHelper.LogException(ex);
                throw;
            }
        }

        // PUT api/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Put(int id, [FromBody]CardPost value)
        {
            try
            {
                var user = await CacheUtil.GetUserSecurity();
                var sqlHelper = new SqlHelper();

                var cardCategoryId = await sqlHelper.ParseCardCategory(value.CardCategory, user.ProviderID, user.LocationID);

                if (value.Procedures != null)
                {
                    await InitializeCardProcedures(sqlHelper, id, user, value.Procedures);
                }
                else
                {
                    await sqlHelper.UpdateCard(id, value.Description, value.SpecialtyID, value.ProcedureID, value.TemplateFlowID, value.TemplateRoomSetupID,
                        value.BundleID, value.BundleFlag, cardCategoryId,
                        value.DefaultFlag == "1", value.SpecialtyDefaultFlag == "1",
                        user.ProviderID, user.LocationID);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Models.LogHelper.LogException(ex);
                throw;
            }
        }

        private async Task InitializeCardProcedures(SqlHelper sqlHelper, int cardId, UserSecurity user, List<CardPostImportProcedure> procedures)
        {
            if (procedures != null)
            {
                await sqlHelper.InitializeCard(cardId, user.ProviderID, user.LocationID);

                foreach (var procedure in procedures)
                {
                    if (!string.IsNullOrEmpty(procedure.ImportSurgeon) && !string.IsNullOrEmpty(procedure.ImportProcedure))
                    {
                        await sqlHelper.InsertCardItemFromStage(cardId, user.ProviderID, user.LocationID,
                            procedure.ImportProcedure, procedure.ImportSurgeon);
                    }

                }
            }
        }

        // DELETE api/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Delete(int id)
        {
            try
            {
            var user = await CacheUtil.GetUserSecurity();
                var sqlHelper = new SqlHelper();

                await sqlHelper.DeleteCard(id, user.ProviderID, user.LocationID);

            return Ok();
            }
            catch (Exception ex)
            {
                Models.LogHelper.LogException(ex);
                throw;
            }
        }
    }
}