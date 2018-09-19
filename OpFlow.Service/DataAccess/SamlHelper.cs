
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Xml;
using ComponentPro.Saml;
using ComponentPro.Saml.Diagnostics;
using ComponentPro.Saml1;
using ComponentPro.Saml2;
using ComponentPro.Saml2.Binding;
using Assertion = ComponentPro.Saml2.Assertion;
using Response = ComponentPro.Saml2.Response;
using Status = ComponentPro.Saml2.Status;

namespace OpFlow.Service.DataAccess
{
    public class SamlHelper
    {
        public const string CertKeyName = "Cert";

        private static X509Certificate2 _appCert;

        private static X509Certificate2 AppCert
        {
            get
            {
                if (_appCert == null)
                {
                    _appCert = (X509Certificate2)HttpContext.Current.Application[CertKeyName];
                }

                return _appCert;
            }
        }

        public static string Login(HttpRequest request)
        {
            try
            {
                #region Receive SAML Response

                // Create a SAML response from the HTTP request.
                ComponentPro.Saml2.Response samlResponse = ComponentPro.Saml2.Response.Create(request);

                // Is it signed?
                if (samlResponse.IsSigned())
                {
                    // Loaded the previously loaded certificate.
                    var x509Certificate = AppCert;

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

                    // Load the private key.
                    // Consider caching the loaded key in production environment for better performance.
                    //var templatePath = System.Web.HttpContext.Current.Request.MapPath(pfxPath);
                    //X509Certificate2 decryptionKey = new X509Certificate2(templatePath, "password");
                    var decryptionKey = AppCert;

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
                FormsAuthentication.SetAuthCookie(userName, false);

                // Redirect to the requested URL.
                return samlResponse.RelayState;

                #endregion
            }

            catch (Exception exception)
            {
                //Trace.Write("ServiceProvider", "An Error occurred", exception);
                throw;
            }
        }

        public static void Logout(HttpRequestBase request, HttpResponseBase response)
        {
            try
            {
                // Get the previously loaded certificate.
                X509Certificate2 x509Certificate = AppCert;

                LogoutResponse logoutResponse;
                LogoutRequest logoutRequest;

                SamlMessageUtil.CreateLogoutMessage(request, out logoutResponse, out logoutRequest, x509Certificate.PublicKey.Key);

                if (logoutRequest != null)
                    HandleLogoutRequest(logoutRequest, x509Certificate, request, response);
                else
                    HandleLogoutResponse(logoutResponse, response);
            }

            catch (System.Exception exception)
            {
                SamlTrace.Log(LogLevel.Info, "An error occurred", exception);
                throw;
            }
        }

        public static void ResolveArtifacts(HttpRequestBase request, HttpResponseBase response)
        {
            try
            {
                // Create an artifact resolve from the request with XML data extracted from the request stream.
                ArtifactResolve artifactResolve = ArtifactResolve.Create(request);

                // Create the artifact type 0004.
                Saml2ArtifactType0004 httpArtifact = new Saml2ArtifactType0004(artifactResolve.Artifact.ArtifactValue);

                // Remove the artifact state from the cache.
                XmlElement samlResponseXml = (XmlElement)SamlSettings.CacheProvider.Remove(httpArtifact.ToString());

                if (samlResponseXml == null) return;

                // Create an artifact response containing the cached SAML message.
                ArtifactResponse artifactResponse = new ArtifactResponse();
                artifactResponse.Issuer = new Issuer(new Uri(request.Url.ToString()).ToString());
                artifactResponse.Message = samlResponseXml;

                // Send the artifact response.
                artifactResponse.Send(response);
            }
            catch (Exception exception)
            {
                SamlTrace.Log(LogLevel.Info, "An Error occurred", exception);
                throw;
            }
        }

        private static void HandleLogoutRequest(LogoutRequest message, X509Certificate2 x509Certificate, HttpRequestBase request, HttpResponseBase response)
        {
            // This is the logged in ID.
            string nameId = message.NameId.NameIdentifier;

            // Do something with the ID, like writing a record about the activity of this user.
            // ...

            // Not needed in this context?...
            //// Logout locally.
            //FormsAuthentication.SignOut();
            //Session.Abandon();

            #region Create and Send LogoutResponse
            // We need to send back a LogoutResponse to the IdP
            LogoutResponse logoutResponse = new LogoutResponse();
            logoutResponse.Status = new Status(SamlPrimaryStatusCode.Success, null);
            logoutResponse.Issuer = new Issuer(GetAbsoluteUrl(request, "~/"));

            // Send the logout response.
            logoutResponse.Redirect(response, WebConfigurationManager.AppSettings["LogoutIdProviderUrl"], null, x509Certificate.PrivateKey);
            #endregion
        }

        private static void HandleLogoutResponse(LogoutResponse message, HttpResponseBase response)
        {
            SamlTrace.Log(LogLevel.Info, "Received a Logout Response");

            // Redirect to the default page.
            response.Redirect("~/", false);
        }

        private static string GetAbsoluteUrl(HttpRequestBase request, string relativeUrl)
        {
            return new Uri(request.Url, relativeUrl).ToString();
        }
    }
}