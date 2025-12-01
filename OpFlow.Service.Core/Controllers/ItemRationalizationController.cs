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
    [Route("api/itemRationalization")]
    public class ItemRationalizationController : OpFlowController
    {
        public ItemRationalizationController(
            IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
        }

        [Route("summary")]
        public async Task<ActionResult> GetSummary()
        {
            var user = await GetUserSecurity();
            

            var specialties = await _sqlHelper.GetSpecialties(user.SelectedLocation);
            var surgeons = await _sqlHelper.GetSurgeons(null, user.SelectedLocation);
            var audits = await _sqlHelper.GetDisposableAudits(user.SelectedLocation);
            var counts = await _sqlHelper.GetDisposableCounts(user.SelectedLocation);

            return Ok(new
            {
                Specialties = specialties,
                Surgeons = surgeons,
                Audits = audits,
                Counts = counts
            });
        }

        // GET api/values/5
        [Route("cardCategories")]
        public async Task<ActionResult> GetCardCategories(int? surgeonId, int? specialtyId, string cardName, string hierarchyLevel, int? cardCategoryId,
            string cardSortField, string cardSortDir)
        {
            var user = await GetUserSecurity();
            

            var cardCategories = await _sqlHelper.GetCardCategories();
            var result = await _sqlHelper.GetCardCategoryXRef(surgeonId, specialtyId, cardName, hierarchyLevel, cardCategoryId,
                user.SelectedLocation);

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
                

            return Ok(new  {
                Cards = result,
                Categories = cardCategories
            });
        }

        [Route("item")]
        public async Task<ActionResult> GetItemRationalizations(int? specialtyId, int? surgeonId, decimal? minCost, decimal? maxCost)
        {
            var user = await GetUserSecurity();
            

            var items = await _sqlHelper.GetItemRationalization(specialtyId, surgeonId, minCost, maxCost, user.SelectedLocation);

            return Ok(items);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [Route("search")]
        [HttpGet]
        public async Task<ActionResult> SearchItemRationalizations(string itemName = null, decimal? minCost = null, decimal? maxCost = null)
        {
            var user = await GetUserSecurity();
            

            var items = await _sqlHelper.SearchItems(itemName, minCost, maxCost, user.SelectedLocation);

            return Ok(items);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [Route("searchAudits")]
        [HttpGet]
        public async Task<ActionResult> SearchAuditCase(int? specialtyId, string itemName, int? surgeonId, int? cardId, DateTime beginDate, DateTime endDate)
        {
            var user = await GetUserSecurity();
            

            var items = await _sqlHelper.GetItemAuditCases(specialtyId, itemName, surgeonId, cardId, beginDate, endDate, user.SelectedLocation);

            return Ok(items);
        }

        // GET api/values/5
        [Route("cardCategories")]
        [HttpPost]
        public async Task<ActionResult> UpdateCardCategories(UpdateCardCategoryPost post)
        {
            var user = await GetUserSecurity();
            

            if (String.IsNullOrEmpty(post.CardCategory))
            {
                return StatusCode((int)HttpStatusCode.Ambiguous);
            }

            var cardCategoryId = await _sqlHelper.ParseCardCategory(post.CardCategory, user.SelectedLocation);

            if (cardCategoryId == null)
            {
                return StatusCode((int)HttpStatusCode.Ambiguous);
            }

            var result = await _sqlHelper.UpdateCardCategories(post.HierarchyLevel, cardCategoryId.Value, post.Cards, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("cardCategoryXref")]
        [HttpPost]
        public async Task<ActionResult> InsertCardCategoryXRef(int cardCategoryId, [FromBody] InsertCardCategoryCardPost request)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.InsertCardCategoryXRef(cardCategoryId, request.CardId, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("cardCategories/{cardCategoryXrefId}")]
        [HttpDelete]
        public async Task<ActionResult> DeleteCardCategoryXRef(int cardCategoryXrefId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.DeleteCardCategoryXRef(cardCategoryXrefId, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [Route("auditComplete")]
        [HttpPut]
        public async Task<ActionResult> FinishDisposableAudit(int surgeryId, string target)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.UpdateDisposableAuditComplete(surgeryId, target, user.UserID, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [Route("audits")]
        [HttpPost]
        public async Task<ActionResult> PostItemAudits([FromBody] ItemAuditPost post)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.InsertDisposableAudit(post.Surgeries, post.Target, user.SelectedLocation);

            List<TraySurgeryAudit> result;
            if (post.Target == "A")
                result = await _sqlHelper.GetDisposableAudits(user.SelectedLocation);
            else
                result = await _sqlHelper.GetDisposableCounts(user.SelectedLocation);

            return Ok(result);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [Route("audits")]
        [HttpPut]
        public async Task<ActionResult> PutItemAudits([FromBody] ItemAuditPut post)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.UpdateDisposableAudit(post.Audits, post.Target, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [Route("audits")]
        [HttpDelete]
        public async Task<ActionResult> DeleteItemAudit(int surgeryId, string target)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.DeleteDisposableAudit(surgeryId, target, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [Route("countNeeded")]
        public async Task<ActionResult> PutCountNeeded(int itemId, bool countNeeded)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.UpdateItemCountNeeded(itemId, countNeeded, user.SelectedLocation);

            return Ok(result);
        }

        [Route("audits/csv", Name = "GetAuditCsv")]
        [HttpGet]
        public async Task<ActionResult> GetAuditCsv(string target)
        {
            var user = await GetUserSecurity();
            

            List<TraySurgeryAudit> audits;
            if (target == "A")
            {
                audits = await _sqlHelper.GetDisposableAudits(user.SelectedLocation);
            }
            else
            {
                audits = await _sqlHelper.GetDisposableCounts(user.SelectedLocation);
            }

            var extract = "Surgeon, Card, Specialty, Old Tray, New Tray, Intrument, Proposed\r\n";
            foreach (var audit in audits)
            {
                extract +=
                    $"\"{audit.SurgeonName?.Trim().Replace("\"", "\"\"")}\",\"{audit.RoomDescription?.Trim().Replace("\"", "\"\"")}\",\"{audit.RoomDescription.Replace("\"", "\"\"")}\",\"{audit.RoomDescription.Replace("\"", "\"\"")}\",\"{audit.RoomDescription.Replace("\"", "\"\"")}\",\"{audit.RoomDescription.Replace("\"", "\"\"")}\",\"{audit.RoomDescription}\"\r\n";
            }

            var extractBytes = System.Text.Encoding.Unicode.GetBytes(extract);
            
            return File(extractBytes, "application/octet-stream", "DisposableExport.csv");
        }
    }
}