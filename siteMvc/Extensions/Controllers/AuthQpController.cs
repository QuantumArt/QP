using Microsoft.AspNetCore.Authorization;
using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.WebMvc.Extensions.Controllers
{
    [Authorize(Policy = "CustomerCodeSelected")]
    public class AuthQpController : QPController
    {

        protected AuthQpController()
        {
        }

        protected AuthQpController(IArticleService articleService, QPublishingOptions options)
            : base(articleService, options)
        {
        }
    }
}
