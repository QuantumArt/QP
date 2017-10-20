using System.Data;
using System.Linq;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository
{
	internal class ClearContentRepository
	{
		/// <summary>
		/// Возвращает количество статей в контенте
		/// </summary>
		/// <param name="contentId"></param>
		/// <returns></returns>
		internal static DataRow GetContentItemsInfo(int contentId)
		{
			using (var scope = new QPConnectionScope())
			{
				var data = Common.RemovingActions_GetContentsItemInfo(null, contentId, scope.DbConnection);
				return data.SingleOrDefault();
			}
		}

		internal static int RemoveContentItems(int contentId, int itemsToDelete)
		{
			using (var scope = new QPConnectionScope())
			{
				return Common.RemovingActions_RemoveContentItems(contentId, itemsToDelete, scope.DbConnection);
			}
		}

		internal static int ClearO2MRelations(int contentId)
		{
			using (var scope = new QPConnectionScope())
			{
				return Common.RemovingActions_ClearO2MRelations(contentId, scope.DbConnection);
			}
		}
	}
}
