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

        // GET api/values/5
        [SwaggerOperation("GetCardSurgeryItems")]
        [Route("api/card/surgeryitems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardItem>))]
        public HttpResponseMessage GetCardSurgeryItems(int cardId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetCardSurgeryItems(cardId, user.ProviderID, user.LocationID);

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
        public HttpResponseMessage GetCardList(int? userId = null, int? procedureId = null, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetCardList(userId, procedureId, user.ProviderID, user.LocationID);

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
            DataAccess.SqlHelper.UpdateCardQuantity(cardId, cardQuantity);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("AssignFlow")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/card/assignFlow", Name = "AssignFlowCard")]
        public async Task<IHttpActionResult> AssignToCard(int cardId, int flowId)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.AssignFlowToCard(flowId, cardId, user.ProviderID, user.LocationID);

            return Ok();
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

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Delete(int id)
        {
        }
    }
}