using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Services.MultistepActions.Export;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Csv
{
    public class CsvWriter
    {
        #region Constants
        private const string FolderForUpload = "temp";
        private const string IdentifierFieldName = "CONTENT_ITEM_ID";
        private const string ExstensionQueryTemplate = " LEFT JOIN CONTENT_{0} [ex{1}] ON [ex{1}].[{2}] = base.CONTENT_ITEM_ID ";
        private const string FieldNameQueryTemplate = "[ex{0}].[{1}] [{0}.{1}]";
        private const string FieldNameHeaderTemplate = "{0}.{1}";
        private const string BaseFieldNameQueryTemplate = "base.[{0}]";
        #endregion

        #region Constructor
        public CsvWriter(int siteId, int contentId, int[] ids, ExportSettings setts)
        {
            this.siteId = siteId;
            this.contentId = contentId;
            this.setts = setts;
            this.ids = ids;
        }
        #endregion

        #region Public properties and methods
        public bool CsvReady => itemsPerStep * (step + 1) >= ids.Length;

        public string CopyFileToTempSiteLiveDirectory()
        {
            var fileInfo = new FileInfo(setts.UploadFilePath);
            if (fileInfo.Exists)
            {
                var currentSite = SiteRepository.GetById(siteId);
                var uploadDir = $@"{currentSite.LiveDirectory}\{FolderForUpload}";
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                var newFileUploadPath = $@"{uploadDir}\{fileInfo.Name}";
                File.Copy(setts.UploadFilePath, newFileUploadPath, true);
                File.Delete(setts.UploadFilePath);

                return $"{fileInfo.Name}";
            }
            return string.Empty;
        }

        public int Write(int step, int itemsPerStep)
        {
            sb = new StringBuilder();
            this.step = step;
            this.itemsPerStep = itemsPerStep;

            if (this.step == 0)
            {
                UpdateExportSettings();
                WriteFieldNames();
            }

            WriteFieldValues();
            return processedItemsCount;
        }
        #endregion

        #region Private fields
        private readonly int siteId;
        private readonly int contentId;
        private readonly ExportSettings setts;
        private readonly int[] ids;
        private int processedItemsCount;
        private int step;
        private StringBuilder sb;
        private int itemsPerStep;
        private int startFrom => step * itemsPerStep + 1;
        private Content[] exstensionContents;
        #endregion

        #region Private properties
        private Content[] ExstensionContents
        {
            get
            {
                if (exstensionContents == null)
                {
                    var exstensionContentIds = ContentRepository.GetReferencedAggregatedContentIds(setts.ContentId, ids ?? new int[0]);
                    exstensionContents = ContentRepository.GetList(exstensionContentIds).ToArray();
                }

                return exstensionContents;
            }
        }
        #endregion

        #region Private methods
        private void WriteFieldNames()
        {
            //Add parameter to file, so Excel can understand field delimiter
            sb.AppendFormat("sep={0}", setts.Delimiter);

            sb.Append(setts.LineSeparator);

            //Write field content_item_id
            sb.Append(ArticleStrings.CONTENT_ITEM_ID);

            //Add field names
            if (setts.HeaderNames.Any())
            {
                sb.AppendFormat("{0}{1}", setts.Delimiter, string.Join(setts.Delimiter.ToString(), setts.HeaderNames));
            }

            foreach (var field in setts.FieldsToExpandSettings)
            {
                sb.AppendFormat("{0}{1}", setts.Delimiter, field.CsvColumnName);
            }

            if (!setts.ExcludeSystemFields)
            {
                sb.AppendFormat("{0}{1}", setts.Delimiter, "CREATED");
                sb.AppendFormat("{0}{1}", setts.Delimiter, "MODIFIED");
                sb.AppendFormat("{0}{1}", setts.Delimiter, ArticleStrings.IsChanged);
            }
            sb.Append(setts.LineSeparator);
        }

        private void WriteFieldValues()
        {
            var articles = GetArticlesForExport(setts.FieldsToExpandSettings);
            var aliases = setts.FieldsToExpandSettings.Select(n => n.Alias).ToArray();
            using (var sw = new StreamWriter(setts.UploadFilePath, true, Encoding.GetEncoding(setts.Encoding)))
            {
                var fields = (from c in ExstensionContents
                              from f in c.Fields
                              select f)
                      .Concat(FieldRepository.GetFullList(contentId))
                      .ToList();

                var ids = articles.AsEnumerable().Select(n => (int)n.Field<decimal>("content_item_id")).ToArray();

                var exstensionIdsMap = ExstensionContents.ToDictionary(
                    c => c.Id,
                    c => articles
                        .AsEnumerable()
                        .Select(n => n.Field<decimal?>(string.Format(FieldNameHeaderTemplate, c.Name, IdentifierFieldName)))
                        .Where(n => n.HasValue)
                        .Select(n => (int)n.Value)
                        .ToArray()
                    );

                if (articles.Any())
                {
                    var dict = fields
                    .Where(n =>
                        n.ExactType == FieldExactTypes.M2MRelation &&
                        articles[0].Table.Columns.Contains(n.ContentId == contentId ? n.Name : string.Format(FieldNameHeaderTemplate, n.Content.Name, n.Name))
                    )
                    .Select(n => new { LinkId = n.LinkId.Value, n.ContentId })
                    .ToDictionary(n => n.LinkId, m => ArticleRepository.GetLinkedItemsMultiple(m.LinkId, m.ContentId == contentId ? ids : exstensionIdsMap[m.ContentId]));

                    foreach (var article in articles)
                    {
                        sb.AppendFormat("{0}{1}", article["content_item_id"], setts.Delimiter);
                        foreach (DataColumn column in article.Table.Columns)
                        {
                            var value = article[column.ColumnName].ToString();
                            var field = fields.FirstOrDefault(f => f.ContentId == contentId ? f.Name == column.ColumnName : string.Format(FieldNameHeaderTemplate, f.Content.Name, f.Name) == column.ColumnName);
                            var alias = aliases.FirstOrDefault(n => aliases.Contains(column.ColumnName));
                            if (!string.IsNullOrEmpty(alias))
                            {
                                sb.AppendFormat("{0}{1}", FormatFieldValue(value), setts.Delimiter);
                            }
                            else if (field != null)
                            {
                                sb.AppendFormat("{0}{1}", FormatFieldValue(article, value, field, dict), setts.Delimiter);
                            }
                            else if (ExstensionContents.Any(c => string.Format(FieldNameHeaderTemplate, c.Name, IdentifierFieldName) == column.ColumnName))
                            {
                                sb.AppendFormat("{0}{1}", string.IsNullOrEmpty(value) ? "NULL" : value, setts.Delimiter);
                            }
                        }

                        //Set flag for IsChanged column
                        if (!setts.ExcludeSystemFields)
                        {
                            sb.AppendFormat("{0}{1}", MultistepActionHelper.DateCultureFormat(article["created"].ToString(), CultureInfo.CurrentCulture.Name, setts.Culture), setts.Delimiter);
                            sb.AppendFormat("{0}{1}", MultistepActionHelper.DateCultureFormat(article["modified"].ToString(), CultureInfo.CurrentCulture.Name, setts.Culture), setts.Delimiter);
                            sb.AppendFormat("{0}{1}", 0, setts.Delimiter);
                        }

                        sb.Append(setts.LineSeparator);
                    }
                }
                sw.Write(sb.ToString());
                processedItemsCount = articles.Count();
            }
        }

        private List<DataRow> GetArticlesForExport(IEnumerable<ExportSettings.FieldSetting> fieldsToExpand)
        {
            //Формируем список полей для запроса
            var sb = new StringBuilder();
            foreach (var s in setts.FieldNames)
            {
                sb.AppendFormat(", {0}", s);
            }

            foreach (var field in fieldsToExpand)
            {
                if (field.ExactType == FieldExactTypes.O2MRelation)
                {
                    sb.AppendFormat(", {0} as [{1}]", string.Join(" + '; ' + ", GetParts(field)), field.Alias);
                }
                else
                {
                    sb.AppendFormat(", dbo.[qp_link_titles]({0}, base.content_item_id, {1}, 255) as [{2}]", field.LinkId, field.RelatedAttributeId, field.Alias);
                }
            }

            var columnsForSql = sb.ToString();
            var exstensions = setts.Exstensions;
            int itemsCount;
            var stepLength = Math.Min(itemsPerStep, ids.Length - startFrom + 1);
            var stepIds = new int[stepLength];
            Array.Copy(ids, startFrom - 1, stepIds, 0, stepLength);

            var orderBy = string.IsNullOrEmpty(setts.OrderByField) ? IdentifierFieldName : setts.OrderByField;
            var filter = $"base.content_item_id in ({string.Join(",", stepIds)}) and base.archive = 0";

            return ArticleRepository.GetArticlesForExport(contentId, exstensions, columnsForSql, filter, 1, itemsPerStep, orderBy, fieldsToExpand, out itemsCount);
        }

        private static IEnumerable<string> GetParts(ExportSettings.FieldSetting field)
        {
            var parts = new List<string>();
            if (field.Related == null || !field.Related.Any())
            {
                parts.Add($"cast({field.TableAlias}.content_item_id as nvarchar(255))");
            }
            else
            {
                foreach (var f in field.Related)
                {
                    switch (f.ExactType) {
                        case FieldExactTypes.M2MRelation:
                            parts.Add(string.Format("dbo.qp_link_titles({0}, {2}.content_item_id, {1}, 255)", f.LinkId.Value, f.RelatedAttributeId, field.TableAlias));
                            break;
                        case FieldExactTypes.O2MRelation:
                            parts.Add($"isnull(cast( {f.TableAlias}.[{f.RelatedAttributeName}] as nvarchar(255)), '')");
                            break;
                        default:
                            parts.Add(new[]
                            {
                                FieldExactTypes.Date,
                                FieldExactTypes.DateTime,
                                FieldExactTypes.Time,
                                FieldExactTypes.Textbox,
                                FieldExactTypes.VisualEdit,
                                FieldExactTypes.Numeric,
                                FieldExactTypes.Classifier
                            }.Contains(f.ExactType) ? $"isnull(cast ( {field.TableAlias}.[{f.Name}] as nvarchar(255)), '')" : $"isnull( {field.TableAlias}.[{f.Name}], '')");
                            break;
                    }
                }
            }
            return parts;
        }

        private string FormatFieldValue(string value)
        {
            if (value.Contains("\""))
            {
                value = value.Replace("\"", "\"\"");
            }

            if (!string.IsNullOrEmpty(value))
            {
                value = $"\"{value}\"";
            }

            if (string.IsNullOrEmpty(value) || value == "\"\"")
            {
                value = "NULL";
            }

            return value;
        }
        private string FormatFieldValue(DataRow article, string value, Field field, Dictionary<int, Dictionary<int, string>> m2mValues)
        {
            if (value.Contains("\""))
            {
                value = value.Replace("\"", "\"\"");
            }

            if (field != null && !string.IsNullOrEmpty(value))
            {
                if (field.Type.DbType == DbType.String || field.Type.DbType == DbType.StringFixedLength)
                {
                    value = $"\"{value}\"";
                }
                else if (field.ExactType == FieldExactTypes.Boolean)
                {
                    value = MultistepActionHelper.BoolFormat(value);
                }
                else if (field.Type.DbType == DbType.Date || field.Type.DbType == DbType.DateTime || field.Type.DbType == DbType.DateTime2)
                {
                    value = MultistepActionHelper.DateCultureFormat(value, CultureInfo.CurrentCulture.Name, setts.Culture);
                }
                else if ((field.Type.DbType == DbType.Double || field.Type.DbType == DbType.Decimal) && field.RelationType != RelationType.ManyToMany)
                {
                    value = MultistepActionHelper.NumericCultureFormat(value, CultureInfo.CurrentCulture.Name, setts.Culture);

                    if (value.Contains(setts.Delimiter))
                    {
                        value = $"\"{value}\"";
                    }
                }
            }

            if (field != null && field.RelationType == RelationType.ManyToMany)
            {
                value = string.Empty;
                Dictionary<int, string> mappings;
                if (m2mValues.TryGetValue(field.LinkId.Value, out mappings) && mappings.Any())
                {
                    var key = field.ContentId == contentId ? IdentifierFieldName : string.Format(FieldNameHeaderTemplate, field.Content.Name, IdentifierFieldName);
                    int id;
                    if (int.TryParse(article[key].ToString(), out id))
                    {
                        string items;
                        if (mappings.TryGetValue(id, out items))
                        {
                            value = items.Replace(",", ";");
                        }
                    }
                }
                value = $"\"{value}\"";
            }


            if (string.IsNullOrEmpty(value) || value == "\"\"")
            {
                value = "NULL";
            }
            return value;
        }

        private string[] GetHeaderNames(Content[] exstensionContents)
        {
            var result = new List<string>();
            var fields = GetBaseFields().Select(s => s.Name);

            result.AddRange(fields);

            foreach (var content in exstensionContents)
            {
                fields = GetExstensionFields(content, FieldNameHeaderTemplate);
                result.AddRange(fields);
            }

            return result.ToArray();
        }

        private string[] GetFieldNames(Content[] exstensionContents)
        {
            var result = new List<string>();
            var fields = GetBaseFields().Select(s => string.Format(BaseFieldNameQueryTemplate, s.Name));

            result.AddRange(fields);

            foreach (var content in exstensionContents)
            {
                fields = GetExstensionFields(content, FieldNameQueryTemplate);
                result.AddRange(fields);
            }

            return result.ToArray();
        }

        private IEnumerable<Field> GetBaseFields()
        {
            return from f in FieldRepository.GetList(setts.ContentId, false)
                   where f.ExactType != FieldExactTypes.M2ORelation && (setts.AllFields || setts.CustomFieldIds.Contains(f.Id))
                   select f;
        }

        private IEnumerable<string> GetExstensionFields(Content content, string template)
        {
            return (from f in content.Fields
                    where f.ExactType != FieldExactTypes.M2ORelation & !f.Aggregated && (setts.AllFields || setts.CustomFieldIds.Contains(f.Id))
                    select string.Format(template, content.Name, f.Name))
                   .Concat(new[] { string.Format(template, content.Name, IdentifierFieldName) });
        }

        private string GetExstensions(Content[] exstensionContents)
        {
            var sb = new StringBuilder();

            foreach (var content in exstensionContents)
            {
                var name = content.Fields.Single(f => f.Aggregated).Name;
                sb.AppendFormat(ExstensionQueryTemplate, content.Id, content.Name, name);
            }

            return sb.ToString();
        }

        private void UpdateExportSettings()
        {
            setts.FieldNames = GetFieldNames(ExstensionContents);
            setts.HeaderNames = GetHeaderNames(ExstensionContents);
            setts.Exstensions = GetExstensions(ExstensionContents);
        }
        #endregion
    }
}
