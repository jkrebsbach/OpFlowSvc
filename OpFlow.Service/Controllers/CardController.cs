using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using OpFlow.Data;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    public class CardController : ApiController
    {

        // GET api/values/5
        [SwaggerOperation("GetById")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage Get(int cardId, int providerId, int locationId)
        {
            var cards = DataAccess.SqlHelper.GetCardData(cardId, providerId, locationId);

            return Request.CreateResponse(HttpStatusCode.OK, cards);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardSurgerys")]
        [Route("api/card/surgery")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardItem>))]
        public HttpResponseMessage GetCardSurgerys(int cardId, int providerId, int locationId)
        {
            var result = DataAccess.SqlHelper.GetCardSurgeryItems(cardId, providerId, locationId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardSurgeryDelays")]
        [Route("api/card/delays")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        public HttpResponseMessage GetCardSurgeryDelays(int surgeryId, int providerId, int locationId)
        {
            var result = DataAccess.SqlHelper.GetCardSurgeryDelays(surgeryId, providerId, locationId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardList")]
        [Route("api/card/list")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        public HttpResponseMessage GetCardList(int userId, int providerId, int locationId)
        {
            var result = DataAccess.SqlHelper.GetCardList(userId, providerId, locationId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardCountAvgClose")]
        [Route("api/card/avgClose")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        public HttpResponseMessage GetCardCountAvgClose(int cardId, int providerId, int locationId)
        {
            var result = DataAccess.SqlHelper.GetCardCountAvgClose(cardId, providerId, locationId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardSurgeryOpens")]
        [Route("api/card/opens")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        public HttpResponseMessage GetCardSurgeryOpens(int surgeryId, int providerId, int locationId)
        {
            var result = DataAccess.SqlHelper.GetCardSurgeryOpens(surgeryId, providerId, locationId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardItemPulls")]
        [Route("api/card/pulled")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardItem>))]
        public HttpResponseMessage GetCardItemPulls(int surgeryId, int providerId, int locationId)
        {
            var result = DataAccess.SqlHelper.GetCardItemPulls(surgeryId, providerId, locationId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardSurgeryCloses")]
        [Route("api/card/closes")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardItem>))]
        public HttpResponseMessage GetCardSurgeryCloses(int surgeryId, int providerId, int locationId)
        {
            var result = DataAccess.SqlHelper.GetCardSurgeryCloses(surgeryId, providerId, locationId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardChecklist")]
        [Route("api/card/checklist")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        public HttpResponseMessage GetCardChecklist(int providerId, int locationId, int cardId)
        {
            var result = DataAccess.SqlHelper.GetProviderCardChecklist(providerId, locationId, cardId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("api/card/users")]
        [SwaggerOperation("GetCardUsers")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeryUser>))]
        public HttpResponseMessage GetCardUsers(int cardId, int providerId, int locationId, int? typeId = null)
        {
            var result = DataAccess.SqlHelper.GetProviderCardUsers(cardId, providerId, locationId, typeId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
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