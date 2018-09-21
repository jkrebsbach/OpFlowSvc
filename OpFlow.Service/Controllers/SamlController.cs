using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using ComponentPro.Saml;
using ComponentPro.Saml.Binding;
using ComponentPro.Saml2;
using ComponentPro.Saml2.Binding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using OpFlow.Service.App_Start;
using OpFlow.Service.DataAccess;
using OpFlow.Service.Providers;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("Saml")]
    public class SamlController : Controller
    {
        private const string CertKeyName = "Cert";
        private const string RequestorCertKeyName = "RequestorCert";

        /// <summary>
        /// Handles the IdpLogin button to requests login at the Identify Provider site.
        /// </summary>
        [Route("IdpLogin", Name = "IdpLogin")]
        [HttpPost]
        public ActionResult IdPLogin()
        {
            var context = HttpContext;

            var spToIdPBinding = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST";
            var idPToSPBinding = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST";

            // Create the authentication request.
            AuthnRequest authnRequest = BuildAuthenticationRequest(context, idPToSPBinding, spToIdPBinding);

            // Create and cache the relay state so we remember which SP resource the user wishes 
            // to access after SSO.
            string spResourceUrl = Util.GetAbsoluteUrl(context, FormsAuthentication.GetRedirectUrl("", false));
            string relayState = Guid.NewGuid().ToString();
            SamlSettings.CacheProvider.Insert(relayState, spResourceUrl, new TimeSpan(1, 0, 0));

            
            // Send the authentication request to the identity provider over the selected binding.
            string idpUrl = string.Format("{0}?{1}={2}", WebConfigurationManager.AppSettings["SingleSignonIdProviderUrl"], Util.BindingVarName, HttpUtility.UrlEncode(spToIdPBinding));

            switch (spToIdPBinding)
            {
                case SamlBindingUri.HttpRedirect:
                    X509Certificate2 x509Certificate = (X509Certificate2)context.Application[CertKeyName];

                    authnRequest.Redirect(Response, idpUrl, relayState, x509Certificate.PrivateKey);
                    break;

                case SamlBindingUri.HttpPost:
                    authnRequest.SendHttpPost(Response, idpUrl, relayState);

                    // Don't send this form.
                    Response.End();
                    break;

                case SamlBindingUri.HttpArtifact:
                    // Create the artifact.
                    string identificationUrl = Util.GetAbsoluteUrl(context, "~/");
                    Saml2ArtifactType0004 httpArtifact = new Saml2ArtifactType0004(SamlArtifact.GetSourceId(identificationUrl), SamlArtifact.GetHandle());

                    // Cache the authentication request for subsequent sending using the artifact resolution protocol.
                    SamlSettings.CacheProvider.Insert(httpArtifact.ToString(), authnRequest.GetXml(), new TimeSpan(1, 0, 0));

                    // Send the artifact.
                    httpArtifact.Redirect(Response, idpUrl, relayState);
                    break;
            }

            // If we got this far, something failed, redisplay form
            return View();
        }

        /// <summary>
        /// Builds an authentication request.
        /// </summary>
        /// <returns>The authentication request.</returns>
        private AuthnRequest BuildAuthenticationRequest(HttpContextBase context, string idpToSPBindingList, string spToIdPBinding)
        {
            string issuerUrl = Util.GetAbsoluteUrl(context, "~/");
            // Construct the assertion Consumer Service Url.
            string assertionConsumerServiceUrl = string.Format("{0}?{1}={2}", Util.GetAbsoluteUrl(context, "~/AssertionService"),
                Util.BindingVarName, HttpUtility.UrlEncode(idpToSPBindingList));

            // Create the authentication request.
            AuthnRequest authnRequest = new AuthnRequest();
            authnRequest.Destination = WebConfigurationManager.AppSettings["SingleSignonIdProviderUrl"];
            authnRequest.Issuer = new Issuer(issuerUrl);
            authnRequest.ForceAuthn = false;
            authnRequest.NameIdPolicy = new NameIdPolicy(null, null, true);
            authnRequest.ProtocolBinding = idpToSPBindingList;
            authnRequest.AssertionConsumerServiceUrl = assertionConsumerServiceUrl;

            if (spToIdPBinding != SamlBindingUri.HttpRedirect)
            {
                // Get the certificate
                X509Certificate2 x509Certificate = (X509Certificate2)context.Application[CertKeyName];

                // Sign the authentication request.
                authnRequest.Sign(x509Certificate);
            }
            return authnRequest;

        }

        /// <summary>
        /// Endpoint to login users via SAML
        /// </summary>
        /// <returns></returns>
        [Route("Login", Name = "SamlLogin")]
        [HttpPost]
        public async Task<ActionResult> SamlLogin()
        {
            try
            {
                #region Receive SAML Response

                // Create a SAML response from the HTTP request.
                ComponentPro.Saml2.Response samlResponse = ComponentPro.Saml2.Response.Create(Request);

                // Is it signed?
                if (samlResponse.IsSigned())
                {
                    // Loaded the previously loaded certificate.
                    X509Certificate2 x509Certificate = (X509Certificate2)System.Web.HttpContext.Current.Application[CertKeyName];

                    // Validate the SAML response with the certificate.
                    if (!samlResponse.Validate(x509Certificate))
                    {
                       throw new ApplicationException("SAML response signature is not valid.");
                    }
                }

                #endregion

                #region Process the response

                // Success?
                if (!samlResponse.IsSuccess())
                {
                    throw new ApplicationException("SAML response is not success");
                }

                Assertion samlAssertion;

                // Define ENCRYPTEDSAML preprocessor flag if you wish to decrypt the SAML response.
#if ENCRYPTEDSAML
                if (samlResponse.GetEncryptedAssertions().Count > 0)
                {
                    EncryptedAssertion encryptedAssertion = samlResponse.GetEncryptedAssertions()[0];

                    var decryptionKey = (X509Certificate2)HttpContext.Application[CertKeyName];

                    // Decrypt the encrypted assertion.
                    samlAssertion = encryptedAssertion.Decrypt(decryptionKey.PrivateKey, null);
                }
                else
                {
                    throw new ApplicationException("No encrypted assertions found in the SAML response");
                }
#else
                // Get the asserted identity.
                if (samlResponse.GetAssertions().Length > 0)
                {
                    samlAssertion = samlResponse.GetAssertions()[0];
                }
                else
                {
                    throw new ApplicationException("No assertions found in the SAML response");
                }
#endif

                // Get the subject name identifier.
                string userName;

                if (samlAssertion.Subject.NameId != null)
                {
                    userName = samlAssertion.Subject.NameId.NameIdentifier;
                }
                else
                {
                    throw new ApplicationException("Name identifier not found in subject");
                }

                #region Extract Custom Attributes

                // If you need to add custom attributes, uncomment the following code
                //if (samlAssertion.AttributeStatements.Count > 0)
                //{
                //    foreach (AttributeStatement attributeStatement in samlAssertion.AttributeStatements)
                //    {
                //        // If you need to decrypt encrypted attributes, refer to this topic: http://www.samlcomponent.net/encrypting-and-decrypting-saml-response-xml
                //        foreach (ComponentPro.Saml2.Attribute attribute in attributeStatement.Attributes)
                //        {
                //            // Process your custom attribute here.
                //            // ...
                //        }
                //    }
                //}

                #endregion

                // Set authentication cookie.
                System.Web.Security.FormsAuthentication.SetAuthCookie(userName, false);

                var email = userName;
                if (userName.IndexOf("@") <= 0)
                {
                    email = userName + "@opflowtech.com";
                }

                var token = await ApplicationOAuthProvider.GenerateBearerToken(userName, email, new List<string>());

                // Redirect to the requested URL.
                var responseUrl = ConfigurationManager.AppSettings["ResponseURL"];
                //return Redirect(samlResponse.RelayState + "?authToken=" + token);
                return Redirect($"{responseUrl}?authToken={token}");

                #endregion
            }

            catch (Exception exception)
            {
                System.Diagnostics.Trace.Write("ServiceProvider - An Error occurred: " + exception.ToString());
                throw exception;
            }

            return View();
        }

        /// <summary>
        /// Endpoint to logout users via SAML
        /// </summary>
        /// <returns></returns>
        [Route("Logout", Name = "SamlLogout")]
        [HttpPost]
        public async Task<ActionResult> SamlLogout()
        {
            SamlHelper.Logout(Request, Response);

            // find current user, sign them out
            try
            {
                var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
                await userManager.UpdateSecurityStampAsync(HttpContext.User.Identity.GetUserId());
            }
            catch (Exception ex)
            {
                throw;
            }

            return null;
        }
        /// <summary>
        /// Endpoint to excpose SAML artifacts
        /// </summary>
        /// <returns></returns>
        [Route("Artifacts", Name = "SamlArtifacts")]
        [HttpPost]
        public ActionResult SamlArtifacts()
        {
            SamlHelper.ResolveArtifacts(Request, Response);

            var result = "SUCCESS";

            return Json(new { result });
        }
        /// <summary>
        /// Endpoint to login users via SAML
        /// </summary>
        /// <returns></returns>
        [Route("Attributes", Name = "SamlAttributes")]
        [HttpPost]
        public ActionResult SamlAttributes()
        {
            var user = CacheUtil.GetUserSecurity();
            var username = HttpContext.User.Identity.Name;

            var result = DataAccess.SqlHelper.GetUser(user.ProviderID, user.LocationID, user.UserAuthID);

            if (result == null)
                throw new HttpException(404, "User not found");

            return Json( new { result });
        }

        public class Util
        {
            /// <summary>
            /// The query string variable that indicates the IdentityProvider to ServiceProvider binding.
            /// </summary>
            public const string BindingVarName = "binding";

            /// <summary>
            /// The query string parameter that contains error description for the login failure. 
            /// </summary>
            public const string ErrorVarName = "error";

            public static string GetAbsoluteUrl(HttpContextBase context, string relativeUrl)
            {
                return new Uri(context.Request.Url, System.Web.Mvc.UrlHelper.GenerateContentUrl(relativeUrl, context)).ToString();
            }
        }
    }
}