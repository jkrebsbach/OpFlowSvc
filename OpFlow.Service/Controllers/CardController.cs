using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
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
        public HttpResponseMessage Get(int cardId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var cards = DataAccess.SqlHelper.GetCardData(cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, cards);
        }

        [SwaggerOperation("GetById")]
        [Route("api/card/details")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardDetail))]
        public HttpResponseMessage Get(int cardId)
        {
            var user = CacheUtil.GetUserSecurity();

            var cards = DataAccess.SqlHelper.GetCardData(cardId, user.ProviderID, user.LocationID);

            var card = cards.FirstOrDefault();
            if (card == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            card.CardUsers = DataAccess.SqlHelper.GetCardUsers(cardId, user.ProviderID, user.LocationID, 1);
            card.CardItems = DataAccess.SqlHelper.GetCardItems(cardId, user.ProviderID, user.LocationID);
            card.SurgeryAdditionalItems = DataAccess.SqlHelper.GetCardAdditionalItems(cardId, user.ProviderID, user.LocationID);
            card.CardProcedures = DataAccess.SqlHelper.GetCardProcedures(cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, card);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardItems")]
        [Route("api/card/carditems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardItem>))]
        public HttpResponseMessage GetCardItems(int cardId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetCardItems(cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardSurgeryDelays")]
        [Route("api/card/delays")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        public HttpResponseMessage GetCardSurgeryDelays(int surgeryId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetCardSurgeryDelays(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardList")]
        [Route("api/card/list")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        public HttpResponseMessage GetCardList(int? userId = null, int? procedureId = null, int? bundleId = null, bool? defaultFilter = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var defaultCardOnly = defaultFilter ?? false;
            var result = DataAccess.SqlHelper.GetCardList(userId, procedureId, bundleId, defaultCardOnly, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardCountAvgClose")]
        [Route("api/card/avgClose")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        public HttpResponseMessage GetCardCountAvgClose(int cardId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetCardCountAvgClose(cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardSurgeryOpens")]
        [Route("api/card/opens")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        public HttpResponseMessage GetCardSurgeryOpens(int surgeryId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetCardSurgeryOpens(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardItemPulls")]
        [Route("api/card/pulled")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardItem>))]
        public HttpResponseMessage GetCardItemPulls(int surgeryId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetCardItemPulls(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardSurgeryCloses")]
        [Route("api/card/closes")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardItem>))]
        public HttpResponseMessage GetCardSurgeryCloses(int surgeryId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetCardSurgeryCloses(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardChecklist")]
        [Route("api/card/checklist")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        public HttpResponseMessage GetCardChecklist(int cardId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetProviderCardChecklist(user.ProviderID, user.LocationID, cardId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("api/card/users")]
        [SwaggerOperation("GetCardUsers")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardUser>))]
        public HttpResponseMessage GetCardUsers(int cardId, int? providerId = null, int? locationId = null, int? typeId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetCardUsers(cardId, user.ProviderID, user.LocationID, typeId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("api/card/bundledefault")]
        [SwaggerOperation("GetBundleDefaultCardFlowRoom")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardFlowRoom))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetBundleDefaultCardFlowRoom(int bundleId)
        {
            var result = DataAccess.SqlHelper.GetBundleDefaultCardFlowRoom(bundleId).FirstOrDefault();

            return result == null ?
                Request.CreateResponse(HttpStatusCode.NotFound) :
                Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("api/card/proceduredefault")]
        [SwaggerOperation("GetProcedureDefaultCardFlowRoom")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardFlowRoom))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetProcedureDefaultCardFlowRoom(string cptCode, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetProcedureDefaultCardFlowRoom(user.ProviderID, user.LocationID, cptCode).FirstOrDefault();

            return result == null ?
                Request.CreateResponse(HttpStatusCode.NotFound) :
                Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("api/card/specialtyproceduredefault")]
        [SwaggerOperation("GetSpecialtyProcedureDefaultCardFlowRoom")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardFlowRoom))]
        public HttpResponseMessage GetSpecialtyProcedureDefaultCardFlowRoom(string cptCode, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetSpecialtyProcedureDefaultCardFlowRoom(user.ProviderID, user.LocationID, cptCode);

            return Request.CreateResponse(HttpStatusCode.OK, result.FirstOrDefault());
        }

        // GET api/values/5
        [Route("api/card/multipleproceduresdefault")]
        [SwaggerOperation("GetMultipleProceduresDefaultCardFlowRoom")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardFlowRoom>))]
        public HttpResponseMessage GetMultipleProceduresDefaultCardFlowRoom(List<string> cptCodes, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = new List<CardFlowRoom>();

            foreach (var cptCode in cptCodes)
            {
                var procedures = DataAccess.SqlHelper.GetProcedureDefaultCardFlowRoom(user.ProviderID, user.LocationID, cptCode);

                result.AddRange(procedures);
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("api/card/specialtymultipleproceduresdefault")]
        [SwaggerOperation("GetSpecialtyMultipleProceduresDefaultCardFlowRoom")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardFlowRoom>))]
        public HttpResponseMessage GetSpecialtyMultipleProceduresDefaultCardFlowRoom(int specialtyId, string cptCodes, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetSpecialtyMultipleProceduresDefaultCardFlowRoom(user.ProviderID, user.LocationID, specialtyId, cptCodes);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // PUT api/values
        [SwaggerOperation("UpdateQuantity")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/card/updateQuantity", Name = "UpdateQuantity")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateItemQty(int cardId, [FromBody]CardQuantityEdit cardQuantity)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.UpdateCardQuantity(cardId, cardQuantity, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("AssignFlow")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/card/assignFlow", Name = "AssignFlowCard")]
        public HttpResponseMessage AssignToCard(int cardId, int flowId)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.AssignFlowToCard(flowId, cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, 418);
        }

        // POST api/values
        [SwaggerOperation("AssignRoomSetupCard")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/card/assignRoomSetup", Name = "AssignRoomSetupCard")]
        public async Task<IHttpActionResult> AssignRoomSetupToCard(int cardId, int roomSetupId)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.AssignRoomSetupToCard(roomSetupId, cardId, user.ProviderID, user.LocationID);

            return Ok();
        }

        [SwaggerOperation("AssignCardUser")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/card/cardUser", Name = "AssignCardUser")]
        [HttpPost]
        public async Task<IHttpActionResult> AssignCardUser(int cardId, int userId)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.AssignUserToCard(cardId, userId, user.ProviderID, user.LocationID);

            return Ok();
        }

        [SwaggerOperation("DeleteCardUser")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/card/cardUser", Name = "DeleteCardUser")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteCardUser(int cardId, int userId)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.RemoveUserFromCard(cardId, userId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // PUT api/values
        [SwaggerOperation("AssignCardItem")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(int))]
        [Route("api/card/cardItem", Name = "AssignCardItem")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutCardItem(int cardId, int itemId, [FromBody]CardItemPost value)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.UpdateCardItem(cardId, itemId, value.OpenQty, value.HoldQty, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // PUT api/values
        [SwaggerOperation("AssignCardProcedure")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(int))]
        [Route("api/card/cardProcedure", Name = "AssignCardProcedure")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> PutCardProcedure(int cardId, string cptCode)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.UpdateCardProcedure(cardId, cptCode, user.ProviderID, user.LocationID);

            return result.HasValue ? 
                Request.CreateResponse(HttpStatusCode.OK, result) : 
                Request.CreateResponse(HttpStatusCode.NotFound, "Invalid CPT Code");
        }

        // PUT api/values
        [SwaggerOperation("DeleteCardProcedure")]
        [Route("api/card/cardProcedure", Name = "DeleteCardProcedure")]
        [HttpDelete]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> PutCardProcedure(int cardId, int procedureId)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.DeleteCardProcedure(cardId, procedureId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(int))]
        public async Task<HttpResponseMessage> Post([FromBody]CardPost value)
        {
            var user = CacheUtil.GetUserSecurity();

            var cardId = DataAccess.SqlHelper.InsertCard(value.Description, value.OwnerUserID, value.SpecialtyID, value.ProcedureID, value.TemplateFlowID, value.TemplateRoomID,
                value.BundleID, value.BundleFlag, value.DefaultFlag == "1", value.SpecialtyDefaultFlag == "1", 
                user.UserID, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.Created, cardId);
        }

        // PUT api/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Put(int id, [FromBody]CardPost value)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.UpdateCard(id, value.Description, value.OwnerUserID, value.SpecialtyID, value.ProcedureID, value.TemplateFlowID, value.TemplateRoomID,
                value.BundleID, value.BundleFlag, value.DefaultFlag == "1", value.SpecialtyDefaultFlag == "1", 
                user.UserID, user.ProviderID, user.LocationID);

            return Ok();
        }

        // DELETE api/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Delete(int id)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.DeleteCard(id, user.ProviderID, user.LocationID);

            return Ok();
        }
    }
}