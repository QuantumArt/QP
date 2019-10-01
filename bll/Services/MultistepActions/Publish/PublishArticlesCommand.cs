using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.MultistepActions.Base;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Publish
{
    public class PublishArticlesCommand : MultistepActionStageCommandBase
    {
        protected override MessageResult Step(int[] ids) => ArticleService.MultistepPublish(ContentId, ids, BoundToExternal);
    }
}
