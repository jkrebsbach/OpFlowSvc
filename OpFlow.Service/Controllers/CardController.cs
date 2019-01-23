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
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    public class CardController : ApiController
    {

        // GET api/values/5
        [SwaggerOperation("GetById")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        public async Task<HttpResponseMessage> Get(int cardId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var cards = await DataAccess.SqlHelper.GetCardData(cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, cards);
        }

        [SwaggerOperation("GetById")]
        [Route("api/card/details")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardDetail))]
        public async Task<HttpResponseMessage> GetDetails(int cardId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var cards = await DataAccess.SqlHelper.GetCardData(cardId, user.ProviderID, user.LocationID);

            var card = cards.FirstOrDefault();
            if (card == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            card.CardUsers = await DataAccess.SqlHelper.GetCardUsers(cardId, user.ProviderID, user.LocationID, 1);
            card.CardItems = await DataAccess.SqlHelper.GetCardItems(cardId, user.ProviderID, user.LocationID);
            card.SurgeryAdditionalItems = await DataAccess.SqlHelper.GetCardAdditionalItems(cardId, user.ProviderID, user.LocationID);
            card.CardProcedures = await DataAccess.SqlHelper.GetCardProcedures(cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, card);
        }

        // GET api/values/5
        [SwaggerOperation("GetUsageHistory")]
        [Route("api/card/usageHistory")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardUsageHistory))]
        public async Task<HttpResponseMessage> GetUsageHistory(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await DataAccess.SqlHelper.GetCardUsageHistory(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardItems")]
        [Route("api/card/carditems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardItem>))]
        public async Task<HttpResponseMessage> GetCardItems(int cardId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await DataAccess.SqlHelper.GetCardItems(cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardSurgeryDelays")]
        [Route("api/card/delays")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        public async Task<HttpResponseMessage> GetCardSurgeryDelays(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await DataAccess.SqlHelper.GetCardSurgeryDelays(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardList")]
        [Route("api/card/list")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        public async Task<HttpResponseMessage> GetCardList(int? userId = null, int? procedureId = null, int? bundleId = null, bool? defaultFilter = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var defaultCardOnly = defaultFilter ?? false;
            var cardList = await DataAccess.SqlHelper.GetCardList(userId, procedureId, bundleId, defaultCardOnly,
                user.ProviderID, user.LocationID);
            var result = cardList.OrderBy(c => c.CardDescription).ToList();

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardCountAvgClose")]
        [Route("api/card/avgClose")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        public async Task<HttpResponseMessage> GetCardCountAvgClose(int cardId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await DataAccess.SqlHelper.GetCardCountAvgClose(cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardSurgeryOpens")]
        [Route("api/card/opens")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        public async Task<HttpResponseMessage> GetCardSurgeryOpens(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await DataAccess.SqlHelper.GetCardSurgeryOpens(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardItemPulls")]
        [Route("api/card/pulled")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardItem>))]
        public async Task<HttpResponseMessage> GetCardItemPulls(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await DataAccess.SqlHelper.GetCardItemPulls(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardSurgeryCloses")]
        [Route("api/card/closes")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardItem>))]
        public async Task<HttpResponseMessage> GetCardSurgeryCloses(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await DataAccess.SqlHelper.GetCardSurgeryCloses(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardChecklist")]
        [Route("api/card/checklist")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        public async Task<HttpResponseMessage> GetCardChecklist(int cardId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await DataAccess.SqlHelper.GetProviderCardChecklist(user.ProviderID, user.LocationID, cardId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("api/card/users")]
        [SwaggerOperation("GetCardUsers")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardUser>))]
        public async Task<HttpResponseMessage> GetCardUsers(int cardId, int? typeId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await DataAccess.SqlHelper.GetCardUsers(cardId, user.ProviderID, user.LocationID, typeId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("api/card/bundledefault")]
        [SwaggerOperation("GetBundleDefaultCardFlowRoom")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardFlowRoom))]
        public async Task<HttpResponseMessage> GetBundleDefaultCardFlowRoom(int bundleId, int? userId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await DataAccess.SqlHelper.GetBundleDefaultCardFlowRoom(bundleId, userId ?? user.UserID, user.ProviderID, user.LocationID) ??
                new CardFlowRoom()
                {
                    CardDescription = "None",
                    FlowDescription = "None",
                    RoomDescription = "None"
                };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("api/card/importSurgeons")]
        [SwaggerOperation("GetImportSurgeons")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Surgeon>))]
        public async Task<HttpResponseMessage> GetImportSurgeons()
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await DataAccess.SqlHelper.GetImportSurgeons(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("api/card/importProcedures")]
        [SwaggerOperation("GetImportProcedures")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Procedure>))]
        public async Task<HttpResponseMessage> GetImportProcedures(string importSurgeon)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await DataAccess.SqlHelper.GetImportProcedures(importSurgeon, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("api/card/proceduredefault")]
        [SwaggerOperation("GetProcedureDefaultCardFlowRoom")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardFlowRoom))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> GetProcedureDefaultCardFlowRoom(string cptCode)
        {
            var user = await CacheUtil.GetUserSecurity();

            var cards = await DataAccess.SqlHelper.GetProcedureDefaultCardFlowRoom(
                user.ProviderID, user.LocationID, cptCode);
            var result = cards.FirstOrDefault();

            return result == null ?
                Request.CreateResponse(HttpStatusCode.NotFound) :
                Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("api/card/specialtyproceduredefault")]
        [SwaggerOperation("GetSpecialtyProcedureDefaultCardFlowRoom")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardFlowRoom))]
        public async Task<HttpResponseMessage> GetSpecialtyProcedureDefaultCardFlowRoom(string cptCode)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await DataAccess.SqlHelper.GetSpecialtyProcedureDefaultCardFlowRoom(user.ProviderID, user.LocationID, cptCode);

            return Request.CreateResponse(HttpStatusCode.OK, result.FirstOrDefault());
        }

        // GET api/values/5
        [Route("api/card/multipleproceduresdefault")]
        [SwaggerOperation("GetMultipleProceduresDefaultCardFlowRoom")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardFlowRoom>))]
        public async Task<HttpResponseMessage> GetMultipleProceduresDefaultCardFlowRoom(List<string> cptCodes)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = new List<CardFlowRoom>();

            foreach (var cptCode in cptCodes)
            {
                var procedures = await DataAccess.SqlHelper.GetProcedureDefaultCardFlowRoom(user.ProviderID, user.LocationID, cptCode);

                result.AddRange(procedures);
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("api/card/specialtymultipleproceduresdefault")]
        [SwaggerOperation("GetSpecialtyMultipleProceduresDefaultCardFlowRoom")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardFlowRoom>))]
        public async Task<HttpResponseMessage> GetSpecialtyMultipleProceduresDefaultCardFlowRoom(int specialtyId, string cptCodes)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await DataAccess.SqlHelper.GetSpecialtyMultipleProceduresDefaultCardFlowRoom(user.ProviderID, user.LocationID, specialtyId, cptCodes);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // PUT api/values
        [SwaggerOperation("UpdateQuantity")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/card/updateQuantity", Name = "UpdateQuantity")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateItemQty(int cardId, [FromBody]CardQuantityEdit cardQuantity)
        {
            var user = await CacheUtil.GetUserSecurity();

            await DataAccess.SqlHelper.UpdateCardQuantity(cardId, cardQuantity, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("AssignFlow")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/card/assignFlow", Name = "AssignFlowCard")]
        public async Task<HttpResponseMessage> AssignToCard(int cardId, int flowId)
        {
            var user = await CacheUtil.GetUserSecurity();

            await DataAccess.SqlHelper.AssignFlowToCard(flowId, cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, 418);
        }

        // POST api/values
        [SwaggerOperation("AssignRoomSetup")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/card/assignRoomSetup", Name = "AssignRoomSetupCard")]
        public async Task<HttpResponseMessage> AssignRoomSetupToCard(int cardId, int roomSetupId)
        {
            var user = await CacheUtil.GetUserSecurity();

            await DataAccess.SqlHelper.AssignRoomSetupToCard(roomSetupId, cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, 418);
        }

        [SwaggerOperation("AssignCardUser")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/card/cardUser", Name = "AssignCardUser")]
        [HttpPost]
        public async Task<IHttpActionResult> AssignCardUser(int cardId, int userId)
        {
            var user = await CacheUtil.GetUserSecurity();

            await DataAccess.SqlHelper.AssignUserToCard(cardId, userId, user.ProviderID, user.LocationID);

            return Ok();
        }

        [SwaggerOperation("DeleteCardUser")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/card/cardUser", Name = "DeleteCardUser")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteCardUser(int cardId, int userId)
        {
            var user = await CacheUtil.GetUserSecurity();

            await DataAccess.SqlHelper.RemoveUserFromCard(cardId, userId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // PUT api/values
        [SwaggerOperation("DeleteCardItem")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(int))]
        [Route("api/card/cardItem", Name = "DeleteCardItem")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteCardItem(int cardId, int itemId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await DataAccess.SqlHelper.DeleteCardItem(cardId, itemId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // PUT api/values
        [SwaggerOperation("AssignCardItem")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(int))]
        [Route("api/card/cardItem", Name = "AssignCardItem")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutCardItem(int cardId, int itemId, [FromBody]CardItemPost value)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await DataAccess.SqlHelper.UpdateCardItem(cardId, itemId, value.OpenQty, value.HoldQty, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // PUT api/values
        [SwaggerOperation("AssignCardProcedure")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(int))]
        [Route("api/card/cardProcedure", Name = "AssignCardProcedure")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> PutCardProcedure(int cardId, int procedureId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await DataAccess.SqlHelper.UpdateCardProcedure(cardId, procedureId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // PUT api/values
        [SwaggerOperation("DeleteCardProcedure")]
        [Route("api/card/cardProcedure", Name = "DeleteCardProcedure")]
        [HttpDelete]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> DeleteCardProcedure(int cardId, int procedureId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await DataAccess.SqlHelper.DeleteCardProcedure(cardId, procedureId, user.ProviderID, user.LocationID);

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

                var cardId = await DataAccess.SqlHelper.InsertCard(value.Description, value.OwnerUserID,
                    value.SpecialtyID, value.ProcedureID, value.TemplateFlowID, value.TemplateRoomSetupID,
                    value.BundleID, value.BundleFlag, value.DefaultFlag == "1", value.SpecialtyDefaultFlag == "1",
                    user.ProviderID, user.LocationID);

                await InitializeCardProcedures(cardId, user, value.Procedures);

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

                if (value.Procedures != null)
                {
                    await InitializeCardProcedures(id, user, value.Procedures);
                }
                else
                {
                    await DataAccess.SqlHelper.UpdateCard(id, value.Description, value.OwnerUserID, value.SpecialtyID, value.ProcedureID, value.TemplateFlowID, value.TemplateRoomSetupID,
                        value.BundleID, value.BundleFlag, value.DefaultFlag == "1", value.SpecialtyDefaultFlag == "1",
                        user.UserID, user.ProviderID, user.LocationID);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Models.LogHelper.LogException(ex);
                throw;
            }
        }

        private async Task InitializeCardProcedures(int cardId, UserSecurity user, List<CardPostImportProcedure> procedures)
        {
            if (procedures != null)
            {
                await DataAccess.SqlHelper.InitializeCard(cardId, user.ProviderID, user.LocationID);

                foreach (var procedure in procedures)
                {
                    if (!string.IsNullOrEmpty(procedure.ImportSurgeon) && !string.IsNullOrEmpty(procedure.ImportProcedure))
                    {
                        await DataAccess.SqlHelper.InsertCardItemFromStage(cardId, user.ProviderID, user.LocationID,
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

            await DataAccess.SqlHelper.DeleteCard(id, user.ProviderID, user.LocationID);

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