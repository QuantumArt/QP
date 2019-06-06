using System.Collections.Generic;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository
{
    internal class FieldDefaultValueRepository
    {
        internal static IEnumerable<int> GetItemIdsToProcess(int contentId, int fieldId, string linkId, bool isBlob, bool isM2M)
        {
            using (var scope = new QPConnectionScope())
            {
                return isM2M
                    ? Common.ApplyFieldDefaultValue_GetM2MItemIdsToProcess(contentId, fieldId, int.Parse(linkId), scope.DbConnection)
                    : Common.ApplyFieldDefaultValue_GetItemIdsToProcess(contentId, fieldId, isBlob, scope.DbConnection);
            }
        }

        internal static void SetDefaultValue(int contentId, int fieldId, bool isBlob, bool ism2m, List<int> idsForStep, bool isSymmetric)
        {
            using (var scope = new QPConnectionScope())
            {
                if (ism2m)
                {
                    var linkId = int.Parse(FieldRepository.GetById(fieldId).DefaultValue);
                    Common.ApplyM2MFieldDefaultValue_SetDefaultValue(contentId, fieldId, linkId, idsForStep, isSymmetric, scope.DbConnection);
                }
                else
                {
                    Common.ApplyFieldDefaultValue_SetDefaultValue(contentId, fieldId, isBlob, false, idsForStep, scope.DbConnection);
                }
            }
        }
    }
}
