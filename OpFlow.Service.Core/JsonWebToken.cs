using Microsoft.IdentityModel.Tokens;
using OpFlow.Service.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

namespace OpFlow.Service
{
    public class JsonWebToken
    {
        public const string key = "OpFlowJwtGeneratedSuperRefrigeratorSecretKeySecurity";

        public static string GenerateToken(ApplicationUser user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim("uid", user.Id.ToString())
            };

            string jwtToken = JsonWebToken.GetToken(claims);

            return jwtToken;
        }
        public static string GetToken(List<Claim> claims)
        {
            // Create Security key  using private key above:
            // not that latest version of JWT using Microsoft namespace instead of System
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            // Also note that securityKey length should be >256b
            // so you have to make sure that your private key has a proper length
            //
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            //  Finally create a Token
            JwtHeader header = new JwtHeader(credentials);
            JwtPayload jwtPayload = GetPayload(claims);

            JwtSecurityToken secToken = new JwtSecurityToken(header, jwtPayload);

            // Token to String so you can use it in your client
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(secToken);

            //List<Claim> claims = handler.
        }

        private static JwtPayload GetPayload(List<Claim> claims)
        {
            string issuer = "Issuer";
            string audience = "Audience";
            DateTime? notBefore = DateTime.Now;
            DateTime? expires = DateTime.Now.AddMinutes(12 * 60);
#if DEBUG
            expires = DateTime.Now.AddMinutes(480);
#endif
            DateTime? issuedAt = DateTime.Now;

            JwtPayload jwtPayload = new JwtPayload(issuer, audience, claims, notBefore, expires, issuedAt);

            return jwtPayload;
        }
    }

    public class AuthToken
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonPropertyName("userName")]
        public string UserName { get; set; }
        [JsonPropertyName(".issued")]
        public string Issued { get; set; }
        [JsonPropertyName(".expires")]
        public string Expires { get; set; }

        [JsonIgnore]
        public DateTime IssuedDate => Issued == null ? DateTime.MinValue : DateTime.Parse(Issued);

        [JsonIgnore]
        public DateTime ExpiresDate => Expires == null ? DateTime.MinValue : DateTime.Parse(Expires);
    }
}
