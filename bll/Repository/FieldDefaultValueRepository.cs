using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Repository
{
	internal class FieldDefaultValueRepository
	{
		internal static IEnumerable<int> GetItemIdsToProcess(int contentId, int fieldId, string linkId, bool isBlob, bool isM2M)
		{
			using (var scope = new QPConnectionScope())
			{
				if (isM2M)
					return Common.ApplyFieldDefaultValue_GetM2MItemIdsToProcess(contentId, fieldId, linkId, scope.DbConnection);
				else
					return Common.ApplyFieldDefaultValue_GetItemIdsToProcess(contentId, fieldId, isBlob, scope.DbConnection);
			}
		}

		internal static void SetDefaultValue(int contentId, int fieldId, bool isBlob, bool isM2m, IEnumerable<int> idsForStep, bool isSymmetric)
		{						
			using (var scope = new QPConnectionScope())
			{
				if (isM2m)
				{
					string defVal = FieldRepository.GetById(fieldId).DefaultValue;
					Common.ApplyM2MFieldDefaultValue_SetDefaultValue(contentId, fieldId, defVal, idsForStep, isSymmetric, scope.DbConnection);
				}
				else
					Common.ApplyFieldDefaultValue_SetDefaultValue(contentId, fieldId, isBlob, isM2m, idsForStep, scope.DbConnection);
			}
		}
	}
}
