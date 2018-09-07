using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

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

            // Load the certificate.
            string fileName = Path.Combine(HttpRuntime.AppDomainAppPath, "X509Certificate.cer");
            LoadCertificate(fileName, "Summer1!");
        }

        public const string CertKeyName = "Cert";

        /// <summary>
        /// Loads the certificate file.
        /// </summary>
        /// <param name="fileName">The certificate file name.</param>
        /// <param name="password">The password for this certificate file.</param>
        private void LoadCertificate(string fileName, string password)
        {
            X509Certificate2 cert = new X509Certificate2(fileName, password, X509KeyStorageFlags.MachineKeySet);

            Application[CertKeyName] = cert;
        }
    }
}
