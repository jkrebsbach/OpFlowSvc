using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.OAuth;
using OpFlow.Service.App_Start;
using OpFlow.Service.DataAccess;
using OpFlow.Service.Models;

namespace OpFlow.Service.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly string _publicClientId;

        public ApplicationOAuthProvider(string publicClientId)
        {
            _publicClientId = publicClientId ?? throw new ArgumentNullException("publicClientId");
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var userManager = context.OwinContext.GetUserManager<ApplicationUserManager>();

            var user = await userManager.FindAsync(context.UserName, context.Password);

            if (user == null)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;
            }

            var oAuthIdentity = await user.GenerateUserIdentityAsync(userManager,
               OAuthDefaults.AuthenticationType);
            var cookiesIdentity = await user.GenerateUserIdentityAsync(userManager,
                CookieAuthenticationDefaults.AuthenticationType);

            var properties = CreateProperties(user.UserName);
            var ticket = new AuthenticationTicket(oAuthIdentity, properties);
            context.Validated(ticket);
            context.Request.Context.Authentication.SignIn(cookiesIdentity);
        }

        public static async Task<string> GenerateBearerToken(string username, string userEmail, string firstName, string lastName, List<string> opflowRoles)
        {
            try
            {
                var owinContext = HttpContext.Current.GetOwinContext();
                var userManager = owinContext.GetUserManager<ApplicationUserManager>();

                var user = await userManager.FindByNameAsync(username);
                
                if (user == null)
                {
                    int? roleId = null;

                    foreach (var opflowRole in opflowRoles)
                    {
                        switch (opflowRole)
                        {
                            case "CN=SC-UNCH-OPFLOW-Surgeon,OU=Applications,OU=Secure,OU=Groups,DC=unch,DC=unc,DC=edu":
                                roleId = 1;
                                break;
                            case
                            "CN=SC-UNCH-OPFLOW-Circulator,OU=Applications,OU=Secure,OU=Groups,DC=unch,DC=unc,DC=edu":
                                roleId = 2;
                                break;
                            case
                            "CN=SC-UNCH-OPFLOW-SurgicalTech,OU=Applications,OU=Secure,OU=Groups,DC=unch,DC=unc,DC=edu":
                                roleId = 3;
                                break;
                            case "CN=SC-UNCH-OPFLOW-CRNA,OU=Applications,OU=Secure,OU=Groups,DC=unch,DC=unc,DC=edu":
                                roleId = 4;
                                break;
                            case "CN=SC-UNCH-OPFLOW-FrontDesk,OU=Applications,OU=Secure,OU=Groups,DC=unch,DC=unc,DC=edu":
                                roleId = 7;
                                break;
                            case
                            "CN=SC-UNCH-OPFLOW-Administration,OU=Applications,OU=Secure,OU=Groups,DC=unch,DC=unc,DC=edu":
                                roleId = 9;
                                break;
                            case
                            "CN=SC-UNCH-OPFLOW-Anesthesiologist,OU=Applications,OU=Secure,OU=Groups,DC=unch,DC=unc,DC=edu":
                                roleId = 11;
                                break;
                            case
                            "CN=SC-UNCH-OPFLOW-PostOpNurse,OU=Applications,OU=Secure,OU=Groups,DC=unch,DC=unc,DC=edu":
                                roleId = 13;
                                break;
                            default:
                                break;
                        }
                    }

                    if (roleId == null)
                        return null;

                    user = new ApplicationUser()
                    {
                        UserName = username,
                        Email = userEmail
                    };
                    var result = await userManager.CreateAsync(user);

                    if (!result.Succeeded)
                        throw new Exception("Unable to create new user");

                    var userAuthId = new Guid(user.Id);

                    // SAML hard coded to UNC
                    var sqlHelper = new SqlHelper();
                    var userId = await sqlHelper.CreateUser(userAuthId, roleId, null, firstName, lastName,
                        username, null, null, "", 1);
                }

                var oAuthIdentity = await user.GenerateUserIdentityAsync(userManager,
                    OAuthDefaults.AuthenticationType);
                var cookiesIdentity = await user.GenerateUserIdentityAsync(userManager,
                    CookieAuthenticationDefaults.AuthenticationType);

                var properties = CreateProperties(user.UserName);
                var ticket = new AuthenticationTicket(oAuthIdentity, properties);

                var secureDataFormat = new TicketDataFormat(new MachineKeyProtector());
                return secureDataFormat.Protect(ticket);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private class MachineKeyProtector : IDataProtector
        {
            private readonly string[] _purpose =
            {
                typeof(OAuthAuthorizationServerMiddleware).Namespace,
                "Access_Token",
                "v1"
            };

            public byte[] Protect(byte[] userData)
            {
                return System.Web.Security.MachineKey.Protect(userData, _purpose);
            }

            public byte[] Unprotect(byte[] protectedData)
            {
                return System.Web.Security.MachineKey.Unprotect(protectedData, _purpose);
            }
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // Resource owner password credentials does not provide a client ID.
            if (context.ClientId == null)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == _publicClientId)
            {
                Uri expectedRootUri = new Uri(context.Request.Uri, "/");

                if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                {
                    context.Validated();
                }
            }

            return Task.FromResult<object>(null);
        }

        public static AuthenticationProperties CreateProperties(string userName)
        {
            IDictionary<string, string> data = new Dictionary<string, string>
            {
                { "userName", userName }
            };
            return new AuthenticationProperties(data);
        }
    }
}