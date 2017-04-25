using Quantumart.QP8.BLL.Services.MultistepActions.Base;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Archive
{
	public class RestoreArticlesService : MultistepActionServiceBase<RestoreArticlesCommand>
	{
		public override string ActionCode
		{
			get { return Constants.ActionCode.MultipleRestoreArticleFromArchive; }
		}
	}
}