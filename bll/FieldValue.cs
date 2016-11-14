using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
    public class FieldValue
    {
        public Field Field { get; set; }

        public string Value
        {
            get { return ObjectValue?.ToString() ?? string.Empty; }
            set { ObjectValue = value; }
        }

        public object ObjectValue { get; set; }

        public string ValueToMerge { get; set; }

        public Article Article { get; set; }

        public ArticleVersion Version { get; set; }

        public int[] RelatedItems => Converter.ToIdArray(Value);

        public int[] CurrentRelatedItems => Converter.ToIdArray(ArticleRepository.GetRelatedItems(Field.BackRelationId.Value, Article.Id));

        public int[] NewUnrelatedItems { get; set; }

        public int[] NewRelatedItems { get; set; }

        public void Validate(RulesException<Article> errors)
        {
            if (Field.Required && string.IsNullOrEmpty(Value))
            {
                errors.Error(Field.FormName, Value, string.Format(ArticleStrings.MissingValue, Field.DisplayName));
            }

            if (Field.ExactType == FieldExactTypes.String)
            {
                if (!string.IsNullOrEmpty(Value) && Value.Length > Field.StringSize)
                {
                    errors.Error(Field.FormName, Value, string.Format(ArticleStrings.TooLongValue, Field.DisplayName, Field.StringSize));
                }
                else if (!string.IsNullOrEmpty(Field.InputMask) && !Regex.IsMatch(Value, Field.InputMask))
                {
                    errors.Error(Field.FormName, Value, string.Format(ArticleStrings.InputMaskNotMatch, Field.DisplayName));
                }
            }
            else if (Field.IsDateTime && !string.IsNullOrEmpty(Value))
            {
                DateTime dt;
                if (!DateTime.TryParse(Value, out dt))
                {
                    errors.Error(Field.FormName, Value, string.Format(ArticleStrings.InvalidDateTimeFormat, Field.DisplayName));
                }
            }
            else if (Field.ExactType == FieldExactTypes.Classifier && !string.IsNullOrEmpty(Value))
            {
                var isCorrectClassifierValue = FieldRepository.GetAggregatableContentListItemsForClassifier(Field).Any(i => StringComparer.CurrentCultureIgnoreCase.Equals(i.Value, Value));
                if (!isCorrectClassifierValue)
                {
                    errors.Error(Field.FormName, Value, ArticleStrings.InvalidClassiferValue);
                }
            }
            else if (Field.ExactType == FieldExactTypes.O2MRelation && Field.RelateToContentId == Field.ContentId && Value == Article.Id.ToString())
            {
                errors.Error(Field.FormName, Value, ArticleStrings.O2MRelateToCurrentArticle);
            }
            else if (Field.ExactType == FieldExactTypes.M2ORelation)
            {
                var ids = new HashSet<int>(RelatedItems);
                if (ids.Contains(Article.Id))
                {
                    errors.Error(Field.FormName, "true", ArticleStrings.O2MRelateToCurrentArticle);
                }

                if (Field.BackRelation.UseRelationCondition && ids.Any())
                {
                    if (!Article.CheckRelationCondition(Field.BackRelation.RelationCondition))
                    {
                        errors.Error(Field.FormName, "true", ArticleStrings.CurrentArticleViolatesRelationCondition);
                    }
                }

                var newUnrelatedItemsWithoutArchive = ArticleRepository.ExcludeArchived(NewUnrelatedItems.ToArray());
                if (Field.BackRelation.Required && newUnrelatedItemsWithoutArchive.Any())
                {
                    errors.Error(Field.FormName, "true", ArticleStrings.BaseRelationFieldRequired);
                }

                if (Field.BackRelation.IsUnique)
                {
                    var unRelatedItems = Converter.ToIdArray(ArticleRepository.GetRelatedItems(Field.BackRelationId.Value, null));
                    var unRelatedItemsAfterUpdate = unRelatedItems.Except(NewRelatedItems).Concat(newUnrelatedItemsWithoutArchive).ToArray();
                    if (!Field.BackRelation.Constraint.IsComplex)
                    {
                        if (unRelatedItemsAfterUpdate.Length > 1)
                        {
                            errors.Error(Field.FormName, "true", string.Format(ArticleStrings.BaseRelationUniqueViolation, unRelatedItemsAfterUpdate.Length));
                        }

                        if (ids.Count > 1)
                        {
                            errors.Error(Field.FormName, "true", string.Format(ArticleStrings.BaseRelationUniqueViolation, ids.Count));
                        }
                    }
                    else
                    {
                        var count = (!newUnrelatedItemsWithoutArchive.Any() ? 0 : ArticleRepository.CountDuplicates(Field.BackRelation.Constraint, unRelatedItemsAfterUpdate, Field.BackRelationId.Value))
                            + ArticleRepository.CountDuplicates(Field.BackRelation.Constraint, ids.ToArray(), Field.BackRelationId.Value);

                        if (count > 0)
                        {
                            errors.Error(Field.FormName, "true", string.Format(ArticleStrings.BaseRelationUniqueViolation, count));
                        }
                    }
                }
            }
        }

        public void UpdateValue(string value)
        {
            if (Field.ExactType == FieldExactTypes.M2MRelation || Field.ExactType == FieldExactTypes.M2ORelation)
            {
                var currentItems = RelatedItems;
                var newItems = Converter.ToIdArray(value);
                Value = Converter.ToIdCommaList(newItems);
                NewUnrelatedItems = currentItems.Except(newItems).ToArray();
                NewRelatedItems = newItems.Except(currentItems).ToArray();
            }
            else
            {
                Value = value;
            }
        }
    }
}
