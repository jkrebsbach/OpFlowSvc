using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using OpFlow.Service.SignalR;
using Owin;

[assembly: OwinStartup(typeof(OpFlow.Service.Startup))]

namespace OpFlow.Service
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            var config = new HubConfiguration();
            config.EnableJSONP = true;

            app.Map("/signalr", map =>
            {
                map.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions()
                {
                    Provider = new QueryStringOAuthBearerProvider()
                });

                var hubConfiguration = new HubConfiguration()
                {
                    Resolver = GlobalHost.DependencyResolver,
                    EnableDetailedErrors = true
                };

                map.RunSignalR(hubConfiguration);
            });
        }
    }
}