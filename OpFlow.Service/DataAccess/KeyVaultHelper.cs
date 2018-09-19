using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace OpFlow.Service.DataAccess
{
    public static class KeyVaultHelper
    {
        const string CLIENT_ID = "e31f24bd-7908-4c51-9fd1-3e3e004ccd1b";
        const string CLIENT_SECRET = "uhX0xERxt1KGFZg/FMj4YV+Df4ryuFvRVsFiqCVp3mc=";

        const string CERTIFICATE_IDENTIFIER = "https://opflowvault.vault.azure.net/certificates/OpFlowWebCert/b9d8cda9055444378484c4b860a465cf";

        const string KEY_VAULT_IDENTIFIER = "https://opflowvault.vault.azure.net/";
        const string CERTIFICATE_NAME = "OpFlowWebCert";

        private static KeyVaultClient GetClient() => new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(async (string authority, string resource, string scope) =>
        {
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            ClientCredential clientCred = new ClientCredential(CLIENT_ID, CLIENT_SECRET);
            var authResult = await context.AcquireTokenAsync(resource, clientCred);
            return authResult.AccessToken;
        }));

        public static async Task<X509Certificate2> GetCertificate()
        {
            try
            {
                var client = GetClient();

                //var Certificate2 = client.GetCertificateAsync(KEY_VAULT_IDENTIFIER, CERTIFICATE_NAME).GetAwaiter().GetResult();
                //Console.WriteLine(Certificate2.X509Thumbprint.ToHexString());

                var certificateIdentifierSecretPart =
                    "https://opflowvault.vault.azure.net/secrets/OpFlowWebCert/b9d8cda9055444378484c4b860a465cf";
                SecretBundle secret =
                    await client.GetSecretAsync(certificateIdentifierSecretPart);


                byte[] bytes;
                if (secret.ContentType == "application/x-pkcs12")
                    bytes = Convert.FromBase64String(secret.Value);
                else
                {
                    bytes = new byte[0];
                    Console.WriteLine("secret is not PFX!!");
                    throw new ArgumentException("This is not a PFX string!!");
                }
                var password = new SecureString();

                var coll = new X509Certificate2Collection();
                coll.Import(bytes, null, X509KeyStorageFlags.Exportable);
                var pfx = coll[0];
                // File output added in case I end up needing to write cert to container
                //          File.WriteAllBytes(Directory.GetCurrentDirectory().ToString() + "/Macs.pfx", bytes);
                Console.WriteLine(pfx.HasPrivateKey);
                Console.WriteLine(pfx.GetRSAPrivateKey());

                var rsa = pfx.GetRSAPrivateKey();
                return pfx;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}