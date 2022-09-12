using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
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

            var cards = await sqlHelper.GetCardData(cardId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, cards);
        }

        [SwaggerOperation("GetById")]
        [Route("details")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardDetail))]
        public async Task<HttpResponseMessage> GetDetails(int? cardId = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            CardDetail card = null;
            if (cardId.HasValue)
            {
                var cards = await sqlHelper.GetCardData(cardId.Value, user.SelectedLocation);

                card = cards.FirstOrDefault();
                if (card == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                card.CardUsers = await sqlHelper.GetCardUsers(cardId.Value, user.SelectedLocation, 1);
                card.CardItems = await sqlHelper.GetCardItems(cardId.Value, user.SelectedLocation);
                card.SurgeryAdditionalItems = await sqlHelper.GetCardAdditionalItems(cardId.Value, user.SelectedLocation);
                card.CardProcedures = await sqlHelper.GetCardProcedures(cardId.Value, user.SelectedLocation);
                card.CardCategories = await sqlHelper.GetCardCategoryXRef(cardId.Value, user.SelectedLocation);
            }

            var cardCategories = await sqlHelper.GetCardCategories();
            var surgeons = await sqlHelper.SearchUsers(null, 1, null, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                CardCategories = cardCategories,
                Card = card,
                Surgeons = surgeons
            });
        }

        // GET api/values/5
        [SwaggerOperation("GetUsageHistory")]
        [Route("usageHistory")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardUsageHistory))]
        public async Task<HttpResponseMessage> GetUsageHistory(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetCardUsageHistory(surgeryId, user.SelectedLocation);

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

            var result = await sqlHelper.GetCardItems(cardId, user.SelectedLocation);

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

            var result = await sqlHelper.GetCardSurgeryDelays(surgeryId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardList")]
        [Route("list")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardListScreen>))]
        public async Task<HttpResponseMessage> GetCardList(int? userId = null, int? specialtyId = null, int? procedureId = null, 
            string cardName = null, bool? defaultFilter = null, int? trayItemId = null, string additionalData = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var defaultCardOnly = defaultFilter ?? false;
            var cardList = await sqlHelper.GetCardList(userId, specialtyId, procedureId, null, cardName, trayItemId, 
                defaultCardOnly, additionalData, user.SelectedLocation);
            var result = cardList.OrderBy(c => c.CardDescription).ToList();

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardsPaged")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(PaginationController))]
        [Route("cardsPaged")]
        public async Task<HttpResponseMessage> GetCardsPaged(string additionalData = null, string term = null, int page = 1)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var instruments = await sqlHelper.GetCardsPaged(additionalData, term, page, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, instruments);
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

            var cardList = await sqlHelper.GetUsedCardList(post.UserIDs, post.TrayIDs, post.CategoryIDs, user.SelectedLocation);
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
                user.SelectedLocation);
            
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardCategories")]
        [Route("cardCategories")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardCategory>))]
        public async Task<HttpResponseMessage> GetCardCategories()
        {
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetCardCategories();

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PostCardCategory")]
        [Route("cardCategory")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpPost]
        public async Task<HttpResponseMessage> PostCardCategory(string cardCategoryName)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var result = await sqlHelper.ParseCardCategory(cardCategoryName, 1);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("DeleteCardCategory")]
        [Route("cardCategory")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteCardCategory(int cardCategoryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var result = await sqlHelper.DeleteCardCategory(cardCategoryId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardCategoryDetails")]
        [Route("cardCategoryDetails")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardCategoryDetail>))]
        public async Task<HttpResponseMessage> GetCardCategoryDetails()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var cardCategories = await sqlHelper.GetCardCategoryDetails(
                null, null, null, null,
                user.SelectedLocation);
            var specialties = await sqlHelper.GetSpecialties(user.SelectedLocation);
            var surgeons = await sqlHelper.GetSurgeons(null, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, new {
                Specialties = specialties,
                Surgeons = surgeons,
                CardCategories = cardCategories
            });
        }

        // GET api/values/5
        [SwaggerOperation("FilterCardCategoryDetails")]
        [Route("cardCategoryDetails")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardCategoryDetail>))]
        [HttpPost]
        public async Task<HttpResponseMessage> PostCardCategoryDetails([FromBody] CardCategoryDetailPost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var cardCategories = await sqlHelper.GetCardCategoryDetails(
                request.CardCategoryID, request.SpecialtyID, request.CardID, request.SurgeonID,
                user.SelectedLocation);
            
            return Request.CreateResponse(HttpStatusCode.OK, cardCategories);
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

            var result = await sqlHelper.UpdateCardFeedback(feedbackId, post.Response, user.SelectedLocation);

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

            var result = await sqlHelper.GetCardUsers(cardId, user.SelectedLocation, typeId);

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

            var result = await sqlHelper.GetBundleDefaultCardFlowRoom(bundleId, userId ?? user.UserID, user.SelectedLocation) ??
                new CardFlowRoom()
                {
                    CardDescription = "None",
                    FlowDescription = "None",
                    RoomDescription = "None"
                };

            result.Cards = await sqlHelper.GetCardList(userId, null, null, bundleId, null, null, false, null, user.SelectedLocation);

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

            var result = await sqlHelper.GetImportSurgeons(user.SelectedLocation);

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

            var result = await sqlHelper.GetImportProcedures(importSurgeon, user.SelectedLocation);

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

            var cards = await sqlHelper.GetProcedureDefaultCardFlowRoom(cptCode, user.SelectedLocation);
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

            var result = await sqlHelper.GetSpecialtyProcedureDefaultCardFlowRoom(user.SelectedLocation, cptCode);

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
                var procedures = await sqlHelper.GetProcedureDefaultCardFlowRoom(cptCode, user.SelectedLocation);

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

            var result = await sqlHelper.GetSpecialtyMultipleProceduresDefaultCardFlowRoom(user.SelectedLocation, specialtyId, cptCodes);

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

            await sqlHelper.UpdateCardQuantity(cardId, cardQuantity, user.SelectedLocation);

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
                    await sqlHelper.UpdateCardQuantityRequest(cardId, editRequest, user.SelectedLocation);
                else if (editRequest.TrayID.HasValue)
                    await sqlHelper.AddCustomSurgeryTrayItem(surgeryId, editRequest.TrayID.Value, editRequest.ItemID, editRequest.OpenQty ?? 0, user.SelectedLocation);
                else
                    await sqlHelper.UpdateSurgeryItemQuantity(surgeryId, editRequest, user.SelectedLocation);
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

            await sqlHelper.AssignFlowToCard(flowId, cardId, user.SelectedLocation);

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

            await sqlHelper.AssignRoomSetupToCard(roomSetupId, cardId, user.SelectedLocation);

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

            await sqlHelper.AssignUserToCard(cardId, userId, user.SelectedLocation);

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

            await sqlHelper.RemoveUserFromCard(cardId, userId, user.SelectedLocation);

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

            var result = await sqlHelper.DeleteCardItem(cardId, itemId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // PUT api/values
        [SwaggerOperation("AssignCardItem")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(int))]
        [Route("cardItem", Name = "AssignCardItem")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutCardItem(int cardId, [FromBody]CardItemPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = -1;

            if (post.ItemType == "tray-group")
            {
                var trayGroups = await sqlHelper.GetTrayGroups(user.SelectedLocation);
                var trayGroup = trayGroups.First(t => t.TrayGroupID == post.ItemID);

                foreach (var tray in trayGroup.Trays)
                {
                    result = await sqlHelper.UpdateCardItem(cardId, tray.TrayItemID, 0, 1, user.SelectedLocation);
                }
            }
            else
            {

                if (post.ItemID == null)
                    post.ItemID = await sqlHelper.InsertItemMaster("SUPPLY", post.ItemName, user.SelectedLocation);

                result = await sqlHelper.UpdateCardItem(cardId, post.ItemID.Value, post.OpenQty, post.HoldQty, user.SelectedLocation);
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
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

            var result = await sqlHelper.UpdateCardProcedure(cardId, procedureId, user.SelectedLocation);

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

            var result = await sqlHelper.DeleteCardProcedure(cardId, procedureId, user.SelectedLocation);

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

                var cardCategoryId = await sqlHelper.ParseCardCategory(value.CardCategory, user.SelectedLocation);

                var cardId = await sqlHelper.InsertCard(value.Description, value.OwnerUserID,
                    value.SpecialtyID, value.ProcedureID, value.TemplateFlowID, value.TemplateRoomSetupID,
                    value.BundleID, value.BundleFlag, cardCategoryId,
                    value.DefaultFlag == "1", value.SpecialtyDefaultFlag == "1",
                    user.SelectedLocation);

                await InitializeCardProcedures(sqlHelper, cardId, user, value.Procedures);

                await sqlHelper.UpdateCardCategoryXRef(cardId, value.CardCategories, user.SelectedLocation);

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

                var cardCategoryId = await sqlHelper.ParseCardCategory(value.CardCategory, user.SelectedLocation);

                if (value.Procedures != null)
                {
                    await InitializeCardProcedures(sqlHelper, id, user, value.Procedures);
                }
                else
                {
                    await sqlHelper.UpdateCard(id, value.Description, value.SpecialtyID, value.ProcedureID, value.TemplateFlowID, value.TemplateRoomSetupID,
                        value.BundleID, value.BundleFlag, cardCategoryId,
                        value.DefaultFlag == "1", value.SpecialtyDefaultFlag == "1",
                        user.SelectedLocation);
                }

                await sqlHelper.UpdateCardCategoryXRef(id, value.CardCategories, user.SelectedLocation);

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
                await sqlHelper.InitializeCard(cardId, user.SelectedLocation);

                foreach (var procedure in procedures)
                {
                    if (!string.IsNullOrEmpty(procedure.ImportSurgeon) && !string.IsNullOrEmpty(procedure.ImportProcedure))
                    {
                        await sqlHelper.InsertCardItemFromStage(cardId, user.SelectedLocation,
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

                await sqlHelper.DeleteCard(id, user.SelectedLocation);

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