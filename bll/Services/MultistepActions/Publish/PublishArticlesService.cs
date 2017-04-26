using Quantumart.QP8.BLL.Services.MultistepActions.Base;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Publish
{
	public class PublishArticlesService : MultistepActionServiceBase<PublishArticlesCommand>
	{	
		public override string ActionCode
		{
			get { return Constants.ActionCode.MultiplePublishArticles; }
		}
	}
}