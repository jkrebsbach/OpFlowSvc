using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpFlow.Data
{
    public class ApiKey
    {
        public Guid UserAuthID { get; set; }
        public string ProviderKey { get; set; }
        public string LoginProvider { get; set; }
        public DateTime? ExpirationDate { get; set; }

        public static ApiKey Generate(Guid userAuthId)
        {
            var guidBytes = new List<Byte>();
            for (var index = 0; index < 5; index++)
            {
                var providerKey = Guid.NewGuid();
                guidBytes.AddRange(providerKey.ToByteArray());
            }
            
            var guidString = System.Convert.ToBase64String(guidBytes.ToArray());

            var result = new ApiKey()
            {
                LoginProvider = "MOBILE",
                UserAuthID = userAuthId,
                ProviderKey = guidString
            };

            return result;
        }
    }
}
