using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OpFlow.Data;
using OpFlow.Service.DataAccess;

namespace OpFlow.Service.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/item")]
    public class ItemController : OpFlowController
    {
        public ItemController(
            IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
        }

        // GET api/values/5
        public async Task<ActionResult> Get(string itemType = null, int? trayId = null, bool? countNeeded = null)
        {
            var user = await GetUserSecurity();
            

            var items = await _sqlHelper.GetItems(itemType, trayId, countNeeded, user.SelectedLocation);

            return Ok(items);
        }

        // GET api/values/5
        [Route("instruments")]
        public async Task<ActionResult> GetInstruments()
        {
            var user = await GetUserSecurity();
            

            var instruments = await _sqlHelper.GetInstruments(user.SelectedLocation);

            return Ok(instruments);
        }

        // GET api/values/5
        [Route("instrumentExport")]
        public async Task<ActionResult> GetInstrumentExport(string description = null, int? typeId = null, int? categoryId = null)
        {
            var user = await GetUserSecurity();
            

            var instruments = await _sqlHelper.GetInstrumentExport(description, typeId, categoryId, user.SelectedLocation);

            var csvExport = "ID, Description, Type, Category\r\n";
            foreach (var instrument in instruments)
            {
                csvExport += $"{instrument.ItemID},\"{instrument.ItemDescription?.Replace("\"", "\"\"")}\",\"{instrument.ItemType?.Replace("\"", "\"\"")}\",\"{instrument.Category?.Replace("\"", "\"\"")}\"\r\n";
            }

            var exportBytes = System.Text.Encoding.UTF32.GetBytes(csvExport);
            
            return File(exportBytes, "application/octet-stream", "InstrumentExport.csv");
        }

        // GET api/values/5
        [Route("instrumentDetailsPaged")]
        public async Task<ActionResult> GetInstrumentDetailsPaged(string description = null, int? typeId = null, int? categoryId = null, int page = 1)
        {
            var user = await GetUserSecurity();
            

            var dsInstruments = await _sqlHelper.GetInstrumentsPaged(description, typeId, categoryId, page, user.SelectedLocation);

            var instruments = dsInstruments.Tables[0].DataTableToList<ItemMaster>();
            var totalCount = dsInstruments.Tables[1].DataTableToList<RowCountEntity>().First().TotalCount;

            var pageSize = 50;
            var skipped = (page - 1) * pageSize;

            var result = new 
            {
                pagination = new PaginationResult(instruments.Count, skipped, totalCount),
                results = instruments
            };

            return Ok(result);
        }

        // GET api/values/5
        [Route("instrumentsPaged")]
        public async Task<ActionResult> GetInstrumentsPaged(string term = null, int? typeId = null, int? categoryId = null, int page = 1)
        {
            var user = await GetUserSecurity();
            

            var dsInstruments = await _sqlHelper.GetInstrumentsPaged(term, typeId, categoryId, page, user.SelectedLocation);

            var instruments = dsInstruments.Tables[0].DataTableToList<ItemMaster>();
            var totalCount = dsInstruments.Tables[1].DataTableToList<RowCountEntity>().First().TotalCount;

            var pageSize = 50;
            var results = instruments.Select(i => new KeyPair() { id = i.ItemID, text = i.ItemDescription }).ToList();
            var skipped = (page - 1) * pageSize;

            var result = new PaginationController()
            {
                pagination = new PaginationResult(instruments.Count, skipped, totalCount),
                results = results
            };

            return Ok(result);
        }

        // GET api/values/5
        [Route("instrumentTrays")]
        public async Task<ActionResult> GetInstrumentTrays(int instrumentId)
        {
            var user = await GetUserSecurity();
            

            var trays = await _sqlHelper.GetInstrumentTrays(instrumentId, user.SelectedLocation);

            return Ok(trays);
        }

        // GET api/values/5
        [Route("suppliesPaged")]
        public async Task<ActionResult> GetSuppliesPaged(string term = null, int page = 1)
        {
            var user = await GetUserSecurity();
            

            var instruments = await _sqlHelper.GetSuppliesPaged(term, page, user.SelectedLocation);

            return Ok(instruments);
        }

        // GET api/values/5
        [Route("instrumentType")]
        public async Task<ActionResult> GetInstrumentTypes()
        {
            var user = await GetUserSecurity();
            

            var trays = await _sqlHelper.GetInstrumentTypes(user.SelectedLocation);

            return Ok(trays);
        }

        // GET api/values/5
        [Route("instrumentCategory")]
        public async Task<ActionResult> GetInstrumentCategories()
        {
            var user = await GetUserSecurity();
            

            var trays = await _sqlHelper.GetInstrumentCategories(user.SelectedLocation);

            return Ok(trays);
        }

        // GET api/values/5
        [Route("trayItems")]
        public async Task<ActionResult> GetTrayItems(int trayId)
        {
            var user = await GetUserSecurity();
            

            var items = await _sqlHelper.GetTrayItems(trayId, user.SelectedLocation);

            return Ok(items);
        }

        // GET api/values/5
        [Route("trayAdmin")]
        public async Task<ActionResult> GetTrayAdmin()
        {
            var user = await GetUserSecurity();
            

            var trays = await _sqlHelper.GetItems("TRAY", null, null, user.SelectedLocation);
            var instruments = await _sqlHelper.GetInstruments(user.SelectedLocation);
            var profiles = await _sqlHelper.GetProcedureProfiles();
            var trayTypes = await _sqlHelper.GetTrayTypes();

            return Ok(new
            {
                Trays = trays,
                Instruments = instruments,
                ProcedureProfiles = profiles,
                TrayTypes = trayTypes
            });
        }

        // GET api/values/5
        [Route("trayAdmin/{trayId}")]
        public async Task<ActionResult> GetTrayAdminDetail(int trayId)
        {
            var user = await GetUserSecurity();
            

            var items = await _sqlHelper.GetTrayItems(trayId, user.SelectedLocation);
            var profiles = await _sqlHelper.GetProcedureProfilesVendor(user.SelectedLocation);

            return Ok(new
            {
                Instruments = items,
                ProcedureProfiles = profiles.Where(p => p.Cards.Any(c => c.Trays.Any(t => t.ItemID == trayId)))
            });
        }

        // GET api/values/5
        [Route("trayExport")]
        [HttpGet]
        public async Task<ActionResult> GetTrayExport(int surgeryId, int? trayId = null)
        {
            var user = await GetUserSecurity();
            

            var counts = await _sqlHelper.GetSurgeryCardItemCounts(surgeryId, user.SelectedLocation);

            var items = counts.TrayCollectionCounts.Where(c => c.CollectionItemID == trayId && c.Usage > 0).ToList();

            var csvExport = CardItemCount.GetCsvHeader();
            csvExport = items.Aggregate(csvExport, (current, item) => current + item.GetCsvExport());

            var exportBytes = System.Text.Encoding.UTF32.GetBytes(csvExport);
            
            return File(exportBytes, "application/octet-stream", "TrayItems.DAT");
        }

        // GET api/values/5
        [Route("collectionTrays")]
        public async Task<ActionResult> GetCollectionTrays(int itemId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetCollectionTrays(itemId, user.ProviderID, user.LocationID);

            return Ok(result);
        }

        // GET api/values/5
        [Route("trayQuestions")]
        public async Task<ActionResult> GetTrayQuestions(int itemId)
        {
            var user = await GetUserSecurity();
            

            var questions = await _sqlHelper.GetTrayQuestions(itemId, user.SelectedLocation);


            return Ok(questions);
        }

        // GET api/values/5
        [Route("comparableItem")]
        [HttpPut]
        public async Task<ActionResult> PutComparableItem(int itemId, ComparableItemPost post)
        {
            var user = await GetUserSecurity();
            

            int comparableItemId = post.RelatedItemID ??  await _sqlHelper.InsertItemMaster("SUPPLY", post.ItemName, user.SelectedLocation);
            
            var result = await _sqlHelper.InsertComparableItem(itemId, comparableItemId, user.ProviderID, user.LocationID);

            return Ok(result);
        }

        // GET api/values/5
        [Route("comparableInstrument")]
        [HttpPut]
        public async Task<ActionResult> PutComparableInstrument(int instrumentId, int trayId, ComparableInstrumentPost post)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.InsertComparableInstrument(instrumentId, trayId, post.RelatedInstrumentID, post.RelatedTrayID, user.SelectedLocation);
            
            return Ok(result);
        }

        // GET api/values/5
        [Route("comparableItem")]
        [HttpDelete]
        public async Task<ActionResult> DeleteComparableItem(int comparableItemId)
        {
            var user = await GetUserSecurity();
            

            if (user.RoleType != "Internal")
                return NotFound();

            var result = await _sqlHelper.DeleteComparableItem( comparableItemId, user.ProviderID, user.LocationID);

            return Ok(result);
        }

        // GET api/values/5
        [Route("comparableInstrument")]
        [HttpDelete]
        public async Task<ActionResult> DeleteComparableInstrument(int comparableInstrumentId)
        {
            var user = await GetUserSecurity();
            

            if (user.RoleType != "Internal")
                return NotFound();

            var result = await _sqlHelper.DeleteComparableInstrument(comparableInstrumentId, user.ProviderID, user.LocationID);
            
            return Ok(result);
        }

        // GET api/values/5
        [Route("instrument/{instrumentId}")]
        [HttpPut]
        public async Task<ActionResult> PutInstrument(int instrumentId, [FromBody] NewInstrumentPost post)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.UpdateInstrument(instrumentId, post.Description, post.TypeID, post.CategoryID, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("trayInstrument/{trayId}")]
        [HttpPut]
        public async Task<ActionResult> PutTrayInstrument(int trayId, int instrumentId, int quantity)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.InsertTrayInstrument(trayId, instrumentId, quantity, user.SelectedLocation);
            
            return Ok(result);
        }

        // GET api/values/5
        [Route("trayInstrument")]
        [HttpPut]
        public async Task<ActionResult> PutTrayInstrument(int trayId, [FromBody] TrayInstrumentPost post)
        {
            var user = await GetUserSecurity();
            

            var result = 0;
            foreach (var instrument in post.Instruments)
            {
                result = await _sqlHelper.InsertTrayInstrumentByName(instrument.InstrumentName, instrument.InstrumentNbr, trayId, instrument.Quantity, user.SelectedLocation);
            }

            return Ok(result);
        }

        // GET api/values/5
        [Route("trayInstrument")]
        [HttpDelete]
        public async Task<ActionResult> DeleteTrayInstrument(int trayId, int instrumentId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.DeleteTrayInstrument(trayId, instrumentId, user.SelectedLocation);
            
            return Ok(result);
        }

        // GET api/values/5
        [Route("tray")]
        [HttpPut]
        public async Task<ActionResult> PutTray([FromBody] NewTrayPost post)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.InsertTray(post.TrayName, post.ProductNbr, post.TrayTypeID, user.SelectedLocation);
            
            return Ok(result);
        }

        // GET api/values/5
        [Route("tray/{itemId}")]
        [HttpPost]
        public async Task<ActionResult> PostTray(int itemId, string vendorId, int trayInstances, string productNbr, int? trayTypeId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.UpdateTray(itemId, vendorId, trayInstances, productNbr, trayTypeId, user.SelectedLocation);

            return Ok(result);
        }
    }
}