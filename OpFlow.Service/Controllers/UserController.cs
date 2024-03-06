using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Services.Protocols;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using OpFlow.Data;
using OpFlow.Service.App_Start;
using OpFlow.Service.DataAccess;
using OpFlow.Service.Models;

namespace OpFlow.Service.Controllers
{
    /// <summary>
    /// Interact with Users entities
    /// </summary>
    [Authorize]
    [RoutePrefix("api/user")]
    public class UserController : ApiController
    {
        /// <summary>
        /// Return user entity for currently authenticated user
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type=typeof(User))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("AuthorizedUser", Name = "AuthorizedUser")]
        public async Task<HttpResponseMessage> Get()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            // This is the actual location the authenticated user is assigned to
            var result = await sqlHelper.GetUser(user.LocationID, user.UserAuthID);
            if (result != null)
                result.PHILocation = (user.SecureDatabaseName != "InvalidConnection");

            return result == null ? Request.CreateResponse(HttpStatusCode.NotFound, "User not found") : Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Vendor Portal Home screen for vendors
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("VendorPortal", Name = "VendorPortal")]
        public async Task<HttpResponseMessage> GetVendorPortal()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var vendorLocations = await sqlHelper.GetVendorLocations(user.ProviderID);
            var userLocations = await sqlHelper.GetUserLocations(user.UserID, user.LocationID);
            var categories = await sqlHelper.GetProcedureProfileCategories();

            // If this user has limited locations, limit portal result here
            if (userLocations.Any(l => l.SelectedLocation))
            {
                vendorLocations = vendorLocations.Where(vl => userLocations.Any(ul => ul.SelectedLocation && ul.LocationID == vl.LocationID)).ToList();
            }

            var procedureProfiles = await sqlHelper.GetProcedureProfilesVendor(user.SelectedLocation);
            var surgeons = await sqlHelper.GetSurgeons(null, user.SelectedLocation);

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

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// User location assignment
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("locationAssignment", Name = "GetLocationAssignment")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetLocationAssignment(int userId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var locations = await sqlHelper.GetUserLocations(userId, user.SelectedLocation);
            var providers = locations.GroupBy(p => new { p.ProviderID, p.ProviderName })
                .Select(p => new OpFlowProvider()
                {
                    ProviderID = p.Key.ProviderID,
                    ProviderName = p.Key.ProviderName
                });

            // If no locations selected, then all locations selected
            if (!locations.Any(l => l.SelectedLocation))
                locations.ForEach(l => l.SelectedLocation = true);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Providers = providers,
                Locations = locations
            });
        }

        /// <summary>
        /// User location assignment
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("cardVariance", Name = "PostCardVariance")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostCardVariance([FromBody] TrayRationalizationReportPost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var summary = await sqlHelper.GetProcedureProfileCardVariance(request.CardId, user.SelectedLocation);

            var items = summary.GroupBy(s => new { s.ItemID, s.ItemName, s.TrayName, s.SourceType });
            var cardCount = request?.CardId?.Count ?? 0;


            var shared = items.Where(i => i.Count() == cardCount);
            var variance = items.Where(i => i.Count() != cardCount);

            return Request.CreateResponse(HttpStatusCode.OK, new
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
        /// User location assignment
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("locationAssignment", Name = "PostLocationAssignment")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostLocationAssignment([FromBody] UserLocationPost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateUserLocations(user.UserID, request.LocationID);
            
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Vendor Portal Home screen for vendors
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("vendorLocation/{locationId}", Name = "PostVendorLocation")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostVendorLocation(int locationId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (!user.Vendor)
                return Request.CreateResponse(HttpStatusCode.NotFound, "Post not found");

            var result = await sqlHelper.UpdateUserVendorLocation(user.UserID, locationId, user.ProviderID, user.LocationID);

            CacheUtil.RefreshUserCache();

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Vendor Portal Home screen for vendors
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("userSettings", Name = "PostUserSettings")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostUserSettings([FromBody] UserSettingsPost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateUserSettings(user.UserID, request.UserSettings, user.LocationID);

            CacheUtil.RefreshUserCache();

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Vendor Portal Home screen for vendors
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("vendorSurgeries/{cardId}", Name = "VendorSurgeries")]
        public async Task<HttpResponseMessage> GetVendorSurgeries(int cardId, int locationId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (!user.Vendor)
                return Request.CreateResponse(HttpStatusCode.NotFound, "Surgeries not found");

            var vendorLocations = await sqlHelper.GetVendorLocations(user.ProviderID);
            if (!vendorLocations.Any(l => l.LocationID == locationId))
                return Request.CreateResponse(HttpStatusCode.NotFound, "Surgeries not found");

            var beginDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(14);
            var surgeries = await sqlHelper.SearchCases(null, null, null, null, cardId, null, null, false, beginDate, endDate, locationId);

            return Request.CreateResponse(HttpStatusCode.OK, surgeries);
        }

        /// <summary>
        /// Vendor Portal Home screen for vendors
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("vendorSurgeons/{locationId}", Name = "VendorSurgeons")]
        public async Task<HttpResponseMessage> GetVendorSurgeons(int locationId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (!user.Vendor)
                return Request.CreateResponse(HttpStatusCode.NotFound, "Surgeons not found");

            var vendorLocations = await sqlHelper.GetVendorLocations(user.ProviderID);
            if (!vendorLocations.Any(l => l.LocationID == locationId))
                return Request.CreateResponse(HttpStatusCode.NotFound, "Surgeons not found");

            var surgeons = await sqlHelper.GetSurgeons(null, locationId);

            return Request.CreateResponse(HttpStatusCode.OK, surgeons);
        }

        /// <summary>
        /// Vendor Portal Home screen for vendors
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("vendorCard/{cardId}", Name = "VendorCard")]
        public async Task<HttpResponseMessage> GetVendorCard(int cardId, int locationId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (!user.Vendor)
                return Request.CreateResponse(HttpStatusCode.NotFound, "Card not found");

            var vendorLocations = await sqlHelper.GetVendorLocations(user.ProviderID);
            if (!vendorLocations.Any(l => l.LocationID == locationId))
                return Request.CreateResponse(HttpStatusCode.NotFound, "Card not found");

            var cardItems = await sqlHelper.GetCardItems(cardId, locationId);

            var result = "Tray, Item, Quantity\r\n";
            
            foreach (var item in cardItems)
            {
                if (item.ItemType == "TRAY")
                {
                    var trayItems = await sqlHelper.GetTrayItems(item.ItemID, locationId);
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

            return ResponseHelper.CsvResponse(result);
        }

        /// <summary>
        /// Vendor Portal Home screen for vendors
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("vendorCard", Name = "PostVendorCard")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostVendorCard([FromBody] VendorCardNamePost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (!user.Vendor)
                return Request.CreateResponse(HttpStatusCode.NotFound, "Put not found");

            var result = await sqlHelper.UpdateVendorCardName(request.CardID, request.CardName, user.ProviderID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Vendor Portal Home screen for vendors
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("replicateCard", Name = "ReplicateVendorCard")]
        [HttpPost]
        public async Task<HttpResponseMessage> ReplicateVendorCard([FromBody] VendorCardNamePost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (!user.Vendor)
                return Request.CreateResponse(HttpStatusCode.NotFound, "Put not found");

            var vendorLocations = await sqlHelper.GetVendorLocations(user.ProviderID);
            if (!vendorLocations.Any(l => l.LocationID == request.LocationID))
                return Request.CreateResponse(HttpStatusCode.NotFound, "Surgeries not found");

            var result = await sqlHelper.UpdateVendorCardReplicate(request.CardID, request.CardName, request.SurgeonID, request.ProcedureProfileID, user.ProviderID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Vendor Portal Home screen for vendors
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("vendorProvider", Name = "PostVendorProvider")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostVendorProvider([FromBody] VendorCreateProviderPost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (!user.Vendor)
                return Request.CreateResponse(HttpStatusCode.NotFound, "Put not found");

            var result = await sqlHelper.InsertVendorProvider(request.Provider, user.ProviderID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Vendor Portal Home screen for vendors
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("vendorLocation", Name = "CreateVendorLocation")]
        [HttpPost]
        public async Task<HttpResponseMessage> CreateVendorLocation([FromBody] VendorCreateLocationPost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (!user.Vendor)
                return Request.CreateResponse(HttpStatusCode.NotFound, "Put not found");

            var result = await sqlHelper.InsertVendorLocation(request.ProviderID, 
                request.Location, request.Street, request.City, request.State, request.Zip, user.ProviderID);

            CacheUtil.RefreshUserCache();

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Map OPP to surgeon card
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("surgeonProfile/{procedureProfileId}", Name = "PutSurgeonProfile")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutSurgeonProfile(int procedureProfileId, [FromBody] SurgeonProfilePost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (!user.Vendor)
                return Request.CreateResponse(HttpStatusCode.NotFound, "Put not found");

            foreach (var surgeonId in request.SurgeonID)
            {
                await sqlHelper.UpdateSurgeonProfile(procedureProfileId, surgeonId, user.VendorLocationID.Value);
            }

            return Request.CreateResponse(HttpStatusCode.OK, procedureProfileId);
        }

        /// <summary>
        /// Search users
        /// </summary>
        /// <param name="nameSearchText">Will filter based on first/last name string match</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<User>))]
        [Route("SearchUsers", Name = "SearchUsers")]
        public async Task<HttpResponseMessage> GetUsers(string nameSearchText = null, int? roleId = null, int? specialtyId = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var users = await sqlHelper.SearchUsers(nameSearchText, roleId, specialtyId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, users);
        }

        /// <summary>
        /// List roles
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<User>))]
        [Route("Roles", Name = "GetRoles")]
        public async Task<HttpResponseMessage> GetRoles()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var roles = await sqlHelper.GetRoles(user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, roles);
        }

        // POST api/values
        [SwaggerOperation("CheckIn")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("Checkin", Name = "Checkin")]
        public async Task<IHttpActionResult> CheckinUser(int surgeryId, [FromBody]User user)
        {
            var userSecurity = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.CheckInUser(user, surgeryId);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("CheckOut")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("Checkout", Name = "Checkout")]
        public async Task<IHttpActionResult> CheckoutUser(int surgeryId, [FromBody]User user)
        {
            var userSecurity = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.CheckOutUser(user, surgeryId);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("CheckOut")]
        
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("Reviewed", Name = "Reviewed")]
        public async Task<IHttpActionResult> WorkupReviewed(int surgeryId, [FromBody]User user)
        {
            var userSecurity = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.WorkupReviewed(user, surgeryId);

            return Ok();
        }

        // POST api/Account/ResetPassword
        [Route("ResetPassword")]
        public async Task<IHttpActionResult> ResetPassword(int userId, [FromBody]SetPasswordBindingModel model)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model.NewPassword != model.ConfirmPassword)
                return BadRequest("Passwords do not match");

            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            var secureUser = await sqlHelper.GetSecureUser(null, userId);

            if ((user.RoleType == "Internal" && user.RoleType == "Admin") ||
                (secureUser?.SelectedLocation == user.SelectedLocation))
            {
                var code = await userManager.GeneratePasswordResetTokenAsync(secureUser.UserAuthID.ToString());

                var result =
                    await userManager.ResetPasswordAsync(secureUser.UserAuthID.ToString(), code, model.NewPassword);

                if (!result.Succeeded)
                {
                    return BadRequest();
                }
            }

            return Ok();
        }

        // PUT api/values/5
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.OK, Type=typeof(int))]
        public async Task<IHttpActionResult> Post([FromBody]UserPost model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            var existing = await userManager.FindByEmailAsync(model.Email);
            if (existing != null)
                return Conflict();

            var authenticationUser = new ApplicationUser() { UserName = model.Email, Email = model.Email };

            var result = await userManager.CreateAsync(authenticationUser, model.Password);

            // Whatever role the current user is in - they can only add users to this role
            var currentAuthId = HttpContext.Current.User.Identity.GetUserId();
            var currentRoles = await userManager.GetRolesAsync(currentAuthId);
            var currentRole = currentRoles?.FirstOrDefault();

            if (currentRole == null)
                throw new Exception("Unable to locate authenticated user");

            userManager.AddToRole(authenticationUser.Id, currentRole);

            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.FirstOrDefault());
            }

            var userAuthId = new Guid(authenticationUser.Id);

            try
            {
                var applicationUserId = await sqlHelper.CreateUser(userAuthId, model.RoleID, model.SpecialtyID,
                    model.FirstName, model.LastName,
                    model.Email, model.CellPhone, model.Initials, model.Title, user.SelectedLocation);

                return Ok(applicationUserId);
            }
            catch (Exception ex)
            {
                // roll back creation of aspnet user
                await userManager.DeleteAsync(authenticationUser);

                throw ex;
            }
        }

        // PUT api/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Put(int userId, [FromBody]UserEdit model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var authUserSecurity = await sqlHelper.GetSecureUser(null, userId);

            if (authUserSecurity.SelectedLocation == user.SelectedLocation)
            {
                var applicationUser = await sqlHelper.UpdateUser(userId, (int)model.RoleID, model.SpecialtyID, model.FirstName, model.LastName,
                    model.Email, model.CellPhone, model.Initials, model.Title, user.SelectedLocation);

                // make certain user auth matches what we sent
                var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var authUser = await userManager.FindByIdAsync(authUserSecurity.UserAuthID.ToString());

                if (authUser.Email != model.Email)
                {
                    authUser.UserName = model.Email;
                    authUser.Email = model.Email;

                    await userManager.UpdateAsync(authUser);
                }
            }

            return Ok();
        }

        // DELETE api/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("{userId}")]
        public async Task<IHttpActionResult> Delete(int userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            var applicationUser = await sqlHelper.GetUser(user.SelectedLocation,  userId);

            ApplicationUser authenticationUser = null;
            
            if (!string.IsNullOrEmpty(applicationUser?.Email))
                authenticationUser = await userManager.FindByEmailAsync(applicationUser.Email);

            if (authenticationUser != null)
            {
                var authResult = await userManager.RemovePasswordAsync(authenticationUser.Id);
            }

            var applicationDeletion = await sqlHelper.DeleteUser(userId, user.SelectedLocation);

            return Ok();
        }
    }
}