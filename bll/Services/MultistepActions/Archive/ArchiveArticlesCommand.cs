using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.MultistepActions.Base;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Archive
{
	public class ArchiveArticlesCommand : MultistepActionStageCommandBase
	{
		protected override MessageResult Step(int[] ids)
		{
			return ArticleService.MultistepMoveToArchive(ContentId, ids, BoundToExternal);
		}
	}
}