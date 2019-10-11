using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.MultistepActions.Base;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Archive
{
    public class ArchiveArticlesService : MultistepActionServiceBase<ArchiveArticlesCommand>
    {
        public override MessageResult PreAction(int parentId, int id, int[] ids) => ArticleService.MultistepMoveToArchivePreAction(ids);

        public override string ActionCode => Constants.ActionCode.MultipleMoveArticleToArchive;
    }
}
