using System.Collections.Generic;
using System.Data;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository
{
	public static class RecreateDynamicImagesRepository
	{
		/// <summary>
		/// Возвращает список id статей и значение базового Image-поля для последующей обработки
		/// </summary>
		/// <param name="contentId"></param>
		/// <param name="imageFieldId"></param>
		/// <returns></returns>
		internal static IEnumerable<DataRow> GetDataToProcess(int imageFieldId)
		{
			using (var scope = new QPConnectionScope())
			{
				return Common.RecreateDynamicImages_GetDataToProcess(imageFieldId, scope.DbConnection);
			}
		}


		internal static void UpdateDynamicFieldValue(int dynamicFieldId, int articleId, string newValue)
		{
			using (var scope = new QPConnectionScope())
			{
				Common.RecreateDynamicImages_UpdateDynamicFieldValue(dynamicFieldId, articleId, newValue, scope.DbConnection);
			}
		}
	}
}
