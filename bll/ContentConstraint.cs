using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository.Articles;

namespace Quantumart.QP8.BLL
{
    public class ContentConstraint : BizObject
    {

		public ContentConstraint()
		{
			Rules = new List<ContentConstraintRule>();
		}

		public int ContentId
        {
            get;
            set;
        }

        public int Id
        {
            get;
            set;
        }

        public IList<ContentConstraintRule> Rules
        {
            get;
            set;
        }

		/// <summary>
		/// Признак, сохранена ли сущность в БД
		/// </summary>
		public bool IsNew
		{
			get
			{
				return (Id == 0);
			}
		}

        public bool IsComplex
        {
            get
            {
                return Rules.Count() > 1;
            }
        }

        /// <summary>
        /// Фильтрация пар поле-значение, релевантных ограничению целостности
        /// </summary>
        /// <param name="fieldValues">список пар поле-значение для статьи</param>
        /// <returns>отфильтрованный список пар</returns>
        public List<FieldValue> Filter(List<FieldValue> fieldValues)
        {
            List<Int32> fieldIds = Rules.AsEnumerable().Select(n => n.FieldId).ToList();
            List<FieldValue> fieldValuesToTest = fieldValues.AsEnumerable()
				.Where(n => fieldIds.Contains(n.Field.Id))
				.ToList();
            return fieldValuesToTest;
        }

        public int CountDuplicates(int id)
        {
            return ArticleRepository.CountDuplicates(this, null, id);
        }
    }
}
