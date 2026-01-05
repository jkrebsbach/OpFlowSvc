using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OpFlow.Data;
using OpFlow.Service.DataAccess;

namespace OpFlow.Service.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/card")]
    public class CardController : OpFlowController
    {
        public CardController(IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
        }

        // GET api/values/5
        public async Task<ActionResult> Get(int cardId)
        {
            var user = await GetUserSecurity();
            

            var cards = await _sqlHelper.GetCardData(cardId, user.SelectedLocation);

            return Ok(cards);
        }

        [Route("details")]
        public async Task<ActionResult> GetDetails(int? cardId = null)
        {
            var user = await GetUserSecurity();
            

            CardDetail card = null;
            if (cardId.HasValue)
            {
                var cards = await _sqlHelper.GetCardData(cardId.Value, user.SelectedLocation);

                card = cards.FirstOrDefault();
                if (card == null)
                {
                    return NotFound();
                }

                card.CardUsers = await _sqlHelper.GetCardUsers(cardId.Value, user.SelectedLocation, 1);
                card.CardItems = await _sqlHelper.GetCardItems(cardId.Value, user.SelectedLocation);
                card.SurgeryAdditionalItems = await _sqlHelper.GetCardAdditionalItems(cardId.Value, user.SelectedLocation);
                card.CardProcedures = await _sqlHelper.GetCardProcedures(cardId.Value, user.SelectedLocation);
                card.CardCategories = await _sqlHelper.GetCardCategoryXRef(cardId.Value, user.SelectedLocation);
            }

            var cardCategories = await _sqlHelper.GetCardCategories();
            var surgeons = await _sqlHelper.SearchUsers(null, 1, null, user.SelectedLocation);

            return Ok(new
            {
                CardCategories = cardCategories,
                Card = card,
                Surgeons = surgeons
            });
        }

        // GET api/values/5
        [Route("usageHistory")]
        public async Task<ActionResult> GetUsageHistory(int surgeryId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetCardUsageHistory(surgeryId, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("carditems")]
        public async Task<ActionResult> GetCardItems(int cardId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetCardItems(cardId, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("delays")]
        public async Task<ActionResult> GetCardSurgeryDelays(int surgeryId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetCardSurgeryDelays(surgeryId, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("list")]
        public async Task<ActionResult> GetCardList(int? userId = null, int? specialtyId = null, int? procedureId = null, 
            string cardName = null, bool? defaultFilter = null, int? trayItemId = null, string additionalData = null)
        {
            var user = await GetUserSecurity();
            

            var defaultCardOnly = defaultFilter ?? false;
            var cardList = await _sqlHelper.GetCardList(userId, specialtyId, procedureId, null, cardName, trayItemId, 
                defaultCardOnly, additionalData, user.SelectedLocation);
            var result = cardList.OrderBy(c => c.CardDescription).ToList();

            return Ok(result);
        }

        // GET api/values/5
        [Route("cardsPaged")]
        public async Task<ActionResult> GetCardsPaged(string additionalData = null, string term = null, int page = 1)
        {
            var user = await GetUserSecurity();
            

            var instruments = await _sqlHelper.GetCardsPaged(additionalData, term, page, user.SelectedLocation);

            return Ok(instruments);
        }

        // GET api/values/5
        [Route("cardsPagedInternal")]
        public async Task<ActionResult> GetCardsPagedInternal(string additionalData = null, string term = null, int page = 1)
        {
            var user = await GetUserSecurity();
            if (user.RoleType != "Internal")
                return NotFound();

            

            var instruments = await _sqlHelper.GetCardsPaged(additionalData, term, page, null);

            return Ok(instruments);
        }

        // GET api/values/5
        [Route("listUsed")]
        [HttpPost]
        public async Task<ActionResult> UsedCardList([FromBody] UsedCardSearchPost post)
        {
            var user = await GetUserSecurity();
            

            var cardList = await _sqlHelper.GetUsedCardList(post.UserIDs, post.TrayIDs, post.CategoryIDs, user.SelectedLocation);
            var result = cardList.OrderBy(c => c.CardDescription).ToList();

            return Ok(result);
        }

        // GET api/values/5
        [Route("feedback")]
        [HttpGet]
        public async Task<ActionResult> GetEditFeedback(int? specialtyId = null, int? userId = null, int? cardId = null, DateTime? beginDate = null, DateTime? endDate = null)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetCardFeedback(specialtyId, userId, cardId, beginDate, endDate,
                user.SelectedLocation);
            
            return Ok(result);
        }

        // GET api/values/5
        [Route("cardCategories")]
        public async Task<ActionResult> GetCardCategories()
        {
            

            var result = await _sqlHelper.GetCardCategories();

            return Ok(result);
        }

        // GET api/values/5
        [Route("cardCategory")]
        [HttpPost]
        public async Task<ActionResult> PostCardCategory(string cardCategoryName)
        {
            var user = await GetUserSecurity();
            

            if (user.RoleType != "Internal")
                return NotFound();

            var result = await _sqlHelper.ParseCardCategory(cardCategoryName, 1);

            return Ok(result);
        }

        // GET api/values/5
        [Route("cardCategory")]
        [HttpDelete]
        public async Task<ActionResult> DeleteCardCategory(int cardCategoryId)
        {
            var user = await GetUserSecurity();
            

            if (user.RoleType != "Internal")
                return NotFound();

            var result = await _sqlHelper.DeleteCardCategory(cardCategoryId);

            return Ok(result);
        }

        // GET api/values/5
        [Route("cardCategoryDetails")]
        public async Task<ActionResult> GetCardCategoryDetails()
        {
            var user = await GetUserSecurity();
            

            var cardCategories = await _sqlHelper.GetCardCategoryDetails(
                null, null, null, null, null,
                user.SelectedLocation);
            var specialties = await _sqlHelper.GetSpecialties(user.SelectedLocation);
            var surgeons = await _sqlHelper.GetSurgeons(null, user.SelectedLocation);
            var procedureProfiles = await _sqlHelper.GetProcedureProfiles();

            return Ok(new {
                Specialties = specialties,
                Surgeons = surgeons,
                CardCategories = cardCategories,
                ProcedureProfiles = procedureProfiles
            });
        }

        // GET api/values/5
        [Route("cardCategoryDetails")]
        [HttpPost]
        public async Task<ActionResult> PostCardCategoryDetails([FromBody] CardCategoryDetailPost request)
        {
            var user = await GetUserSecurity();
            

            var cardCategories = await _sqlHelper.GetCardCategoryDetails(
                request.CardCategoryID, request.SpecialtyID, request.CardID, request.ProcedureProfileID, request.SurgeonID,
                user.SelectedLocation);
            
            return Ok(cardCategories);
        }

        // GET api/values/5
        [Route("feedback")]
        [HttpPost]
        public async Task<ActionResult> PostEditFeedback(int feedbackId, [FromBody]FeedbackRequest post)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.UpdateCardFeedback(feedbackId, post.Response, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("users")]
        public async Task<ActionResult> GetCardUsers(int cardId, int? typeId = null)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetCardUsers(cardId, user.SelectedLocation, typeId);

            return Ok(result);
        }

        // GET api/values/5
        [Route("bundledefault")]
        public async Task<ActionResult> GetBundleDefaultCardFlowRoom(int bundleId, int? userId = null)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetBundleDefaultCardFlowRoom(bundleId, userId ?? user.UserID, user.SelectedLocation) ??
                new CardFlowRoom()
                {
                    CardDescription = "None",
                    FlowDescription = "None",
                    RoomDescription = "None"
                };

            result.Cards = await _sqlHelper.GetCardList(userId, null, null, bundleId, null, null, false, null, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("importSurgeons")]
        public async Task<ActionResult> GetImportSurgeons()
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetImportSurgeons(user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("importProcedures")]
        public async Task<ActionResult> GetImportProcedures(string importSurgeon)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetImportProcedures(importSurgeon, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("proceduredefault")]
        public async Task<ActionResult> GetProcedureDefaultCardFlowRoom(string cptCode)
        {
            var user = await GetUserSecurity();
            

            var cards = await _sqlHelper.GetProcedureDefaultCardFlowRoom(cptCode, user.SelectedLocation);
            var result = cards.FirstOrDefault();

            return result == null ?
                NotFound() :
                Ok(result);
        }

        // GET api/values/5
        [Route("specialtyproceduredefault")]
        public async Task<ActionResult> GetSpecialtyProcedureDefaultCardFlowRoom(string cptCode)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetSpecialtyProcedureDefaultCardFlowRoom(user.SelectedLocation, cptCode);

            return Ok(result.FirstOrDefault());
        }

        // GET api/values/5
        [Route("multipleproceduresdefault")]
        public async Task<ActionResult> GetMultipleProceduresDefaultCardFlowRoom(List<string> cptCodes)
        {
            var user = await GetUserSecurity();
            

            var result = new List<CardFlowRoom>();

            foreach (var cptCode in cptCodes)
            {
                var procedures = await _sqlHelper.GetProcedureDefaultCardFlowRoom(cptCode, user.SelectedLocation);

                result.AddRange(procedures);
            }

            return Ok(result);
        }

        // GET api/values/5
        [Route("specialtymultipleproceduresdefault")]
        public async Task<ActionResult> GetSpecialtyMultipleProceduresDefaultCardFlowRoom(int specialtyId, string cptCodes)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetSpecialtyMultipleProceduresDefaultCardFlowRoom(user.SelectedLocation, specialtyId, cptCodes);

            return Ok(result);
        }

        // PUT api/values
        [Route("updateQuantity", Name = "UpdateQuantity")]
        [HttpPut]
        public async Task<ActionResult> UpdateItemQty(int cardId, [FromBody]CardQuantityEdit cardQuantity)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.UpdateCardQuantity(cardId, cardQuantity, user.SelectedLocation);

            return Ok();
        }

        // PUT api/values
        [Route("updateQuantityRequest", Name = "UpdateQuantityRequest")]
        [HttpPut]
        public async Task<ActionResult> UpdateItemQtyRequest(int cardId, int surgeryId, [FromBody]CardQuantityEditRequest cardQuantity)
        {
            var user = await GetUserSecurity();
            

            foreach (var editRequest in cardQuantity.EditData)
            {
                if (cardQuantity.Target == "C")
                    await _sqlHelper.UpdateCardQuantityRequest(cardId, editRequest, user.SelectedLocation, user.UserID);
                else if (editRequest.TrayID.HasValue)
                    await _sqlHelper.AddCustomSurgeryTrayItem(surgeryId, editRequest.TrayID.Value, editRequest.ItemID, editRequest.OpenQty ?? 0, user.SelectedLocation, user.UserID);
                else
                    await _sqlHelper.UpdateSurgeryItemQuantity(surgeryId, editRequest, user.SelectedLocation, user.UserID);
            }

            return Ok();
        }

        // POST api/values
        [Route("assignFlow", Name = "AssignFlowCard")]
        public async Task<ActionResult> AssignToCard(int cardId, int flowId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.AssignFlowToCard(flowId, cardId, user.SelectedLocation);

            return Ok(418);
        }

        // POST api/values
        [Route("assignRoomSetup", Name = "AssignRoomSetupCard")]
        public async Task<ActionResult> AssignRoomSetupToCard(int cardId, int roomSetupId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.AssignRoomSetupToCard(roomSetupId, cardId, user.SelectedLocation);

            return Ok(418);
        }

        [Route("cardUser", Name = "AssignCardUser")]
        [HttpPost]
        public async Task<ActionResult> AssignCardUser(int cardId, int userId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.AssignUserToCard(cardId, userId, user.SelectedLocation);

            return Ok();
        }

        [Route("cardUser", Name = "DeleteCardUser")]
        [HttpDelete]
        public async Task<ActionResult> DeleteCardUser(int cardId, int userId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.RemoveUserFromCard(cardId, userId, user.SelectedLocation);

            return Ok();
        }

        // PUT api/values
        [Route("cardItem", Name = "DeleteCardItem")]
        [HttpDelete]
        public async Task<ActionResult> DeleteCardItem(int cardId, int itemId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.DeleteCardItem(cardId, itemId, user.SelectedLocation);

            return Ok(result);
        }

        // PUT api/values
        [Route("cardItem", Name = "AssignCardItem")]
        [HttpPut]
        public async Task<ActionResult> PutCardItem(int cardId, [FromBody]CardItemPost post)
        {
            var user = await GetUserSecurity();
            

            var result = -1;

            if (post.ItemType == "tray-group")
            {
                var trayGroups = await _sqlHelper.GetTrayGroups(user.SelectedLocation);
                var trayGroup = trayGroups.First(t => t.TrayGroupID == post.ItemID);

                foreach (var tray in trayGroup.Trays)
                {
                    result = await _sqlHelper.UpdateCardItem(cardId, tray.TrayItemID, 0, 1, user.SelectedLocation);
                }
            }
            else
            {

                if (post.ItemID == null)
                    post.ItemID = await _sqlHelper.InsertItemMaster("SUPPLY", post.ItemName, user.SelectedLocation);

                result = await _sqlHelper.UpdateCardItem(cardId, post.ItemID.Value, post.OpenQty, post.HoldQty, user.SelectedLocation);
            }

            return Ok(result);
        }

        // PUT api/values
        [Route("cardProcedure", Name = "AssignCardProcedure")]
        [HttpPut]
        public async Task<ActionResult> PutCardProcedure(int cardId, int procedureId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.UpdateCardProcedure(cardId, procedureId, user.SelectedLocation);

            return Ok(result);
        }

        // PUT api/values
        [Route("cardProcedure", Name = "DeleteCardProcedure")]
        [HttpDelete]
        public async Task<ActionResult> DeleteCardProcedure(int cardId, int procedureId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.DeleteCardProcedure(cardId, procedureId, user.SelectedLocation);

            return Ok(result);
        }

        // POST api/values
        [HttpPost]
        public async Task<ActionResult> Post([FromBody]CardPost value)
        {
            try
            {
                var user = await GetUserSecurity();
                

                var cardCategoryId = await _sqlHelper.ParseCardCategory(value.CardCategory, user.SelectedLocation);

                var cardId = await _sqlHelper.InsertCard(value.Description, value.OwnerUserID,
                    value.SpecialtyID, value.ProcedureID, value.TemplateFlowID, value.TemplateRoomSetupID,
                    value.BundleID, value.BundleFlag, cardCategoryId,
                    value.DefaultFlag == "1", value.SpecialtyDefaultFlag == "1",
                    user.SelectedLocation);

                await InitializeCardProcedures(_sqlHelper, cardId, user, value.Procedures);

                await _sqlHelper.UpdateCardCategoryXRef(cardId, value.CardCategories, user.SelectedLocation);

                return CreatedAtAction("Post", cardId);
            }
            catch (Exception ex)
            {
                Models.LogHelper.LogException(ex);
                throw;
            }
        }

        // PUT api/values/5
        [HttpPut]
        public async Task<ActionResult> Put(int id, [FromBody]CardPost value)
        {
            try
            {
                var user = await GetUserSecurity();
                

                var cardCategoryId = await _sqlHelper.ParseCardCategory(value.CardCategory, user.SelectedLocation);

                if (value.Procedures != null)
                {
                    await InitializeCardProcedures(_sqlHelper, id, user, value.Procedures);
                }
                else
                {
                    await _sqlHelper.UpdateCard(id, value.Description, value.SpecialtyID, value.ProcedureID, value.TemplateFlowID, value.TemplateRoomSetupID,
                        value.BundleID, value.BundleFlag, cardCategoryId,
                        value.DefaultFlag == "1", value.SpecialtyDefaultFlag == "1",
                        user.SelectedLocation);
                }

                await _sqlHelper.UpdateCardCategoryXRef(id, value.CardCategories, user.SelectedLocation);

                return Ok();
            }
            catch (Exception ex)
            {
                Models.LogHelper.LogException(ex);
                throw;
            }
        }

        private async Task InitializeCardProcedures(SqlHelper sqlHelper, int cardId, UserSecurity user, List<CardPostImportProcedure> procedures)
        {
            if (procedures != null)
            {
                await _sqlHelper.InitializeCard(cardId, user.SelectedLocation);

                foreach (var procedure in procedures)
                {
                    if (!string.IsNullOrEmpty(procedure.ImportSurgeon) && !string.IsNullOrEmpty(procedure.ImportProcedure))
                    {
                        await _sqlHelper.InsertCardItemFromStage(cardId, user.SelectedLocation,
                            procedure.ImportProcedure, procedure.ImportSurgeon);
                    }

                }
            }
        }

        // DELETE api/values/5
        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
            var user = await GetUserSecurity();
                

                await _sqlHelper.DeleteCard(id, user.SelectedLocation);

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