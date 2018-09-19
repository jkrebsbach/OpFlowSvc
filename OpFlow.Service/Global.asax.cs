using System;
using System.Collections.Generic;
using System.Deployment.Internal.CodeSigning;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using OpFlow.Service.DataAccess;

namespace OpFlow.Service
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapMvcAttributeRoutes();
        }

        protected void Application_Start()
        {
            ComponentPro.Licensing.Saml.LicenseManager.SetLicenseKey(ComponentProLicense.Key);

            AreaRegistration.RegisterAllAreas();

            RegisterRoutes(RouteTable.Routes);

            GlobalConfiguration.Configure(WebApiConfig.Register);

            // Load the certificates.
            LoadCertificate();
            LoadRequestorCertificate();
        }

        public const string CertKeyName = "Cert";
        private const string RequestorCertKeyName = "RequestorCert";

        private void LoadCertificate()
        {
            //var sha256DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256";
            //var sha256SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
            //CryptoConfig.AddAlgorithm(typeof(RSAPKCS1SHA256SignatureDescription), sha256SignatureMethod);

            //var decryptionKeyTask = KeyVaultHelper.GetCertificate();
            //decryptionKeyTask.Wait();

            //Application[CertKeyName] = decryptionKeyTask.Result;

            X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            certStore.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certCollection = certStore.Certificates.Find(
                X509FindType.FindByThumbprint,
                // Replace below with your certificate's thumbprint
                "0A16E68470EB22FAA7D7874C997BD9E8CBBAE1FA",
                false);
            
            // Get the first cert with the thumbprint
            if (certCollection.Count > 0)
            {
                X509Certificate2 cert = certCollection[0];

                // Use certificate
                Console.WriteLine(cert.FriendlyName);
                Application[CertKeyName] = cert;
                return;
            }


            certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            certStore.Open(OpenFlags.ReadOnly);
            certCollection = certStore.Certificates.Find(
                X509FindType.FindByThumbprint,
                // Replace below with your certificate's thumbprint
                "0A16E68470EB22FAA7D7874C997BD9E8CBBAE1FA",
                false);

            // Get the first cert with the thumbprint
            if (certCollection.Count > 0)
            {
                X509Certificate2 cert = certCollection[0];

                // Use certificate
                Console.WriteLine(cert.FriendlyName);
                Application[CertKeyName] = cert;
            }
        }

        private void LoadRequestorCertificate()
        {
            var certPath = Path.Combine(HttpRuntime.AppDomainAppPath, @"FedTest.cer");
            var cert = X509Certificate2.CreateFromCertFile(certPath);

            Application[RequestorCertKeyName] = cert;
        }

        /// <summary>
        /// Loads the certificate file.
        /// </summary>
        /// <param name="fileName">The certificate file name.</param>
        /// <param name="password">The password for this certificate file.</param>
        private void LoadCertificate(string fileName, string password)
        {
            // Add RSA-SHA-256 algorithm to the CryptoConfig.

            var sha256DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256";
            var sha256SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
            CryptoConfig.AddAlgorithm(typeof(RSAPKCS1SHA256SignatureDescription), sha256SignatureMethod);

            X509Certificate2 cert = new X509Certificate2(fileName, password,
                X509KeyStorageFlags.MachineKeySet |
                X509KeyStorageFlags.PersistKeySet |
                X509KeyStorageFlags.Exportable);

            Application[CertKeyName] = cert;
        }
    }
}
