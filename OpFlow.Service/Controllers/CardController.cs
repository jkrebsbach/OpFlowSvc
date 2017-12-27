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

        // GET api/values
        [SwaggerOperation("GetAll")]
        public IEnumerable<Card> Get()
        {
            return DataAccess.SqlHelper.GetCards();
        }

        // GET api/values/5
        [SwaggerOperation("GetById")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage Get(int id)
        {
            var cards = DataAccess.SqlHelper.GetCards();

            var result = cards.FirstOrDefault(s => s.CardID == id);

            if (result == null)
                return Request.CreateResponse(HttpStatusCode.NotFound, "Card not found");

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