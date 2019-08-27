using Microsoft.AspNetCore.Authorization;
using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.WebMvc.Extensions.Controllers
{
    [Authorize]
    public class AuthQpController : QPController
    {

        protected AuthQpController()
        {
        }

        protected AuthQpController(IArticleService dbArticleService, QPublishingOptions options)
            : base(dbArticleService, options)
        {
        }
    }
}
