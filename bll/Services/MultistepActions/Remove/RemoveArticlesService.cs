using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.MultistepActions.Base;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Remove
{
    public class RemoveArticlesService : MultistepActionServiceBase<RemoveArticlesCommand>
    {
        public override MessageResult PreAction(int parentId, int id, int[] ids) => ArticleService.MultistepRemovePreAction(parentId, ids);

        public override string ActionCode => Constants.ActionCode.MultipleRemoveArticle;
    }
}
