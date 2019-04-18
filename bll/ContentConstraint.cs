using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;

namespace Quantumart.QP8.BLL
{
    public class ContentConstraint : BizObject
    {
        public ContentConstraint()
        {
            Rules = new List<ContentConstraintRule>();
        }

        public int ContentId { get; set; }

        public int Id { get; set; }

        public IList<ContentConstraintRule> Rules { get; set; }

        /// <summary>
        /// Признак, сохранена ли сущность в БД
        /// </summary>
        public bool IsNew => Id == 0;

        public bool IsComplex => Rules.Count > 1;

        /// <summary>
        /// Фильтрация пар поле-значение, релевантных ограничению целостности
        /// </summary>
        /// <param name="fieldValues">список пар поле-значение для статьи</param>
        /// <returns>отфильтрованный список пар</returns>
        public List<FieldValue> Filter(List<FieldValue> fieldValues)
        {
            var fieldIds = Rules.AsEnumerable().Select(n => n.FieldId).ToList();
            return fieldValues.AsEnumerable().Where(n => fieldIds.Contains(n.Field.Id)).ToList();
        }

        public int CountDuplicates(int id) => ArticleRepository.CountDuplicates(this, null, id, false);
    }
}
