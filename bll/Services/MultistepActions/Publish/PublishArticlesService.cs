#if !NET_STANDARD
using Quantumart.QP8.BLL.Services.MultistepActions.Base;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Publish
{
    public class PublishArticlesService : MultistepActionServiceBase<PublishArticlesCommand>
    {
        public override string ActionCode => Constants.ActionCode.MultiplePublishArticles;
    }
}
#endif
