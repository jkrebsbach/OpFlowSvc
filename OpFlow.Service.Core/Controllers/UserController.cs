using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using OpFlow.Service.Models;

namespace OpFlow.Service.Controllers
{
    /// <summary>
    /// Interact with Users entities
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/user")]
    public class UserController : OpFlowController
    {
        private UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
        }

        /// <summary>
        /// Return user entity for currently authenticated user
        /// </summary>
        /// <returns></returns>
        [Route("AuthorizedUser", Name = "AuthorizedUser")]
        public async Task<ActionResult> Get()
        {
            var user = await GetUserSecurity();
            

            // This is the actual location the authenticated user is assigned to
            var result = await _sqlHelper.GetUser(user.LocationID, user.UserAuthID);
            if (result != null)
                result.PHILocation = (user.SecureDatabaseName != "InvalidConnection");

            return result == null ? NotFound("User not found") : Ok(result);
        }

        /// <summary>
        /// Vendor Portal Home screen for vendors
        /// </summary>
        /// <returns></returns>
        [Route("VendorPortal", Name = "VendorPortal")]
        public async Task<ActionResult> GetVendorPortal()
        {
            var user = await GetUserSecurity();
            

            var vendorLocations = await _sqlHelper.GetVendorLocations(user.ProviderID);
            var userLocations = await _sqlHelper.GetUserLocations(user.UserID, user.LocationID);
            var categories = await _sqlHelper.GetProcedureProfileCategories();

            // If this user has limited locations, limit portal result here
            if (userLocations.Any(l => l.SelectedLocation))
            {
                vendorLocations = vendorLocations.Where(vl => userLocations.Any(ul => ul.SelectedLocation && ul.LocationID == vl.LocationID)).ToList();
            }

            var procedureProfiles = await _sqlHelper.GetProcedureProfilesVendor(user.SelectedLocation);
            var surgeons = await _sqlHelper.GetSurgeons(null, user.SelectedLocation);

            var owners = procedureProfiles.GroupBy(pp => new { pp.OwnerID, pp.OwnerName }).Select(pp => new OpFlowProvider()
            { 
                ProviderID = pp.Key.OwnerID ?? -1,
                ProviderName = pp.Key.OwnerName
            });


            var selectedLocation = vendorLocations.FirstOrDefault(l => l.LocationID == user.SelectedLocation);
            if (selectedLocation != null) selectedLocation.ActiveLocation = true;

            
            var providers = vendorLocations.GroupBy(v => new { v.ProviderID, v.ProviderName }).Select(v => new OpFlowProvider()
            {
                ProviderID = v.Key.ProviderID,
                ProviderName = v.Key.ProviderName,
                Locations = v.ToList()
            });

            var result = new
            {
                Providers = providers,
                Locations = vendorLocations,
                AllProcedureProfiles = procedureProfiles,
                ProcedureProfiles = procedureProfiles.Where(p => p.Cards.Any()),
                Surgeons = surgeons.OrderBy(s => s.LastName).ThenBy(s => s.FirstName),
                Owners = owners,
                ProcedureProfileCategories = categories
            };

            return Ok(result);
        }

        /// <summary>
        /// User location assignment
        /// </summary>
        /// <returns></returns>
        [Route("locationAssignment", Name = "GetLocationAssignment")]
        [HttpGet]
        public async Task<ActionResult> GetLocationAssignment(int userId)
        {
            var user = await GetUserSecurity();
            

            var locations = await _sqlHelper.GetUserLocations(userId, user.SelectedLocation);
            var providers = locations.GroupBy(p => new { p.ProviderID, p.ProviderName })
                .Select(p => new OpFlowProvider()
                {
                    ProviderID = p.Key.ProviderID,
                    ProviderName = p.Key.ProviderName
                });

            // If no locations selected, then all locations selected
            if (!locations.Any(l => l.SelectedLocation))
                locations.ForEach(l => l.SelectedLocation = true);

            return Ok(new
            {
                Providers = providers,
                Locations = locations
            });
        }

