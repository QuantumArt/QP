using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.MultistepActions.Base;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Remove
{
	public class RemoveArticlesCommand : MultistepActionStageCommandBase
	{
		protected override MessageResult Step(int[] ids)
		{
			return ArticleService.MultistepRemove(ContentId, ids, false, BoundToExternal);
		}
	}
}