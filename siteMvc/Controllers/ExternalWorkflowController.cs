using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Services.ExternalWorkflow;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;

namespace Quantumart.QP8.WebMvc.Controllers
{
    [Route("[controller]")]
    [ExternalWorkflowCustomAction]
    public class ExternalWorkflowController : QPController
    {
        private readonly IExternalWorkflowService _externalWorkflowService;

        public ExternalWorkflowController(IExternalWorkflowService externalWorkflowService)
        {
            _externalWorkflowService = externalWorkflowService;
        }

        [HttpPost("publishWorkflow")]
        public async Task<IActionResult> PublishWorkflow([FromForm]string customerCode,
            [FromForm(Name = "content_item_id")]int contentItemId,
            [FromForm]int siteId,
            CancellationToken token)
        {
            bool result = await _externalWorkflowService.PublishWorkflow(customerCode, contentItemId, siteId, token);

            return result ? Ok() : new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        [HttpPost("startWorkflow")]
        public async Task<IActionResult> StartWorkflow([FromForm]string customerCode,
            [FromForm(Name = "content_item_id")]int contentItemId,
            [FromForm(Name = "content_id")]int contentId,
            CancellationToken token)
        {
            bool result = await _externalWorkflowService.StartProcess(customerCode,
                contentItemId,
                contentId,
                token);

            return result ? Ok() : new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
