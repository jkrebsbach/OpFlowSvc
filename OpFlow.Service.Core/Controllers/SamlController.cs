using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace OpFlow.Service.Controllers
{
    [AllowAnonymous]
    [Route("Saml")]
    public class SamlController : Controller
    {
        private const string CertKeyName = "Cert";
        private const string RequestorCertKeyName = "RequestorCert";
        /*
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
            //string issuerUrl = Util.GetAbsoluteUrl(context, "~/");
            string issuerUrl = "http://authentication.operativeflow.com/adfs/services/trust";

            // Construct the assertion Consumer Service Url.
            string assertionConsumerServiceUrl = string.Format("{0}?{1}={2}", Util.GetAbsoluteUrl(context, "~/saml/login"),
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

                var exportedKeyMaterial = x509Certificate.PrivateKey.ToXmlString(true);

                var key = new RSACryptoServiceProvider(new CspParameters(24)); // PROV_RSA_AES 
                key.PersistKeyInCsp = false;
                key.FromXmlString(exportedKeyMaterial);

                // Sign the authentication request.
                authnRequest.Sign(key, x509Certificate, "http://www.w3.org/2001/04/xmlenc#sha256", "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"); // In this case we sign the entity descriptor. 
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
                    X509Certificate2 x509Certificate = (X509Certificate2)System.Web.HttpContext.Current.Application[RequestorCertKeyName];

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

                Assertion samlAssertion = null;

                var encAssertions = samlResponse.GetEncryptedAssertions();
                var assertions = samlResponse.GetAssertions();

                if (encAssertions.Count > 0)
                {
                    foreach (var encryptedAssertion in encAssertions)
                    {
                        if (encryptedAssertion == null)
                        {
                            continue;
                        }

                        var decryptionKey = (X509Certificate2)HttpContext.Application[CertKeyName];

                        // Decrypt the encrypted assertion.
                        samlAssertion = encryptedAssertion.Decrypt(decryptionKey.PrivateKey, null);
                    }
                }
                else
                {
                    throw new ApplicationException("No encrypted assertions found in the SAML response");
                }

                // Get the asserted identity.
                if (samlAssertion == null && assertions.Length > 0)
                {
                    samlAssertion = samlResponse.GetAssertions()[0];
                }
                else
                {
                    throw new ApplicationException("No assertions found in the SAML response");
                }


                if (samlAssertion == null)
                    throw new Exception("Saml assertion null");

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

                var firstName = string.Empty;
                var lastName = string.Empty;
                var opflowRoles = new List<string>();

                //If you need to add custom attributes, uncomment the following code
                foreach (var attributeStatement in samlAssertion.AttributeStatements ?? new AttributeStatement [0])
                {
                    // If you need to decrypt encrypted attributes, refer to this topic: http://www.samlcomponent.net/encrypting-and-decrypting-saml-response-xml
                    foreach (ComponentPro.Saml2.Attribute attribute in attributeStatement.Attributes)
                    {
                        if (attribute.Name == "fname")
                            firstName = ParseStringValue(attribute.Values);

                        if (attribute.Name == "lname")
                            lastName = ParseStringValue(attribute.Values);

                        // Process your custom attribute here.
                        // ...
                        if (attribute.Name == "role")
                            opflowRoles = attribute.Values.ToList().Select(a => a.Data?.ToString() ?? "").ToList();
                    }
                }

                #endregion

                // attemp to generate universally unique identifiers
                var email = userName;
                if (userName.IndexOf("@", StringComparison.Ordinal) <= 0)
                {
                    email = userName + "@unc.saml";
                }

                var token = await ApplicationOAuthProvider.GenerateBearerToken(userName, email, firstName, lastName, opflowRoles);

                if (token == null)
                {
                    //var strContent = JsonConvert.SerializeObject(opflowRoles);
                    //return Content("We got here, and not there");
                    return View("Unauthorized");
                }
                
                // Set authentication cookie.
                System.Web.Security.FormsAuthentication.SetAuthCookie(userName, false);
                
                // Redirect to the requested URL.
                var responseUrl = ConfigurationManager.AppSettings["ResponseURL"];
                //return Redirect(samlResponse.RelayState + "?authToken=" + token);
                return Redirect($"{responseUrl}?authToken={token}");

                #endregion
            }

            catch (Exception ex)
            {
                Models.LogHelper.LogException(ex);

                System.Diagnostics.Trace.Write("ServiceProvider - An Error occurred: " + ex.ToString());
                throw ex;
            }

            return View();
        }

        private string ParseStringValue(IList<AttributeValue> values)
        {
            return values.FirstOrDefault()?.Data.ToString() ?? string.Empty;
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
        public async Task<ActionResult> SamlAttributes()
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetUser(user.SelectedLocation, user.UserAuthID);

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
        */
    }
}