using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.ViewModels;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class CssController : AuthQpController
    {
        private readonly IStatusTypeService _statusTypeService;

        public CssController(IStatusTypeService statusTypeService)
        {
            _statusTypeService = statusTypeService;
        }

        public async Task<ActionResult> CustomCss()
        {
            var viewModel = new CustomCssViewModel(_statusTypeService.GetColouredStatuses());
            string content = await RenderPartialView("CustomCss", viewModel);
            return Content(content, "text/css");
        }
    }
}
