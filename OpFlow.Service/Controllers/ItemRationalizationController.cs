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
    [RoutePrefix("api/itemRationalization")]
    public class ItemRationalizationController : ApiController
    {
        [SwaggerOperation("GetSummary")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemRationalization>))]
        [Route("summary")]
        public async Task<HttpResponseMessage> GetSummary()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var specialties = await sqlHelper.GetSpecialties(user.SelectedLocation);
            var surgeons = await sqlHelper.GetSurgeons(null, user.LocationID);
            var audits = await sqlHelper.GetDisposableAudits(user.ProviderID, user.LocationID);
            var counts = await sqlHelper.GetDisposableCounts(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Specialties = specialties,
                Surgeons = surgeons,
                Audits = audits,
                Counts = counts
            });
        }

        // GET api/values/5
        [SwaggerOperation("GetCardCategories")]
        [Route("cardCategories")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardWithCategory>))]
        public async Task<HttpResponseMessage> GetCardCategories(int? surgeonId, int? specialtyId, string cardName, string hierarchyLevel, int? cardCategoryId,
            string cardSortField, string cardSortDir)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var cardCategories = await sqlHelper.GetCardCategories();
            var result = await sqlHelper.GetCardCategoryXRef(surgeonId, specialtyId, cardName, hierarchyLevel, cardCategoryId,
                user.ProviderID, user.LocationID);

            if (cardSortDir == null)
                cardSortDir = "asc";

            switch (cardSortField)
            {
                case "SpecialtyDescription":
                    result = cardSortDir == "asc" ? result.OrderBy(r => r.SpecialtyDescription).ToList()
                        : result.OrderByDescending(r => r.SpecialtyDescription).ToList();
                    break;
                case "SurgeonName":
                    result = cardSortDir == "asc" ? result.OrderBy(r => r.OwnerLastName).ThenBy(r => r.OwnerFirstName).ToList()
                        : result.OrderByDescending(r => r.OwnerLastName).ThenByDescending(r => r.OwnerFirstName).ToList();
                    break;
                case "Categories":
                    result = cardSortDir == "asc" ? result.OrderBy(r => r.CardCategories?.FirstOrDefault()?.CardCategory ?? "").ToList()
                        : result.OrderByDescending(r => r.CardCategories?.FirstOrDefault()?.CardCategory ?? "").ThenByDescending(r => r.OwnerFirstName).ToList();
                    break;
                case "CardDescription":
                default:
                    result = cardSortDir == "asc" ? result.OrderBy(r => r.CardDescription).ToList()
                        : result.OrderByDescending(r => r.CardDescription).ToList();
                    break;
            }
                

            return Request.CreateResponse(HttpStatusCode.OK, new  {
                Cards = result,
                Categories = cardCategories
            });
        }

        [SwaggerOperation("GetItemRationalizations")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemRationalization>))]
        [Route("item")]
        public async Task<HttpResponseMessage> GetItemRationalizations(int? specialtyId, int? surgeonId, decimal? minCost, decimal? maxCost)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var items = await sqlHelper.GetItemRationalization(specialtyId, surgeonId, minCost, maxCost, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, items);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("SearchItemRationalizations")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemRationalization>))]
        [Route("search")]
        [HttpGet]
        public async Task<HttpResponseMessage> SearchItemRationalizations(string itemName = null, decimal? minCost = null, decimal? maxCost = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var items = await sqlHelper.SearchItems(itemName, minCost, maxCost, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, items);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("SearchAuditCase")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemRationalizationCase>))]
        [Route("searchAudits")]
        [HttpGet]
        public async Task<HttpResponseMessage> SearchAuditCase(int? specialtyId, string itemName, int? surgeonId, int? cardId, DateTime beginDate, DateTime endDate)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var items = await sqlHelper.GetItemAuditCases(specialtyId, itemName, surgeonId, cardId, beginDate, endDate, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, items);
        }

        // GET api/values/5
        [SwaggerOperation("UpdateCardCategories")]
        [Route("cardCategories")]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        public async Task<HttpResponseMessage> UpdateCardCategories(UpdateCardCategoryPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (String.IsNullOrEmpty(post.CardCategory))
            {
                return Request.CreateResponse(HttpStatusCode.Ambiguous);
            }

            var cardCategoryId = await sqlHelper.ParseCardCategory(post.CardCategory, user.ProviderID, user.LocationID);

            if (cardCategoryId == null)
            {
                return Request.CreateResponse(HttpStatusCode.Ambiguous);
            }

            var result = await sqlHelper.UpdateCardCategories(post.HierarchyLevel, cardCategoryId.Value, post.Cards, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("InsertCardCategoryXRef")]
        [Route("cardCategories")]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        public async Task<HttpResponseMessage> InsertCardCategoryXRef(int cardCategoryId, int cardId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.InsertCardCategoryXRef(cardCategoryId, cardId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("DeleteCardCategoryXRef")]
        [Route("cardCategories/{cardCategoryXrefId}")]
        [HttpDelete]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        public async Task<HttpResponseMessage> DeleteCardCategoryXRef(int cardCategoryXrefId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.DeleteCardCategoryXRef(cardCategoryXrefId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("FinishDisposableAudit")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("auditComplete")]
        [HttpPut]
        public async Task<HttpResponseMessage> FinishDisposableAudit(int surgeryId, string target)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateDisposableAuditComplete(surgeryId, target, user.UserID, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("PostItemAudits")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("audits")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostItemAudits([FromBody] ItemAuditPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.InsertDisposableAudit(post.Surgeries, post.Target, user.ProviderID, user.LocationID);

            List<TraySurgeryAudit> result;
            if (post.Target == "A")
                result = await sqlHelper.GetDisposableAudits(user.ProviderID, user.LocationID);
            else
                result = await sqlHelper.GetDisposableCounts(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("PutItemAudits")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("audits")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutItemAudits([FromBody] ItemAuditPut post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateDisposableAudit(post.Audits, post.Target, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("DeleteItemAudit")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("audits")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteItemAudit(int surgeryId, string target)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.DeleteDisposableAudit(surgeryId, target, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("PutCountNeeded")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("countNeeded")]
        public async Task<HttpResponseMessage> PutCountNeeded(int itemId, bool countNeeded)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateItemCountNeeded(itemId, countNeeded, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(string))]
        [Route("audits/csv", Name = "GetAuditCsv")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetAuditCsv(string target)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            List<TraySurgeryAudit> audits;
            if (target == "A")
            {
                audits = await sqlHelper.GetDisposableAudits(user.ProviderID, user.LocationID);
            }
            else
            {
                audits = await sqlHelper.GetDisposableCounts(user.ProviderID, user.LocationID);
            }

            var extract = "Surgeon, Card, Specialty, Old Tray, New Tray, Intrument, Proposed\r\n";
            foreach (var audit in audits)
            {
                extract +=
                    $"\"{audit.SurgeonName?.Trim().Replace("\"", "\"\"")}\",\"{audit.RoomDescription?.Trim().Replace("\"", "\"\"")}\",\"{audit.RoomDescription.Replace("\"", "\"\"")}\",\"{audit.RoomDescription.Replace("\"", "\"\"")}\",\"{audit.RoomDescription.Replace("\"", "\"\"")}\",\"{audit.RoomDescription.Replace("\"", "\"\"")}\",\"{audit.RoomDescription}\"\r\n";
            }

            var extractBytes = System.Text.Encoding.Unicode.GetBytes(extract);
            var memStream = new MemoryStream(extractBytes);
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(memStream)
            };

            result.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                    { FileName = "DisposableExport.csv", };

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-steam");
            result.Content.Headers.ContentLength = memStream.Length;

            return result;
        }
    }
}