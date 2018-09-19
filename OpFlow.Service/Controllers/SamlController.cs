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
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using ComponentPro.Saml2;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using OpFlow.Service.App_Start;
using OpFlow.Service.DataAccess;
using OpFlow.Service.Providers;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [AllowAnonymous]
    public class SamlController : Controller
    {
        private const string CertKeyName = "Cert";
        private const string RequestorCertKeyName = "RequestorCert";

        /// <summary>
        /// Endpoint to login users via SAML
        /// </summary>
        /// <returns></returns>
        [Route("Saml/Login", Name = "SamlLogin")]
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
        [Route("Saml/Logout", Name = "SamlLogout")]
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
        [Route("Saml/Artifacts", Name = "SamlArtifacts")]
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
        [Route("Saml/Attributes", Name = "SamlAttributes")]
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
    }
}