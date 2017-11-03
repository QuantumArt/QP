using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.MultistepActions.Base;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Remove
{
    public class RemoveArticlesFromArchiveCommand : MultistepActionStageCommandBase
    {
        protected override MessageResult Step(int[] ids) => ArticleService.MultistepRemove(ContentId, ids, true, BoundToExternal);
    }
}