        /// <summary>
        /// User location assignment
        /// </summary>
        /// <returns></returns>
        [Route("cardVariance", Name = "PostCardVariance")]
        [HttpPost]
        public async Task<ActionResult> PostCardVariance([FromBody] TrayRationalizationReportPost request)
        {
            var user = await GetUserSecurity();
            

            var summary = await _sqlHelper.GetProcedureProfileCardVariance(request.CardId, user.SelectedLocation);

            var items = summary.GroupBy(s => new { s.ItemID, s.ItemName, s.TrayName, s.SourceType });
            var cardCount = request?.CardId?.Count ?? 0;


            var shared = items.Where(i => i.Count() == cardCount);
            var variance = items.Where(i => i.Count() != cardCount);

            return Ok(new
            {
                InternalTrays = summary.Where(s => s.SourceType == "T").Select(i => i.TrayName).Distinct(),
                ExternalTrays = summary.Where(s => s.SourceType == "V").Select(i => i.TrayName).Distinct(),
                Trays = new
                {
                    Shared = shared.Where(i => i.Key.TrayName != null).SelectMany(s => s.ToList()).GroupBy(s => new { s.TrayName, s.ItemName })
                        .Select(s => new { s.Key.TrayName, s.Key.ItemName, CardQty = s.Max(i => i.CardQty) }),
                    Variance = variance.Where(i => i.Key.TrayName != null).SelectMany(s => s.ToList())
                },
                Supplies = new
                {
                    Shared = shared.Where(i => i.Key.TrayName == null).SelectMany(s => s.ToList()).GroupBy(s => new { s.TrayName, s.ItemName })
                        .Select(s => new { s.Key.TrayName, s.Key.ItemName, CardQty = s.Max(i => i.CardQty) }),
                    Variance = variance.Where(i => i.Key.TrayName == null).SelectMany(s => s.ToList())
                }
            });
        }

        /// <summary>
        /// Retrieve API key for user
        /// </summary>
        /// <returns></returns>
        [Route("apiKey", Name = "ApiKey")]
        [HttpGet]
        public async Task<ActionResult> GetApiKey()
        {
            var userAuthId = await GetUserAuthID();
            

            var result = await _sqlHelper.GetApiKey(userAuthId);
            if (result == null || result?.ExpirationDate > DateTime.Now)
            {
                result = ApiKey.Generate(userAuthId);
                await _sqlHelper.InsertApiKey(result);
            }

            return Ok(new
            {
                ApiKey = result.ProviderKey
            });
        }

        /// <summary>
        /// User location assignment
        /// </summary>
        /// <returns></returns>
        [Route("locationAssignment", Name = "PostLocationAssignment")]
        [HttpPost]
        public async Task<ActionResult> PostLocationAssignment([FromBody] UserLocationPost request)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.UpdateUserLocations(user.UserID, request.LocationID);
            
