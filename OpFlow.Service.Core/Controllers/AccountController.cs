using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using OpFlow.Service.DataAccess;
using OpFlow.Service.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web;

// https://github.com/MikeWasson/LocalAccountsApp
namespace OpFlow.Service.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AccountController : ControllerBase
    {
        private const string LocalLoginProvider = "Local";
        private UserManager<ApplicationUser> _userManager;

        private SqlHelper _sqlHelper;

        public AccountController(SqlHelper sqlHelper, UserManager<ApplicationUser> userManager)
        {
            _sqlHelper = sqlHelper;
            _userManager = userManager;
        }

        public UserManager<ApplicationUser> UserManager => _userManager;

        // GET api/Account/UserInfo
        [AllowAnonymous]
        [Route("token")]
        [HttpPost()]
        public async Task<ActionResult> Login([FromBody] RegisterBindingModel authModel)
        {
            if (authModel.Email == null || authModel.Password == null) return Unauthorized();

            var user = await _userManager.FindByEmailAsync(authModel.Email);
            if (user == null) return Unauthorized();

            var passwordValid = await _userManager.CheckPasswordAsync(user, authModel.Password);
            if (!passwordValid) return Unauthorized();

            var authentication = JsonWebToken.GenerateToken(user);

            if (authentication == null) return Unauthorized();

            var result = new AuthToken()
            {
                AccessToken = authentication,
                TokenType = "bearer",
                ExpiresIn = 43199,
                Issued = DateTime.Now.ToString("o", CultureInfo.InvariantCulture),
                Expires = DateTime.Now.AddHours(12).ToString("o", CultureInfo.InvariantCulture)
            };

            return Ok(result);
        }

        // GET api/Account/UserInfo
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("UserInfo")]
        public UserInfoViewModel GetUserInfo()
        {
            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            return new UserInfoViewModel
            {
                Email = User.Identity.Name,
                HasRegistered = externalLogin == null,
                LoginProvider = externalLogin?.LoginProvider
            };
        }

        // GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
        [Route("ManageInfo")]
        public async Task<ActionResult<ManageInfoViewModel>> GetManageInfo(string returnUrl, bool generateState = false)
        {
            IdentityUser user = await UserManager.FindByIdAsync(User.Identity.Name);

            if (user == null)
            {
                return null;
            }

            List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();


            if (user.PasswordHash != null)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = LocalLoginProvider,
                    ProviderKey = user.UserName,
                });
            }

            return new ManageInfoViewModel
            {
                LocalLoginProvider = LocalLoginProvider,
                Email = user.UserName,
                Logins = logins
            };
        }

        // POST api/Account/ChangePassword
        [Route("ChangePassword")]
        public async Task<ActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var appUser = await UserManager.GetUserAsync(HttpContext.User);
            IdentityResult result = await UserManager.ChangePasswordAsync(appUser, model.OldPassword,
                model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/SetPassword
        [Route("SetPassword")]
        public async Task<ActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var appUser = await UserManager.GetUserAsync(HttpContext.User);
            IdentityResult result = await UserManager.AddPasswordAsync(appUser, model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // GET api/Account/ApiKey
        [AllowAnonymous]
        [Route("ApiKey", Name = "ApiKeyLogin")]
        [HttpPost]
        public async Task<ActionResult> ApiKeyLogin([FromBody] ApiKeyRequest request)
        {
            if (request?.ApiKey == null) return Unauthorized();

            var apiKey = await _sqlHelper.GetApiKey(request.ApiKey);

            var user = await _userManager.FindByIdAsync(apiKey.UserAuthID.ToString());

            var authentication = JsonWebToken.GenerateToken(user);

            if (authentication == null) return Unauthorized();

            var result = new AuthToken()
            {
                AccessToken = authentication,
                TokenType = "bearer",
                ExpiresIn = 43199,
                Issued = DateTime.Now.ToString("o", CultureInfo.InvariantCulture),
                Expires = DateTime.Now.AddHours(12).ToString("o", CultureInfo.InvariantCulture)
            };

            return Ok(result);
        }

        public class ApiKeyRequest
        {
            public string ApiKey { get; set; }
        }

        // POST api/Account/Register
        //[AllowAnonymous]
        [Route("Register")]
        public async Task<ActionResult> Register(RegisterBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };

            IdentityResult result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        #region Helpers

        private ActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                throw new InvalidOperationException();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider)
                };

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.Name
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return WebEncoders.Base64UrlEncode(data);
            }
        }

        #endregion
    }
}