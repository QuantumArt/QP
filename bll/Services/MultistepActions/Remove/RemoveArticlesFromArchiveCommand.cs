using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.BLL.Services.DbServices;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.MultistepActions.Base;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Remove
{
    public class RemoveArticlesFromArchiveCommand : MultistepActionStageCommandBase
    {
        protected override MessageResult Step(int[] ids)
        {
            var service = new ArticleService(new PathHelper(new DbService(S3Options)));
            return service.MultistepRemove(ContentId, ids, true, BoundToExternal);
        }
    }
}
