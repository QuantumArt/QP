using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Helpers
{
    public class ArticleListHelper
    {
        private static readonly string EMPTY_ICON = "0.gif";
        private static readonly string IS_SCHEDULED_ICON = "is_scheduled.gif";
        private static readonly string IS_SPLITED_ICON = "is_splited.gif";
        private static readonly string IS_INVISIBLE_ICON = "is_invisible.gif";
        private static readonly string LOCKED_BY_YOU_ICON = "locked.gif";
        private static readonly string LOCKED_NOT_BY_YOU_ICON = "locked_by_user.gif";


        internal static IEnumerable<SimpleDataRow> GetResult(IEnumerable<DataRow> rows, IEnumerable<Field> fieldList, bool? onlyIds)
        {
            var articleIds = rows.Select(x => (int)x.Field<decimal>("CONTENT_ITEM_ID"));
            if (onlyIds.HasValue && onlyIds.Value)
            {
                foreach (var id in articleIds)
                {
                    var dr = new SimpleDataRow();
                    dr.Add(FieldName.CONTENT_ITEM_ID, (decimal)id);
                    yield return dr;
                }
            }
            else
            {
                var m2mFieldValues = new Dictionary<string, List<string>>();
                var m2oFieldValues = new Dictionary<string, List<string>>();
                var m2oFields = fieldList.Where(x => x.ExactType == FieldExactTypes.M2ORelation && x.ViewInList).ToList();
                var m2mFields = fieldList.Where(x => x.ExactType == FieldExactTypes.M2MRelation && x.ViewInList).ToList();

                foreach (var field in m2oFields)
                {
                    var m2oDisplayFieldName = ArticleService.GetTitleName(field.BackRelation.ContentId);
                    var m2oValues = ArticleService.GetM2OValuesBatch(articleIds, field.BackRelation.ContentId, field.Id, field.BackRelation.Name, m2oDisplayFieldName);
                    foreach (var val in m2oValues)
                    {
                        m2oFieldValues.Add(val.Key, val.Value);
                    }
                }

                foreach (var field in m2mFields)
                {
                    var m2mDisplayFieldName = ArticleService.GetTitleName(field.RelateToContentId.Value);
                    var m2mValues = ArticleService.GetM2MValuesBatch(articleIds, field.LinkId.Value, m2mDisplayFieldName, field.RelateToContentId.Value);
                    foreach (var val in m2mValues)
                    {
                        m2mFieldValues.Add(val.Key, val.Value);
                    }
                }

                var resultValues = m2mFieldValues.Union(m2oFieldValues).ToDictionary(k => k.Key, v => v.Value);

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
                icon = LOCKED_BY_YOU_ICON;
                toolTip = SiteStrings.Tooltip_LockedByYou;
            }
            else if (lockedBy.HasValue)
            {
                icon = LOCKED_NOT_BY_YOU_ICON;
                var lockerDisplayName = row.Field<string>(FieldName.LOCKER_FIRST_NAME) + " " + row.Field<string>(FieldName.LOCKER_LAST_NAME);
                toolTip = string.Format(SiteStrings.Tooltip_LockedByUser, lockerDisplayName);
            }
            else
            {
                icon = EMPTY_ICON;
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
                icon = IS_SPLITED_ICON;
                toolTip = ArticleStrings.IsSplitedTooltip;
            }
            else
            {
                icon = EMPTY_ICON;
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
                icon = IS_SCHEDULED_ICON;
                toolTip = ArticleStrings.IsScheduledTooltip;
            }
            else
            {
                icon = EMPTY_ICON;
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
                icon = EMPTY_ICON;
                toolTip = string.Empty;
            }
            else
            {
                icon = IS_INVISIBLE_ICON;
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
                    var result = new List<string>();
                    if (fieldValues.TryGetValue(articleId + "_" + field.LinkId.Value, out result))
                    {
                        bool addDots;
                        if (addDots = result.Count() - 1 > Default.MaxViewInListArticleNumber)
                        {
                            result.RemoveAt(Default.MaxViewInListArticleNumber);
                        }

                        fieldFormattedValue = string.Join(", ", result);

                        if (addDots)
                            fieldFormattedValue += "...";
                        fieldFormattedValue = Cleaner.RemoveAllHtmlTagsAndSpaces(Converter.ToString(fieldFormattedValue));
                    }
                    else
                        fieldFormattedValue = "";
                }

                else if (fieldTypeName == FieldTypeName.M2ORelation)
                {
                    var result = new List<string>();
                    if (fieldValues.TryGetValue(articleId + "_" + field.Id, out result))
                    {
                        bool addDots;
                        if (addDots = result.Count() - 1 > Default.MaxViewInListArticleNumber)
                        {
                            result.RemoveAt(Default.MaxViewInListArticleNumber);
                        }

                        fieldFormattedValue = string.Join(", ", result);

                        if (addDots)
                            fieldFormattedValue += "...";
                        fieldFormattedValue = Cleaner.RemoveAllHtmlTagsAndSpaces(Converter.ToString(fieldFormattedValue));
                    }
                    else
                        fieldFormattedValue = "";
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
