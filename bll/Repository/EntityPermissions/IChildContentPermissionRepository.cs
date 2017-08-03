using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Repository.EntityPermissions
{
    internal interface IChildContentPermissionRepository : IChildEntityPermissionRepository
    {
        /// <summary>
        /// Фильтрует id связанных контентов оставляя только те для которых нет permission уровня равного или больше чем чтение
        /// </summary>
        IEnumerable<int> FilterNoPermissionContent(IEnumerable<int> relatedContentId, int? userId, int? groupId);

        /// <summary>
        /// Возвращает контенты
        /// </summary>
        IEnumerable<Content> GetContentList(IEnumerable<int> contentIDs);

        /// <summary>
        /// Создает permisions ордновременно для множества контентов
        /// </summary>
        void MultipleSetPermission(IEnumerable<int> contentIDs, int? userId, int? groupId, int permissionLevel);
    }
}