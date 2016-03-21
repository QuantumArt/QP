﻿using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Quantumart.QP8.BLL.Helpers
{
    public class ArticleListHelper
    {
        private const string EmptyIcon = "0.gif";
        private const string IsScheduledIcon = "is_scheduled.gif";
        private const string IsSplitedIcon = "is_splited.gif";
        private const string IsInvisibleIcon = "is_invisible.gif";
        private const string LockedByYouIcon = "locked.gif";
        private const string LockedNotByYouIcon = "locked_by_user.gif";


        internal static IEnumerable<SimpleDataRow> GetResult(IEnumerable<DataRow> rows, IEnumerable<Field> fieldList, bool? onlyIds)
        {
            var articleIds = rows.Select(x => (int)x.Field<decimal>("CONTENT_ITEM_ID"));
            if (onlyIds.HasValue && onlyIds.Value)
            {
                foreach (var id in articleIds)
                {
                    var dr = new SimpleDataRow { { FieldName.CONTENT_ITEM_ID, (decimal)id } };
                    yield return dr;
                }
            }
            else
            {
                var m2MFieldValues = new Dictionary<string, List<string>>();
                var m2OFieldValues = new Dictionary<string, List<string>>();
                var m2OFields = fieldList.Where(x => x.ExactType == FieldExactTypes.M2ORelation && x.ViewInList).ToList();
                var m2MFields = fieldList.Where(x => x.ExactType == FieldExactTypes.M2MRelation && x.ViewInList).ToList();

                foreach (var field in m2OFields)
                {
                    var m2ODisplayFieldName = ArticleService.GetTitleName(field.BackRelation.ContentId);
                    var m2OValues = ArticleService.GetM2OValuesBatch(articleIds, field.BackRelation.ContentId, field.Id, field.BackRelation.Name, m2ODisplayFieldName);
                    foreach (var val in m2OValues)
                    {
                        m2OFieldValues.Add(val.Key, val.Value);
                    }
                }

                foreach (var field in m2MFields)
                {
                    var m2MDisplayFieldName = ArticleService.GetTitleName(field.RelateToContentId.Value);
                    var m2MValues = ArticleService.GetM2MValuesBatch(articleIds, field.LinkId.Value, m2MDisplayFieldName, field.RelateToContentId.Value);
                    foreach (var val in m2MValues)
                    {
                        m2MFieldValues.Add(val.Key, val.Value);
                    }
                }

                var resultValues = m2MFieldValues.Union(m2OFieldValues).ToDictionary(k => k.Key, v => v.Value);

                foreach (var row in rows)
                {
                    var dr = new SimpleDataRow();

                    CopyValue(dr, row, FieldName.CONTENT_ITEM_ID);
                    CopyValue(dr, row, FieldName.STATUS_TYPE_NAME);
                    CopyValue(dr, row, "STATUS_TYPE_COLOR");
                    CopyValue(dr, row, FieldName.MODIFIER_LOGIN);

                    dr.Add(FieldName.MODIFIED, row.Field<DateTime>(FieldName.MODIFIED).ValueToDisplay());
                    dr.Add(FieldName.CREATED, row.Field<DateTime>(FieldName.CREATED).ValueToDisplay());

                    AddLockedByIcon(dr, row);
                    AddSplittedIcon(dr, row);
                    AddInvisibleIcon(dr, row);
                    AddScheduledIcon(dr, row);
                    var currentArticleId = (int)row.Field<decimal>("CONTENT_ITEM_ID");

                    var relationCounters = new Dictionary<int, int>();
                    foreach (var field in fieldList)
                    {
                        var sourceValue = GetSourceDynamicValue(field, row, relationCounters);
                        var destValue = GetDestDynamicValue(field, sourceValue, currentArticleId, resultValues);
                        dr.Add(field.FormName, destValue);
                    }

                    yield return dr;
                }
            }
        }

        private static void AddLockedByIcon(SimpleDataRow newRow, DataRow row)
        {
            var lockedBy = (int?)row.Field<decimal?>(FieldName.LOCKED_BY);
            string icon, toolTip;

            if (lockedBy == QPContext.CurrentUserId)
            {
                icon = LockedByYouIcon;
                toolTip = SiteStrings.Tooltip_LockedByYou;
            }
            else if (lockedBy.HasValue)
            {
                icon = LockedNotByYouIcon;
                var lockerDisplayName = row.Field<string>(FieldName.LOCKER_FIRST_NAME) + " " + row.Field<string>(FieldName.LOCKER_LAST_NAME);
                toolTip = string.Format(SiteStrings.Tooltip_LockedByUser, lockerDisplayName);
            }
            else
            {
                icon = EmptyIcon;
                toolTip = string.Empty;
            }

            newRow.Add(FieldName.LOCKED_BY_ICON, icon);
            newRow.Add(FieldName.LOCKED_BY_TOOLTIP, toolTip);
        }

        private static void AddSplittedIcon(SimpleDataRow newRow, DataRow row)
        {
            var splitted = row.Field<bool?>(FieldName.SPLITTED);
            string icon, toolTip;
            if (splitted.HasValue && splitted.Value)
            {
                icon = IsSplitedIcon;
                toolTip = ArticleStrings.IsSplitedTooltip;
            }
            else
            {
                icon = EmptyIcon;
                toolTip = string.Empty;
            }
            newRow.Add(FieldName.SPLITTED_ICON, icon);
            newRow.Add(FieldName.SPLITTED_TOOLTIP, toolTip);
        }

        private static void AddScheduledIcon(SimpleDataRow newRow, DataRow row)
        {
            var scheduled = row.Field<bool?>(FieldName.SCHEDULED);
            string icon, toolTip;
            if (scheduled.HasValue && scheduled.Value)
            {
                icon = IsScheduledIcon;
                toolTip = ArticleStrings.IsScheduledTooltip;
            }
            else
            {
                icon = EmptyIcon;
                toolTip = string.Empty;
            }
            newRow.Add(FieldName.SCHEDULED_ICON, icon);
            newRow.Add(FieldName.SCHEDULED_TOOLTIP, toolTip);
        }

        private static void AddInvisibleIcon(SimpleDataRow newRow, DataRow row)
        {
            var visible = row.Field<bool?>(FieldName.VISIBLE);
            string icon, toolTip;
            if (visible.HasValue && visible.Value)
            {
                icon = EmptyIcon;
                toolTip = string.Empty;
            }
            else
            {
                icon = IsInvisibleIcon;
                toolTip = ArticleStrings.IsInvisibleTooltip;
            }
            newRow.Add(FieldName.VISIBLE_ICON, icon);
            newRow.Add(FieldName.VISIBLE_TOOLTIP, toolTip);
        }


        private static void CopyValue(SimpleDataRow newRow, DataRow oldRow, string key)
        {
            newRow.Add(key, oldRow[key]);
        }

        private static string GetDestDynamicValue(Field field, object sourceValue, int articleId, Dictionary<string, List<string>> fieldValues)
        {
            var fieldFormattedValue = string.Empty;
            var fieldTypeName = field.Type.Name;

            if (fieldTypeName == FieldTypeName.Boolean)
            {
                var fieldValue = Converter.ToBoolean(sourceValue);
                fieldFormattedValue = $@"<input type=""checkbox"" align=""absmiddle"" {(fieldValue ? @" checked=""checked=""" : string.Empty)} disabled=""disabled"" />";
            }
            else if (fieldTypeName == FieldTypeName.Date
                || fieldTypeName == FieldTypeName.Time
                || fieldTypeName == FieldTypeName.DateTime)
            {
                var rawFieldValue = Converter.ToNullableDateTime(sourceValue);

                if (rawFieldValue != null)
                {
                    var fieldValue = (DateTime)rawFieldValue;

                    if (fieldTypeName == FieldTypeName.Date)
                    {
                        fieldFormattedValue = fieldValue.ToString("d");
                    }
                    else if (fieldTypeName == FieldTypeName.Time)
                    {
                        fieldFormattedValue = fieldValue.ToString("T");
                    }
                    else if (fieldTypeName == FieldTypeName.DateTime)
                    {
                        fieldFormattedValue = fieldValue.ToString("G");
                    }
                }
            }
            else if (fieldTypeName == FieldTypeName.Numeric && !field.IsClassifier)
            {
                var fieldSize = field.DecimalPlaces;
                var rawFieldValue = Converter.ToNullableDecimal(sourceValue);

                if (rawFieldValue.HasValue)
                {
                    fieldFormattedValue = rawFieldValue.Value.ToString("N" + fieldSize);
                }
            }

            else if (fieldTypeName == FieldTypeName.VisualEdit || fieldTypeName == FieldTypeName.Textbox)
            {
                var rawFieldValue = Converter.ToString(sourceValue);
                if (rawFieldValue.Length == Default.MaxViewInListFieldLength + 1)
                {
                    rawFieldValue = rawFieldValue.Substring(0, Default.MaxViewInListFieldLength) + "...";
                }

                if (!string.IsNullOrWhiteSpace(rawFieldValue))
                {
                    fieldFormattedValue = PlaceHolderHelper.ReplacePlaceHoldersToUrls(field.Content.Site, rawFieldValue);
                    fieldFormattedValue = Cleaner.RemoveAllHtmlTags(fieldFormattedValue);
                }
            }

            else if (field.ExactType == FieldExactTypes.StringEnum)
            {
                var alias = field.StringEnumItems
                    .Where(i => i.Value == Converter.ToString(sourceValue))
                    .Select(i => i.Alias)
                    .FirstOrDefault();

                fieldFormattedValue = Cleaner.RemoveAllHtmlTagsAndSpaces(Converter.ToString(alias));
            }

            else if (fieldTypeName == FieldTypeName.Relation || fieldTypeName == FieldTypeName.M2ORelation)
            {
                if (field.ExactType == FieldExactTypes.M2MRelation)
                {
                    List<string> result;
                    if (fieldValues.TryGetValue(articleId + "_" + field.LinkId.Value, out result))
                    {
                        var addDots = result.Count - 1 > Default.MaxViewInListArticleNumber;
                        if (addDots)
                        {
                            result.RemoveAt(Default.MaxViewInListArticleNumber);
                        }

                        fieldFormattedValue = string.Join(", ", result);

                        if (addDots)
                        {
                            fieldFormattedValue += "...";
                        }

                        fieldFormattedValue = Cleaner.RemoveAllHtmlTagsAndSpaces(Converter.ToString(fieldFormattedValue));
                    }
                    else
                        fieldFormattedValue = "";
                }

                else if (fieldTypeName == FieldTypeName.M2ORelation)
                {
                    List<string> result;
                    if (fieldValues.TryGetValue(articleId + "_" + field.Id, out result))
                    {
                        var addDots = result.Count() - 1 > Default.MaxViewInListArticleNumber;
                        if (addDots)
                        {
                            result.RemoveAt(Default.MaxViewInListArticleNumber);
                        }

                        fieldFormattedValue = string.Join(", ", result);

                        if (addDots)
                        {
                            fieldFormattedValue += "...";
                        }

                        fieldFormattedValue = Cleaner.RemoveAllHtmlTagsAndSpaces(Converter.ToString(fieldFormattedValue));
                    }
                    else
                    {
                        fieldFormattedValue = "";
                    }
                }
                else
                {
                    fieldFormattedValue = Cleaner.RemoveAllHtmlTagsAndSpaces(Converter.ToString(sourceValue));
                }
            }

            else
            {
                fieldFormattedValue = Cleaner.RemoveAllHtmlTagsAndSpaces(Converter.ToString(sourceValue));
            }

            return fieldFormattedValue;
        }

        private static object GetSourceDynamicValue(Field field, DataRow sourceRecord, Dictionary<int, int> relationCounters)
        {
            var sourceName = Article.GetDynamicColumnName(field, relationCounters);
            var sourceValue = sourceRecord[sourceName];
            return sourceValue;
        }
    }
}
