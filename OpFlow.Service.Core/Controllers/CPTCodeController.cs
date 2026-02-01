using System.Text;
using Microsoft.AspNetCore.Mvc;
using OpFlow.Data.Core;

namespace OpFlow.Service.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class CptController : ControllerBase
    {
        private readonly ICptUploadService _uploadService;

        public CptController(ICptUploadService uploadService)
        {
            _uploadService = uploadService;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(200_000_000)] // adjust if needed
        public async Task<ActionResult<CptUploadResultDto>> Upload([FromForm] CptUploadForm form, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var result = await _uploadService.ParseFilesAsync(form, ct);

            return Ok(result);
        }
    }
}
