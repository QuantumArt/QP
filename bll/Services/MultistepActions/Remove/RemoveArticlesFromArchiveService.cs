using Quantumart.QP8.BLL.Services.MultistepActions.Base;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Remove
{
    public class RemoveArticlesFromArchiveService : MultistepActionServiceBase<RemoveArticlesFromArchiveCommand>
    {
        public override string ActionCode => Constants.ActionCode.MultipleRemoveArticleFromArchive;
    }
}