            return Ok(result);
        }

        /// <summary>
        /// Vendor Portal Home screen for vendors
        /// </summary>
        /// <returns></returns>
        [Route("vendorLocation/{locationId}", Name = "PostVendorLocation")]
        [HttpPost]
        public async Task<ActionResult> PostVendorLocation(int locationId)
        {
            var user = await GetUserSecurity();
            

            if (!user.Vendor)
                return NotFound("Post not found");

            var result = await _sqlHelper.UpdateUserVendorLocation(user.UserID, locationId, user.ProviderID, user.LocationID);

            RefreshUserCache();

            return Ok(result);
        }

        /// <summary>
        /// Vendor Portal Home screen for vendors
        /// </summary>
        /// <returns></returns>
        [Route("userSettings", Name = "PostUserSettings")]
        [HttpPost]
        public async Task<ActionResult> PostUserSettings([FromBody] UserSettingsPost request)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.UpdateUserSettings(user.UserID, request.UserSettings, user.LocationID);

            RefreshUserCache();

            return Ok(result);
        }

        /// <summary>
        /// Vendor Portal Home screen for vendors
        /// </summary>
        /// <returns></returns>
        [Route("vendorSurgeries/{cardId}", Name = "VendorSurgeries")]
        public async Task<ActionResult> GetVendorSurgeries(int cardId, int locationId)
        {
            var user = await GetUserSecurity();
            

            if (!user.Vendor)
                return NotFound("Surgeries not found");

            var vendorLocations = await _sqlHelper.GetVendorLocations(user.ProviderID);
            if (!vendorLocations.Any(l => l.LocationID == locationId))
                return NotFound("Surgeries not found");

            var beginDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(14);
            var surgeries = await _sqlHelper.SearchCases(null, null, null, null, cardId, null, null, false, beginDate, endDate, locationId);

            return Ok(surgeries);
        }

        /// <summary>
        /// Vendor Portal Home screen for vendors
        /// </summary>
        /// <returns></returns>
        [Route("vendorSurgeons/{locationId}", Name = "VendorSurgeons")]
        public async Task<ActionResult> GetVendorSurgeons(int locationId)
        {
            var user = await GetUserSecurity();
            

            if (!user.Vendor)
                return NotFound("Surgeons not found");

            var vendorLocations = await _sqlHelper.GetVendorLocations(user.ProviderID);
            if (!vendorLocations.Any(l => l.LocationID == locationId))
                return NotFound("Surgeons not found");

            var surgeons = await _sqlHelper.GetSurgeons(null, locationId);

            return Ok(surgeons);
        }

        /// <summary>
        /// Vendor Portal Home screen for vendors
        /// </summary>
        /// <returns></returns>
        [Route("vendorCard/{cardId}", Name = "VendorCard")]
        public async Task<ActionResult> GetVendorCard(int cardId, int locationId)
        {
            var user = await GetUserSecurity();
            

            if (!user.Vendor)
                return NotFound("Card not found");

            var vendorLocations = await _sqlHelper.GetVendorLocations(user.ProviderID);
            if (!vendorLocations.Any(l => l.LocationID == locationId))
                return NotFound("Card not found");

            var cardItems = await _sqlHelper.GetCardItems(cardId, locationId);

            var result = "Tray, Item, Quantity\r\n";
            
            foreach (var item in cardItems)
            {
                if (item.ItemType == "TRAY")
                {
                    var trayItems = await _sqlHelper.GetTrayItems(item.ItemID, locationId);
                    foreach (var trayItem in trayItems)
                    {
                        result += $"{item.ItemDescription},\"{trayItem.InstrumentName}\", {trayItem.Quantity}\r\n";
                    }
                }
                else
                {
                    result += $"No Tray,\"{item.ItemDescription}\", {item.Quantity}\r\n";
                }
            }

            return CsvResponse(result);
        }

        /// <summary>
        /// Vendor Portal Home screen for vendors
        /// </summary>
        /// <returns></returns>
        [Route("vendorCard", Name = "PostVendorCard")]
        [HttpPost]
        public async Task<ActionResult> PostVendorCard([FromBody] VendorCardNamePost request)
        {
            var user = await GetUserSecurity();
            

            if (!user.Vendor)
                return NotFound("Put not found");

            var result = await _sqlHelper.UpdateVendorCardName(request.CardID, request.CardName, user.ProviderID);

            return Ok(result);
        }

        /// <summary>
        /// Vendor Portal Home screen for vendors
        /// </summary>
        /// <returns></returns>
        [Route("replicateCard", Name = "ReplicateVendorCard")]
        [HttpPost]
        public async Task<ActionResult> ReplicateVendorCard([FromBody] VendorCardNamePost request)
        {
            var user = await GetUserSecurity();
            

            if (!user.Vendor)
                return NotFound("Put not found");

            var vendorLocations = await _sqlHelper.GetVendorLocations(user.ProviderID);
            if (!vendorLocations.Any(l => l.LocationID == request.LocationID))
                return NotFound("Surgeries not found");

            var result = await _sqlHelper.UpdateVendorCardReplicate(request.CardID, request.CardName, request.SurgeonID, request.ProcedureProfileID, user.ProviderID);

            return Ok(result);
        }

        /// <summary>
        /// Vendor Portal Home screen for vendors
        /// </summary>
        /// <returns></returns>
        [Route("vendorProvider", Name = "PostVendorProvider")]
        [HttpPost]
        public async Task<ActionResult> PostVendorProvider([FromBody] VendorCreateProviderPost request)
        {
            var user = await GetUserSecurity();
            

            if (!user.Vendor)
                return NotFound("Put not found");

            var result = await _sqlHelper.InsertVendorProvider(request.Provider, user.ProviderID);

            return Ok(result);
        }

        /// <summary>
        /// Vendor Portal Home screen for vendors
        /// </summary>
        /// <returns></returns>
        [Route("vendorLocation", Name = "CreateVendorLocation")]
        [HttpPost]
        public async Task<ActionResult> CreateVendorLocation([FromBody] VendorCreateLocationPost request)
        {
            var user = await GetUserSecurity();
            

            if (!user.Vendor)
                return NotFound("Put not found");

            var result = await _sqlHelper.InsertVendorLocation(request.ProviderID, 
                request.Location, request.Street, request.City, request.State, request.Zip, user.ProviderID);

            RefreshUserCache();

            return Ok(result);
        }

        /// <summary>
        /// Map OPP to surgeon card
        /// </summary>
        /// <returns></returns>
        [Route("surgeonProfile/{procedureProfileId}", Name = "PutSurgeonProfile")]
        [HttpPut]
        public async Task<ActionResult> PutSurgeonProfile(int procedureProfileId, [FromBody] SurgeonProfilePost request)
        {
            var user = await GetUserSecurity();
            

            if (!user.Vendor)
                return NotFound("Put not found");

            foreach (var surgeonId in request.SurgeonID)
            {
                await _sqlHelper.UpdateSurgeonProfile(procedureProfileId, surgeonId, user.VendorLocationID.Value);
            }

            return Ok(procedureProfileId);
        }

        /// <summary>
        /// Search users
        /// </summary>
        /// <param name="nameSearchText">Will filter based on first/last name string match</param>
        /// <returns></returns>
        [Route("SearchUsers", Name = "SearchUsers")]
        public async Task<ActionResult> GetUsers(string nameSearchText = null, int? roleId = null, int? specialtyId = null)
        {
            var user = await GetUserSecurity();
            

            var users = await _sqlHelper.SearchUsers(nameSearchText, roleId, specialtyId, user.SelectedLocation);

            return Ok(users);
        }

        /// <summary>
        /// List roles
        /// </summary>
        /// <returns></returns>
        [Route("Roles", Name = "GetRoles")]
        public async Task<ActionResult> GetRoles()
        {
            var user = await GetUserSecurity();
            

            var roles = await _sqlHelper.GetRoles(user.SelectedLocation);

            return Ok(roles);
        }

        // POST api/values
        [Route("Checkin", Name = "Checkin")]
        public async Task<ActionResult> CheckinUser(int surgeryId, [FromBody]User user)
        {
            var userSecurity = await GetUserSecurity();
            

            await _sqlHelper.CheckInUser(user, surgeryId);

            return Ok();
        }

        // POST api/values
        [Route("Checkout", Name = "Checkout")]
        public async Task<ActionResult> CheckoutUser(int surgeryId, [FromBody]User user)
        {
            var userSecurity = await GetUserSecurity();
            

            await _sqlHelper.CheckOutUser(user, surgeryId);

            return Ok();
        }

        // POST api/values
        [Route("Reviewed", Name = "Reviewed")]
        public async Task<ActionResult> WorkupReviewed(int surgeryId, [FromBody]User user)
        {
            var userSecurity = await GetUserSecurity();
            

            await _sqlHelper.WorkupReviewed(user, surgeryId);

            return Ok();
        }

        // POST api/Account/ResetPassword
        [Route("ResetPassword")]
        public async Task<ActionResult> ResetPassword(int userId, [FromBody]SetPasswordBindingModel model)
        {
            var user = await GetUserSecurity();
            

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model.NewPassword != model.ConfirmPassword)
                return BadRequest("Passwords do not match");

            var secureUser = await _sqlHelper.GetSecureUser(null, userId);

            if ((user.RoleType == "Internal" && user.RoleType == "Admin") ||
                (secureUser?.SelectedLocation == user.SelectedLocation))
            {
                var appUser = await _userManager.GetUserAsync(HttpContext.User);
                var code = await _userManager.GeneratePasswordResetTokenAsync(appUser);

                var result =
                    await _userManager.ResetPasswordAsync(appUser, code, model.NewPassword);

                if (!result.Succeeded)
                {
                    return BadRequest();
                }
            }

            return Ok();
        }

        // PUT api/values/5
        public async Task<ActionResult> Post([FromBody]UserPost model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await GetUserSecurity();
            
            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
                return Conflict();

            var authenticationUser = new ApplicationUser() { UserName = model.Email, Email = model.Email };

            var result = await _userManager.CreateAsync(authenticationUser, model.Password);

            // Whatever role the current user is in - they can only add users to this role
            var appUser = await _userManager.GetUserAsync(HttpContext.User);
            var currentRoles = await _userManager.GetRolesAsync(appUser);
            var currentRole = currentRoles?.FirstOrDefault();

            if (currentRole == null)
                throw new Exception("Unable to locate authenticated user");

            _userManager.AddToRoleAsync(appUser, currentRole);

            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.FirstOrDefault().Description);
            }

            var userAuthId = new Guid(authenticationUser.Id);

            try
            {
                var applicationUserId = await _sqlHelper.CreateUser(userAuthId, model.RoleID, model.SpecialtyID,
                    model.FirstName, model.LastName,
                    model.Email, model.CellPhone, model.Initials, model.Title, user.SelectedLocation);

                return Ok(applicationUserId);
            }
            catch (Exception ex)
            {
                // roll back creation of aspnet user
                await _userManager.DeleteAsync(authenticationUser);

                throw ex;
            }
        }

        // PUT api/values/5
        public async Task<ActionResult> Put(int userId, [FromBody]UserEdit model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await GetUserSecurity();
            

            var authUserSecurity = await _sqlHelper.GetSecureUser(null, userId);

            if (authUserSecurity.SelectedLocation == user.SelectedLocation)
            {
                var applicationUser = await _sqlHelper.UpdateUser(userId, (int)model.RoleID, model.SpecialtyID, model.FirstName, model.LastName,
                    model.Email, model.CellPhone, model.Initials, model.Title, user.SelectedLocation);

                // make certain user auth matches what we sent
                var authUser = await _userManager.FindByIdAsync(authUserSecurity.UserAuthID.ToString());

                if (authUser.Email != model.Email)
                {
                    authUser.UserName = model.Email;
                    authUser.Email = model.Email;

                    await _userManager.UpdateAsync(authUser);
                }
            }

            return Ok();
        }

        // DELETE api/values/5
        [Route("{userId}")]
        public async Task<ActionResult> Delete(int userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await GetUserSecurity();
            
            var applicationUser = await _sqlHelper.GetUser(user.SelectedLocation,  userId);

            ApplicationUser authenticationUser = null;
            
            if (!string.IsNullOrEmpty(applicationUser?.Email))
                authenticationUser = await _userManager.FindByEmailAsync(applicationUser.Email);

            if (authenticationUser != null)
            {
                var authResult = await _userManager.RemovePasswordAsync(authenticationUser);
            }

            var applicationDeletion = await _sqlHelper.DeleteUser(userId, user.SelectedLocation);

            return Ok();
        }
    }
}