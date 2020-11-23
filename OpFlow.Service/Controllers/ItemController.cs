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
    [RoutePrefix("api/item")]
    public class ItemController : ApiController
    {
        // GET api/values/5
        [SwaggerOperation("Get")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemMaster>))]
        public async Task<HttpResponseMessage> Get(string itemType = null, int? trayId = null, bool? countNeeded = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var items = await sqlHelper.GetItems(itemType, trayId, countNeeded, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, items);
        }

        // GET api/values/5
        [SwaggerOperation("GetInstruments")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemMaster>))]
        [Route("instruments")]
        public async Task<HttpResponseMessage> GetInstruments()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var instruments = await sqlHelper.GetInstruments(user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, instruments);
        }

        // GET api/values/5
        [SwaggerOperation("GetInstrumentsPaged")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemMaster>))]
        [Route("instrumentsPaged")]
        public async Task<HttpResponseMessage> GetInstrumentsPaged(string term = null, int page = 1)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var instruments = await sqlHelper.GetInstrumentsPaged(term, page, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, instruments);
        }

        // GET api/values/5
        [SwaggerOperation("GetInstrumentTrays")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemMaster>))]
        [Route("instrumentTrays")]
        public async Task<HttpResponseMessage> GetInstrumentTrays(int instrumentId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var trays = await sqlHelper.GetInstrumentTrays(instrumentId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, trays);
        }

        // GET api/values/5
        [SwaggerOperation("GetSuppliesPaged")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemMaster>))]
        [Route("suppliesPaged")]
        public async Task<HttpResponseMessage> GetSuppliesPaged(string term = null, int page = 1)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var instruments = await sqlHelper.GetSuppliesPaged(term, page, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, instruments);
        }

        // GET api/values/5
        [SwaggerOperation("GetTrayItems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemTray>))]
        [Route("trayItems")]
        public async Task<HttpResponseMessage> GetTrayItems(int trayId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var items = await sqlHelper.GetTrayItems(trayId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, items);
        }

        // GET api/values/5
        [SwaggerOperation("GetTrayAdmin")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemMaster>))]
        [Route("trayAdmin")]
        public async Task<HttpResponseMessage> GetTrayAdmin()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var trays = await sqlHelper.GetItems("TRAY", null, null, user.SelectedLocation);
            var instruments = await sqlHelper.GetInstruments(user.SelectedLocation);
            var profiles = await sqlHelper.GetProcedureProfiles();
            var trayTypes = await sqlHelper.GetTrayTypes();

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Trays = trays,
                Instruments = instruments,
                ProcedureProfiles = profiles,
                TrayTypes = trayTypes
            });
        }

        // GET api/values/5
        [SwaggerOperation("GetTrayAdminDetail")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemMaster>))]
        [Route("trayAdmin/{trayId}")]
        public async Task<HttpResponseMessage> GetTrayAdminDetail(int trayId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var items = await sqlHelper.GetTrayItems(trayId, user.SelectedLocation);
            var profiles = await sqlHelper.GetProcedureProfilesVendor(user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Instruments = items,
                ProcedureProfiles = profiles.Where(p => p.Cards.Any(c => c.Trays.Any(t => t.ItemID == trayId)))
            });
        }

        // GET api/values/5
        [SwaggerOperation("GetTrayExport")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("trayExport")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetTrayExport(int surgeryId, int? trayId = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var counts = await sqlHelper.GetSurgeryCardItemCounts(surgeryId, user.SelectedLocation);

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
        [Route("collectionTrays")]
        public async Task<HttpResponseMessage> GetCollectionTrays(int itemId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetCollectionTrays(itemId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetTrayQuestions")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<TrayQuestionSummary>))]
        [Route("trayQuestions")]
        public async Task<HttpResponseMessage> GetTrayQuestions(int itemId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var questions = await sqlHelper.GetTrayQuestions(itemId, user.SelectedLocation);


            return Request.CreateResponse(HttpStatusCode.OK, questions);
        }

        // GET api/values/5
        [SwaggerOperation("PutComparableItem")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("comparableItem")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutComparableItem(int itemId, ComparableItemPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            int comparableItemId = post.RelatedItemID ??  await sqlHelper.InsertItemMaster("SUPPLY", post.ItemName, user.SelectedLocation);
            
            var result = await sqlHelper.InsertComparableItem(itemId, comparableItemId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PutComparableInstrument")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("comparableInstrument")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutComparableInstrument(int instrumentId, int trayId, ComparableInstrumentPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.InsertComparableInstrument(instrumentId, trayId, post.RelatedInstrumentID, post.RelatedTrayID, user.SelectedLocation);
            
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("DeleteComparableItem")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("comparableItem")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteComparableItem(int comparableItemId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var result = await sqlHelper.DeleteComparableItem( comparableItemId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("DeleteComparableInstrument")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("comparableInstrument")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteComparableInstrument(int comparableInstrumentId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var result = await sqlHelper.DeleteComparableInstrument(comparableInstrumentId, user.ProviderID, user.LocationID);
            
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PutTrayInstrument")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("trayInstrument/{trayId}")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutTrayInstrument(int trayId, int instrumentId, int quantity)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.InsertTrayInstrument(trayId, instrumentId, quantity, user.SelectedLocation);
            
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PutTrayInstrument")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("trayInstrument")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutTrayInstrument(int trayId, [FromBody] TrayInstrumentPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = 0;
            foreach (var instrument in post.Instruments)
            {
                result = await sqlHelper.InsertTrayInstrumentByName(instrument.InstrumentName, instrument.InstrumentNbr, trayId, instrument.Quantity, user.SelectedLocation);
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("DeleteTrayInstrument")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("trayInstrument")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteTrayInstrument(int trayId, int instrumentId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.DeleteTrayInstrument(trayId, instrumentId, user.SelectedLocation);
            
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PutTray")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("tray")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutTray([FromBody] NewTrayPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.InsertTray(post.TrayName, post.ProductNbr, post.TrayTypeID, user.SelectedLocation);
            
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PostTray")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("tray/{itemId}")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostTray(int itemId, string vendorId, int trayInstances, string productNbr, int? trayTypeId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateTray(itemId, vendorId, trayInstances, productNbr, trayTypeId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}