using OpFlow.Service.DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OpFlow.Service.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/trayscheduling")]
    public class TraySchedulingController : OpFlowController
    {
        private EmailHelper _emailHelper;

        public TraySchedulingController(EmailHelper emailHelper,
            IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
            _emailHelper = emailHelper;
        }

        /// <summary>
        /// Return search result for receiving provider
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("Nightly", Name = "NightlyRefresh")]
        [HttpPost]
        public async Task<ActionResult> NightlyRefresh()
        {
            var authorized = AuthenticateUser();
            if (!authorized)
            {
                return Unauthorized();
            }

            await UnscheduledCases();

            return Ok("DONE");
        }

        private async Task UnscheduledCases()
        {
            
            var proposals = await _sqlHelper.GetProposedTrayAlert();

            foreach (var proposal in proposals)
            {
                var message = $"Proposal for {proposal.TrayName} due by {proposal.TrayChangesTarget.Value.ToShortDateString()} has not been accepted";

                await _emailHelper.SendEmail(proposal.ProposalUsers.ToList<Data.User>(), "Proposal not accepted", message);

                await _sqlHelper.InsertProposedTrayCommunication(
                    null, proposal.TrayProposalID, message, proposal.ProviderID, proposal.LocationID);
            }
        }

        private bool AuthenticateUser()
        {
            var authorization = _httpContext.Request.Headers["Authorization"].First();

            if (authorization == null || !authorization.StartsWith("Basic ")) return false;

            try
            {
                var token = authorization.Substring("Basic ".Length).Trim();
                var result = Encoding.UTF8.GetString(Convert.FromBase64String(token));

                var pieces = result.Split(':');
                if (pieces[0] == "scheduler@opflow.com")
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }
    }
}
