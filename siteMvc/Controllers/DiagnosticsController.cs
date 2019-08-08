using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class DiagnosticsController : QPController
    {
        public async Task<ActionResult> Index()
        {
            return await JsonHtml("Index", new object());
        }
    }
}
