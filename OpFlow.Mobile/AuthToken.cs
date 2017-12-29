using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace OpFlow.Mobile
{
    public class AuthToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("userName")]
        public string UserName { get; set; }
        [JsonProperty(".issued")]
        public string Issued { get; set; }
        [JsonProperty(".expires")]
        public string Expires { get; set; }

        public DateTime? IssuedDate => Issued == null ? null : (DateTime?)DateTime.Parse(Issued);

        public DateTime? ExpiresDate => Expires == null ? null : (DateTime?)DateTime.Parse(Expires);
    }
}
