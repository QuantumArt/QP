using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Constants;
using System.Text.RegularExpressions;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL
{

    /// <summary>
    /// Представляет значение поля пользовательской таблицы (контента) со всей информацией, необходимой для его вывода и обработки
    /// </summary>
    public class FieldValue
    {
        /// <summary>
        /// Ссылка на поле
        /// </summary>
        public Field Field
        {
            get;
            set;
        }

        /// <summary>
        /// Значение
        /// </summary>
        public string Value
        {
            get { return (_objectValue == null) ? String.Empty : _objectValue.ToString(); }
			set { _objectValue = value; }
        }

		private object _objectValue;

		public object ObjectValue
		{
			get { return _objectValue;  }
			set { _objectValue = value; }
		}

        /// <summary>
        /// Значение для слияния (используется для слияния на этапе рендеринга)
        /// </summary>
        public string ValueToMerge
        {
            get;
            set;
        }
        
        /// <summary>
        /// Ссылка на статью-контейнер
        /// </summary>
        public Article Article
        {
            get;
            set;
        }

        /// <summary>
        /// Ссылка на версию (необходима для просмотра/сравнения версия, в обычном режиме - пустая)
        /// </summary>
        public ArticleVersion Version
        {
            get;
            set;
        }

        public int[] RelatedItems
        {
            get
            {
                return Converter.ToIdArray(Value);
            }
        }

        public int[] CurrentRelatedItems
        {
            get
            {
                return Converter.ToIdArray(ArticleRepository.GetRelatedItems(Field.BackRelationId.Value, Article.Id));
            }
        }

        public int[] NewUnrelatedItems
        {
            get; 
            set; 
        }

        public int[] NewRelatedItems
        {
            get;
            set;
        }

        /// <summary>
        /// Валидирует значение поля (используется при редактировании статьи)
        /// </summary>
        /// <param name="errors">коллекция ошибок валидации</param>
        public void Validate(RulesException<Article> errors)
        {
            if (Field.Required && String.IsNullOrEmpty(Value))
            {
                errors.Error(Field.FormName, Value, String.Format(ArticleStrings.MissingValue, Field.DisplayName));
            }

            if (Field.ExactType == FieldExactTypes.String)
            {
                if (!String.IsNullOrEmpty(Value) && Value.Length > Field.StringSize)
				{
					errors.Error(Field.FormName, Value, String.Format(ArticleStrings.TooLongValue, Field.DisplayName, Field.StringSize));
				}
				else if (!String.IsNullOrEmpty(Field.InputMask) && !Regex.IsMatch(Value, Field.InputMask))
				{
					errors.Error(Field.FormName, Value, String.Format(ArticleStrings.InputMaskNotMatch, Field.DisplayName));					
				}
            }
			else if (Field.IsDateTime && !String.IsNullOrEmpty(Value))
			{
				DateTime dt;
				if(!DateTime.TryParse(Value, out dt))
					errors.Error(Field.FormName, Value, String.Format(ArticleStrings.InvalidDateTimeFormat, Field.DisplayName));
			}
			else if (Field.ExactType == FieldExactTypes.Classifier && !String.IsNullOrEmpty(Value)) 
			{
				bool isCorrectClassifierValue = FieldRepository.GetAggregatableContentListItemsForClassifier(Field)
					.Where(i => StringComparer.CurrentCultureIgnoreCase.Equals(i.Value, Value))
					.Any();
				if(!isCorrectClassifierValue)
					errors.Error(Field.FormName, Value, ArticleStrings.InvalidClassiferValue);
			}
			else if (Field.ExactType == FieldExactTypes.O2MRelation && Field.RelateToContentId == Field.ContentId && Value == Article.Id.ToString())
			{
				errors.Error(Field.FormName, Value, ArticleStrings.O2MRelateToCurrentArticle);
			}
			else if (Field.ExactType == FieldExactTypes.M2ORelation)
			{
				HashSet<int> ids = new HashSet<int>(RelatedItems);
				if (ids.Contains(Article.Id))
				{
					errors.Error(Field.FormName, "true", ArticleStrings.O2MRelateToCurrentArticle);
				}

				if (Field.BackRelation.UseRelationCondition && ids.Any())
				{
					if (!Article.CheckRelationCondition(Field.BackRelation.RelationCondition))
						errors.Error(Field.FormName, "true", ArticleStrings.CurrentArticleViolatesRelationCondition);
				}

				int[] newUnrelatedItemsWithoutArchive = ArticleRepository.ExcludeArchived(NewUnrelatedItems.ToArray());


				if (Field.BackRelation.Required && newUnrelatedItemsWithoutArchive.Any())
				{
					errors.Error(Field.FormName, "true", ArticleStrings.BaseRelationFieldRequired);
				}

				if (Field.BackRelation.IsUnique)
				{
					int[] unRelatedItems = Converter.ToIdArray(ArticleRepository.GetRelatedItems(Field.BackRelationId.Value, null));
					int[] unRelatedItemsAfterUpdate = unRelatedItems.Except(NewRelatedItems).Concat(newUnrelatedItemsWithoutArchive).ToArray();
					if (!Field.BackRelation.Constraint.IsComplex)
					{
						if (unRelatedItemsAfterUpdate.Length > 1)
							errors.Error(Field.FormName, "true", String.Format(ArticleStrings.BaseRelationUniqueViolation, unRelatedItemsAfterUpdate.Length));

						if (ids.Count() > 1)
							errors.Error(Field.FormName, "true", String.Format(ArticleStrings.BaseRelationUniqueViolation, ids.Count()));
					}
					else
					{
						int count = ((!newUnrelatedItemsWithoutArchive.Any()) ? 0 : ArticleRepository.CountDuplicates(
							Field.BackRelation.Constraint,
							unRelatedItemsAfterUpdate,
							Field.BackRelationId.Value
						)) + ArticleRepository.CountDuplicates(
							Field.BackRelation.Constraint,
							ids.ToArray(),
							Field.BackRelationId.Value
						);

						if (count > 0)
						{
							errors.Error(Field.FormName, "true", String.Format(ArticleStrings.BaseRelationUniqueViolation, count));
						}
					}
				}
			}

        }

        public void UpdateValue(string value)
        {
            if (Field.ExactType == FieldExactTypes.M2MRelation || Field.ExactType == FieldExactTypes.M2ORelation)
            {
                int[] currentItems = RelatedItems;
                int[] newItems = Converter.ToIdArray(value);
                Value = Converter.ToIdCommaList(newItems);
                NewUnrelatedItems = currentItems.Except(newItems).ToArray();
                NewRelatedItems = newItems.Except(currentItems).ToArray();
            }
            else
                Value = value;
        }
    }
}
