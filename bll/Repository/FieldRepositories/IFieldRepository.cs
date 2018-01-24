using System.Collections.Generic;
using Quantumart.QP8.BLL.Services.VisualEditor;

namespace Quantumart.QP8.BLL.Repository.FieldRepositories
{
    public interface IFieldRepository
    {
        Field GetById(int fieldId);

        Field GetByName(int contentId, string fieldName);

        IList<Field> GetByNames(int contentId, IList<string> fieldNames);

        Field GetByOrder(int contentId, int order);

        /// <summary>
        /// Возвращает все поля которые ссылаются на данное поле связью O2M (включая виртуальные)
        /// </summary>
        IEnumerable<Field> GetRelatedO2MFields(int fieldId);

        Field GetByBackRelationId(int fieldId);

        IEnumerable<Field> GetChildList(int fieldId);

        bool HasAnyAggregators(int fieldId);

        VisualEditFieldParams GetVisualEditFieldParams(int fieldId);

        IEnumerable<Article> GetActiveArticlesForM2MField(int fieldId);

        int GetTextFieldMaxLength(Field dbField);

        /// <summary>
        /// Определяет, существуют ли значения поля в БД, не входящие в перечисление
        /// </summary>
        bool IsNonEnumFieldValueExist(Field field);

        int? GetNumericFieldMaxValue(Field dbField);

        bool CheckNumericValuesAsKey(Field field, Field dbField);

        /// <summary>
        /// Существуют ли множественные связи по данному полю.
        /// </summary>
        bool DoPluralLinksExist(Field dbField);

        bool LinkNetNameExists(ContentLink link);

        bool NetNameExists(ContentLink link);

        bool NetPluralNameExists(ContentLink link);

        bool LinqBackPropertyNameExists(Field field);

        bool LinqPropertyNameExists(Field field);

        void RemoveLinkVersions(int fieldId);

        void Delete(int id);

        void ClearTreeOrder(int id);

        Field CreateNew(Field item, bool explicitOrder);

        Field Update(Field item);

        void SetFieldM2MDefValue(int fieldId, int[] defaultArticles);

        /// <summary>
        /// Обновляет значения поля StringEnum в существующих статьях, в соответствии со составом перечисления
        /// </summary>
        void CorrectEnumInContentData(Field field);

        Field GetBaseField(int fieldId, int articleId);

        int GetBaseFieldId(int fieldId, int articleId);

        List<Field> GetDynamicImageFields(int contentId, int imageFieldId);

        void UpdateContentFieldsOrders(int contentId, int currentOrder, int newOrder);

        /// <summary>
        /// Возвращает список контентов для классификатора
        /// </summary>
        IEnumerable<Content> GetAggregatableContentsForClassifier(Field classifier);

        bool FieldHasArticles(int contentId);

        bool NameExists(Field field);
    }
}
