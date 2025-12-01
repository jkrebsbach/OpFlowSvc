using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace OpFlow.Service.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/lookup")]
    public class LookupController : OpFlowController
    {
        public LookupController(
            IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
        }

        [Route("states")]
        public async Task<List<KeyValuePair<string, string>>> GetStates()
        {
            var user = await GetUserSecurity();
        
            var result = new List<KeyValuePair<string, string>>();

            result.Add(new KeyValuePair<string, string>("AL", "Alabama"));
            result.Add(new KeyValuePair<string, string>("AK", "Alaska"));
            result.Add(new KeyValuePair<string, string>("AZ", "Arizona"));
            result.Add(new KeyValuePair<string, string>("AR", "Arkansas"));
            result.Add(new KeyValuePair<string, string>("CA", "California"));
            result.Add(new KeyValuePair<string, string>("CO", "Colorado"));
            result.Add(new KeyValuePair<string, string>("CT", "Connecticut"));
            result.Add(new KeyValuePair<string, string>("DE", "Delaware"));
            result.Add(new KeyValuePair<string, string>("FL", "Florida"));
            result.Add(new KeyValuePair<string, string>("GA", "Georgia"));
            result.Add(new KeyValuePair<string, string>("HI", "Hawaii"));
            result.Add(new KeyValuePair<string, string>("ID", "Idaho"));
            result.Add(new KeyValuePair<string, string>("IL", "Illinois"));
            result.Add(new KeyValuePair<string, string>("IN", "Indiana"));
            result.Add(new KeyValuePair<string, string>("IA", "Iowa"));
            result.Add(new KeyValuePair<string, string>("KS", "Kansas"));
            result.Add(new KeyValuePair<string, string>("KY", "Kentucky"));
            result.Add(new KeyValuePair<string, string>("LA", "Louisiana"));
            result.Add(new KeyValuePair<string, string>("ME", "Maine"));
            result.Add(new KeyValuePair<string, string>("MD", "Maryland"));
            result.Add(new KeyValuePair<string, string>("MA", "Massachusetts"));
            result.Add(new KeyValuePair<string, string>("MI", "Michigan"));
            result.Add(new KeyValuePair<string, string>("MN", "Minnesota"));
            result.Add(new KeyValuePair<string, string>("MS", "Mississippi"));
            result.Add(new KeyValuePair<string, string>("MO", "Missouri"));
            result.Add(new KeyValuePair<string, string>("MT", "Montana"));
            result.Add(new KeyValuePair<string, string>("NE", "Nebraska"));
            result.Add(new KeyValuePair<string, string>("NV", "Nevada"));
            result.Add(new KeyValuePair<string, string>("NH", "New Hampshire"));
            result.Add(new KeyValuePair<string, string>("NJ", "New Jersey"));
            result.Add(new KeyValuePair<string, string>("NM", "New Mexico"));
            result.Add(new KeyValuePair<string, string>("NY", "New York"));
            result.Add(new KeyValuePair<string, string>("NC", "North Carolina"));
            result.Add(new KeyValuePair<string, string>("ND", "North Dakota"));
            result.Add(new KeyValuePair<string, string>("OH", "OhiO"));
            result.Add(new KeyValuePair<string, string>("OK", "Oklahoma"));
            result.Add(new KeyValuePair<string, string>("OR", "Oregon"));
            result.Add(new KeyValuePair<string, string>("PA", "Pennsylvania"));
            result.Add(new KeyValuePair<string, string>("RI", "Rhode Island"));
            result.Add(new KeyValuePair<string, string>("SC", "South Carolina"));
            result.Add(new KeyValuePair<string, string>("SD", "South Dakota"));
            result.Add(new KeyValuePair<string, string>("TN", "Tennessee"));
            result.Add(new KeyValuePair<string, string>("TX", "Texas"));
            result.Add(new KeyValuePair<string, string>("UT", "Utah"));
            result.Add(new KeyValuePair<string, string>("VT", "Vermont"));
            result.Add(new KeyValuePair<string, string>("VA", "Virginia"));
            result.Add(new KeyValuePair<string, string>("WA", "Washington"));
            result.Add(new KeyValuePair<string, string>("WV", "West Virginia"));
            result.Add(new KeyValuePair<string, string>("WI", "Wisconsin"));
            result.Add(new KeyValuePair<string, string>("WY", "Wyoming"));

            return result;
        }
    }
}