using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Services.ExternalWorkflow;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    [Route("[controller]")]
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

            if (result)
            {
                return Ok();
            }

            return BadRequest();
        }
    }
}
