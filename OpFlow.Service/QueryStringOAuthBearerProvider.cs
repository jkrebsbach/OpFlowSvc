using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth;

//https://stackoverflow.com/questions/26657296/signalr-authentication-with-webapi-bearer-token

namespace OpFlow.Service
{
    public class QueryStringOAuthBearerProvider : OAuthBearerAuthenticationProvider
    {
        public override Task RequestToken(OAuthRequestTokenContext context)
        {
            var value = context.Request.Query.Get("access_token");

            if (!string.IsNullOrEmpty(value))
            {
                context.Token = value;
            }

            context.Request.Headers.Add("Authorization", new [] { $"Bearer {context.Token}" });

            return Task.FromResult<object>(null);
        }
    }
}