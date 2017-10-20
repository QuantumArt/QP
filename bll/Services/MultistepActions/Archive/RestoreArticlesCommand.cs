using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.MultistepActions.Base;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Archive
{
	public class RestoreArticlesCommand : MultistepActionStageCommandBase
	{
		protected override MessageResult Step(int[] ids) => ArticleService.MultistepRestoreFromArchive(ContentId, ids, BoundToExternal);
	}
}
