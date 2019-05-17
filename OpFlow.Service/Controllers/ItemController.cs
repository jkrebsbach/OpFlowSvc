using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    public class ItemController : ApiController
    {
        // GET api/values/5
        [SwaggerOperation("Get")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemMaster>))]
        public async Task<HttpResponseMessage> Get(string itemType = null, int? trayId = null, bool? countNeeded = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var items = await sqlHelper.GetItems(itemType, trayId, countNeeded, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, items);
        }

        // GET api/values/5
        [SwaggerOperation("GetTrayItems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemTray>))]
        [Route("api/item/trayItems")]
        public async Task<HttpResponseMessage> GetTrayItems(int trayId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var items = await sqlHelper.GetTrayItems(trayId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, items);
        }

        // GET api/values/5
        [SwaggerOperation("GetTrayExport")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/item/trayExport")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetTrayExport(int surgeryId, int? trayId = null, string itemType = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var counts = await sqlHelper.GetSurgeryCardItemCounts(surgeryId, user.ProviderID, user.LocationID);

            var items = counts.TrayCollectionCounts.Where(c => c.CollectionItemID == trayId && c.Usage > 0).ToList();

            var csvExport = CardItemCount.GetCsvHeader();
            csvExport = items.Aggregate(csvExport, (current, item) => current + item.GetCsvExport());

            var exportBytes = System.Text.Encoding.UTF32.GetBytes(csvExport);
            var memStream = new MemoryStream(exportBytes);

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(memStream)
            };

            result.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                    { FileName = "TrayItems.DAT", };

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-steam");
            result.Content.Headers.ContentLength = memStream.Length;

            return result;
        }

        // GET api/values/5
        [SwaggerOperation("GetCollectionTrays")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemMaster>))]
        [Route("api/item/collectionTrays")]
        public async Task<HttpResponseMessage> GetCollectionTrays(int itemId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var result = await sqlHelper.GetCollectionTrays(itemId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetTrayQuestions")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<TrayQuestionSummary>))]
        [Route("api/item/trayQuestions")]
        public async Task<HttpResponseMessage> GetTrayQuestions(int itemId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var questions = await sqlHelper.GetTrayQuestions(itemId, user.ProviderID, user.LocationID);


            return Request.CreateResponse(HttpStatusCode.OK, questions);
        }

        // GET api/values/5
        [SwaggerOperation("PutTrayInstrument")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("api/item/trayInstrument")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutTrayInstrument(int trayId, [FromBody] TrayInstrumentPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var result = 0;
            foreach (var instrument in post.Instruments)
            {
                result = await sqlHelper.InsertTrayInstrument(instrument.InstrumentName, instrument.InstrumentNbr, trayId, instrument.Quantity, user.ProviderID, user.LocationID);
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}