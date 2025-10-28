using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Validators.TextFieldTag;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
    public class FieldValue
    {
        [Required]
        public Field Field { get; set; }

        public string Value
        {
            get => ObjectValue?.ToString() ?? string.Empty;
            set => ObjectValue = value;
        }

        public object ObjectValue { get; set; }

        public string ValueToMerge { get; set; }

        public Article Article { get; set; }

        public ArticleVersion Version { get; set; }

        public int[] RelatedItems => Converter.ToIdArray(Value);

        public int[] NewUnrelatedItems { get; set; } = { };

        public int[] NewRelatedItems { get; set; } = { };

        public void Validate(RulesException<Article> errors)
        {
            if (Field.IsReadOnly)
            {
                return;
            }

            if (Field.Required && string.IsNullOrEmpty(Value))
            {
                errors.Error(Field.FormName, Value, string.Format(ArticleStrings.MissingValue, Field.DisplayName));
            }

            if (QPContext.TextFieldTagValidation.Enabled
                && Field.ExactType is FieldExactTypes.File
                    or FieldExactTypes.Image
                    or FieldExactTypes.String
                    or FieldExactTypes.Textbox
                    or FieldExactTypes.VisualEdit
                && !string.IsNullOrWhiteSpace(Value))
            {
                TextFieldTagValidator.Validate(this, errors);
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
                DateTime parsedDt;
                if (!DateTime.TryParse(Value, out parsedDt))
                {
                    errors.Error(Field.FormName, Value, string.Format(ArticleStrings.InvalidDateTimeFormat, Field.DisplayName));
                }
                else
                {
                    if (Field.DenyPastDates && parsedDt < DateTime.Now)
                    {
                        errors.Error(Field.FormName, Value, string.Format(ArticleStrings.DenyPastDates, Field.DisplayName));
                    }
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
                    var backwardId = Field.BackRelation.Id;
                    var unRelatedItems = Converter.ToIdArray(ArticleRepository.GetRelatedItems(new [] {backwardId}, null)[backwardId]);
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
                        var count = !newUnrelatedItemsWithoutArchive.Any() ? 0 :
                            ArticleRepository.CountDuplicates(
                                Field.BackRelation.Constraint,
                                unRelatedItemsAfterUpdate.Union(ids).ToArray(),
                                Field.BackRelationId.Value,
                                false,
                                Field.BackRelation.Content.UseNativeEfTypes
                            );

                        if (count > 0)
                        {
                            errors.Error(Field.FormName, "true", string.Format(ArticleStrings.BaseRelationUniqueViolation, count));
                        }
                    }
                }
            }
        }

        public void UpdateValue(string value, FieldExactTypes? typeToOverride = null)
        {
            var resultType = typeToOverride ?? Field.ExactType;
            if (resultType is FieldExactTypes.M2MRelation or FieldExactTypes.M2ORelation)
            {
                var currentItems = RelatedItems;
                var newItems = Converter.ToIdArray(value);
                Value = Converter.ToIdCommaList(newItems);
                NewUnrelatedItems = currentItems.Except(newItems).ToArray();
                NewRelatedItems = newItems.Except(currentItems).ToArray();
            }
            else if (!string.IsNullOrEmpty(value) &&
                resultType is FieldExactTypes.Numeric or FieldExactTypes.O2MRelation
                && decimal.TryParse(value, out var parsed)
            )
            {
                ObjectValue = parsed;
            }
            else
            {
                Value = value;
            }
        }
    }
}
