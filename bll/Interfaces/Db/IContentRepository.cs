using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Interfaces.Db
{
    public interface IContentRepository
    {
        /// <summary>
        /// Возвращает контент по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор контента</param>
        /// <returns>Информация о контенте</returns>
        Content GetById(int id);

        ContentLink GetContentLinkById(int linkId);

        /// <summary>
        /// Есть ли статьи у контента
        /// </summary>
        /// <param name="contentId">Идентификатор контента</param>
        bool IsAnyArticle(int contentId);

        string GetTreeFieldName(int contentId, int exceptId);

        IEnumerable<int> GetDisplayFieldIds(int contentId, bool withRelations, int excludeId);

        /// <summary>
        /// Переключает RelationId равный currentRelationFieldId на значение newRelationFieldId
        /// </summary>
        void ChangeRelationIdToNewOne(int currentRelationFieldId, int newRelationFieldId);

        ContentLink SaveLink(ContentLink link);

        ContentLink UpdateLink(ContentLink link);
    }
}
