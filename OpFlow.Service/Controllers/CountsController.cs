using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using OpFlow.Data;
using OpFlow.Service;
using OpFlow.Service.DataAccess;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [RoutePrefix("api/counts")]
    public class CountsController : ApiController
    {
        // POST api/counts/start
        [SwaggerOperation("StartCounts")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("start", Name = "StartCounts")]
        [HttpPost]
        public async Task<IHttpActionResult> StartCounts([FromBody] StartCountsPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var processId = Guid.NewGuid();
            var occurredAtUtc = DateTime.UtcNow;

            await sqlHelper.InsertCountsEvent(
                processId: processId,
                statusCode: (int)CountsStatusCode.Began,
                statusName: CountsStatusCode.Began.ToString(),
                occurredAtUtc: occurredAtUtc,
                clientOccurredAtUtc: post?.ClientOccurredAtUtc,
                sourceSystem: string.IsNullOrWhiteSpace(post?.SourceSystem) ? "swift" : post.SourceSystem,
                correlationId: post?.CorrelationId,
                details: post?.Details,
                errorMessage: null,
                errorCode: null,
                errorStackTrace: null,
                locationId: user.SelectedLocation
            );

            return Content(HttpStatusCode.Created, new
            {
                ProcessId = processId,
                OccurredAtUtc = occurredAtUtc
            });
        }

        // PUT api/counts/status
        [SwaggerOperation("LogCountsStatus")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("status", Name = "LogCountsStatus")]
        [HttpPut]
        public async Task<IHttpActionResult> LogCountsStatus([FromBody] CountsStatusPost post)
        {
            if (post == null) return BadRequest("Body is required.");
            if (post.ProcessId == Guid.Empty) return BadRequest("ProcessId is required.");
            if (post.StatusCode <= 0) return BadRequest("StatusCode is required.");

            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var statusName = GetStatusName(post.StatusCode);
            var occurredAtUtc = DateTime.UtcNow;

            // If Error, require an ErrorMessage (you can relax this if you want)
            if (post.StatusCode == (int)CountsStatusCode.Error && string.IsNullOrWhiteSpace(post.ErrorMessage))
                return BadRequest("ErrorMessage is required when StatusCode = Error.");

            await sqlHelper.InsertCountsEvent(
                processId: post.ProcessId,
                statusCode: post.StatusCode,
                statusName: statusName,
                occurredAtUtc: occurredAtUtc,
                clientOccurredAtUtc: post.ClientOccurredAtUtc,
                sourceSystem: string.IsNullOrWhiteSpace(post.SourceSystem) ? "swift" : post.SourceSystem,
                correlationId: post.CorrelationId,
                details: post.Details,
                errorMessage: post.StatusCode == (int)CountsStatusCode.Error ? post.ErrorMessage : null,
                errorCode: post.StatusCode == (int)CountsStatusCode.Error ? post.ErrorCode : null,
                errorStackTrace: post.StatusCode == (int)CountsStatusCode.Error ? post.ErrorStackTrace : null,
                locationId: user.SelectedLocation
            );

            return Ok();
        }

        // POST api/counts/error (convenience)
        [SwaggerOperation("LogCountsError")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("error", Name = "LogCountsError")]
        [HttpPost]
        public async Task<IHttpActionResult> LogCountsError([FromBody] CountsStatusPost post)
        {
            if (post == null) return BadRequest("Body is required.");
            if (post.ProcessId == Guid.Empty) return BadRequest("ProcessId is required.");

            post.StatusCode = (int)CountsStatusCode.Error;
            return await LogCountsStatus(post);
        }

        // GET api/counts/{processId}/events (debug timeline)
        [SwaggerOperation("GetCountsEvents")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("{processId:guid}/events", Name = "GetCountsEvents")]
        [HttpGet]
        public async Task<IHttpActionResult> GetCountsEvents(Guid processId)
        {
            if (processId == Guid.Empty) return BadRequest("ProcessId is required.");

            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            // NOTE: implement this in SqlHelper (or remove this endpoint for now)
            var events = await sqlHelper.GetCountsEvents(processId, user.SelectedLocation);

            return Ok(events);
        }

        private static string GetStatusName(int statusCode)
        {
            if (Enum.IsDefined(typeof(CountsStatusCode), statusCode))
                return ((CountsStatusCode)statusCode).ToString();

            return "Unknown";
        }
    }
}
