using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Enums.Csv;
using Quantumart.QP8.BLL.Extensions;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Services.MultistepActions.Export;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Csv
{
    public class CsvWriter
    {
        private const string FolderForUpload = "temp";
        private const string IdentifierFieldName = FieldName.ContentItemId;
        private const string ExtensionQueryTemplate = " LEFT JOIN CONTENT_{0} [ex{1}] ON [ex{1}].[{2}] = base.CONTENT_ITEM_ID ";
        private const string FieldNameQueryTemplate = "[ex{0}].[{1}] [{0}.{1}]";
        private const string FieldNameHeaderTemplate = "{0}.{1}";
        private const string BaseFieldNameQueryTemplate = "base.[{0}]";

        public CsvWriter(int siteId, int contentId, int[] ids, ExportSettings setts)
        {
            _siteId = siteId;
            _contentId = contentId;
            _settings = setts;
            _ids = ids;
        }

        public bool CsvReady => _itemsPerStep * (_step + 1) >= _ids.Length;

        public string CopyFileToTempSiteLiveDirectory()
        {
            var fileInfo = new FileInfo(_settings.UploadFilePath);
            if (fileInfo.Exists)
            {
                var currentSite = SiteRepository.GetById(_siteId);
                var uploadDir = $@"{currentSite.LiveDirectory}\{FolderForUpload}";
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                var newFileUploadPath = $@"{uploadDir}\{fileInfo.Name}";
                File.Copy(_settings.UploadFilePath, newFileUploadPath, true);
                File.Delete(_settings.UploadFilePath);

                return $"{fileInfo.Name}";
            }

            return string.Empty;
        }

        public int Write(int step, int itemsPerStep)
        {
            _sb = new StringBuilder();
            _step = step;
            _itemsPerStep = itemsPerStep;
            if (_step == 0)
            {
                UpdateExportSettings();
                WriteFieldNames();
            }

            WriteFieldValues();
            return _processedItemsCount;
        }

        private readonly int _siteId;
        private readonly int _contentId;
        private readonly ExportSettings _settings;
        private readonly int[] _ids;
        private int _processedItemsCount;
        private int _step;
        private StringBuilder _sb;
        private int _itemsPerStep;
        private Content[] _extensionContents;

        private int StartFrom => _step * _itemsPerStep + 1;

        private IEnumerable<Content> ExtensionContents
        {
            get
            {
                if (_extensionContents == null)
                {
                    var extensionContentIds = ContentRepository.GetReferencedAggregatedContentIds(_settings.ContentId, _ids ?? new int[0]);
                    _extensionContents = ContentRepository.GetList(extensionContentIds).ToArray();
                }

                return _extensionContents;
            }
        }

        private void WriteFieldNames()
        {
            if (_settings.Delimiter.ToString() != CsvDelimiter.Tab.Description())
            {
                _sb.AppendFormat("sep={0}", _settings.Delimiter);
                _sb.Append(_settings.LineSeparator);
            }

            _sb.Append(FieldName.ContentItemId);
            if (_settings.HeaderNames.Any())
            {
                _sb.AppendFormat("{0}{1}", _settings.Delimiter, string.Join(_settings.Delimiter.ToString(), _settings.HeaderNames));
            }

            foreach (var field in _settings.FieldsToExpandSettings)
            {
                _sb.AppendFormat("{0}{1}", _settings.Delimiter, field.CsvColumnName);
            }

            if (!_settings.ExcludeSystemFields)
            {
                foreach (var fieldName in new[]
                {
                    FieldName.Created,
                    FieldName.Modified,
                    FieldName.UniqueId,
                    FieldName.IsChanged
                })
                {
                    _sb.Append(_settings.Delimiter);
                    _sb.Append(fieldName);
                }
            }

            _sb.Append(_settings.LineSeparator);
        }

        private void WriteFieldValues()
        {
            var articles = GetArticlesForExport(_settings.FieldsToExpandSettings);
            var aliases = _settings.FieldsToExpandSettings.Select(n => n.Alias).ToArray();

            var fields = ExtensionContents.SelectMany(c => c.Fields).Concat(FieldRepository.GetFullList(_contentId)).ToList();
            var ids = articles.AsEnumerable().Select(n => (int)n.Field<decimal>("content_item_id")).ToArray();
            var extensionIdsMap = ExtensionContents.ToDictionary(c => c.Id, c => articles
                .AsEnumerable()
                .Select(n => n.Field<decimal?>(string.Format(FieldNameHeaderTemplate, c.Name, IdentifierFieldName)))
                .Where(n => n.HasValue)
                .Select(n => (int)n.Value)
                .ToArray()
            );

            if (articles.Any())
            {
                var dict = fields
                    .Where(n => n.ExactType == FieldExactTypes.M2MRelation && articles[0].Table.Columns.Contains(n.ContentId == _contentId ? n.Name : string.Format(FieldNameHeaderTemplate, n.Content.Name, n.Name)))
                    .Select(n => new { LinkId = n.LinkId.Value, n.ContentId })
                    .ToDictionary(n => n.LinkId, m => ArticleRepository.GetLinkedItemsMultiple(m.LinkId, m.ContentId == _contentId ? ids : extensionIdsMap[m.ContentId]));

                foreach (var article in articles)
                {
                    _sb.AppendFormat("{0}{1}", article["content_item_id"], _settings.Delimiter);
                    foreach (DataColumn column in article.Table.Columns)
                    {
                        var value = article[column.ColumnName].ToString();
                        var field = fields.FirstOrDefault(f => f.ContentId == _contentId ? f.Name == column.ColumnName : string.Format(FieldNameHeaderTemplate, f.Content.Name, f.Name) == column.ColumnName);
                        var alias = aliases.FirstOrDefault(n => aliases.Contains(column.ColumnName));
                        if (!string.IsNullOrEmpty(alias))
                        {
                            _sb.AppendFormat("{0}{1}", FormatFieldValue(value), _settings.Delimiter);
                        }
                        else if (field != null)
                        {
                            _sb.AppendFormat("{0}{1}", FormatFieldValue(article, value, field, dict), _settings.Delimiter);
                        }
                        else if (ExtensionContents.Any(c => string.Format(FieldNameHeaderTemplate, c.Name, IdentifierFieldName) == column.ColumnName))
                        {
                            _sb.AppendFormat("{0}{1}", string.IsNullOrEmpty(value) ? "NULL" : value, _settings.Delimiter);
                        }
                    }

                    if (!_settings.ExcludeSystemFields)
                    {
                        foreach (var fieldValue in new[]
                        {
                                MultistepActionHelper.DateCultureFormat(article[FieldName.Created].ToString(), CultureInfo.CurrentCulture.Name, _settings.Culture),
                                MultistepActionHelper.DateCultureFormat(article[FieldName.Modified].ToString(), CultureInfo.CurrentCulture.Name, _settings.Culture),
                                article[FieldName.UniqueId].ToString(),
                                "0"
                            })
                        {
                            _sb.Append(fieldValue);
                            _sb.Append(_settings.Delimiter);
                        }
                    }

                    _sb.Append(_settings.LineSeparator);
                }
            }

            using (var sw = new StreamWriter(_settings.UploadFilePath, true, Encoding.GetEncoding(_settings.Encoding)))
            {
                sw.Write(_sb.ToString());
            }

            _processedItemsCount = articles.Count;
        }

        private List<DataRow> GetArticlesForExport(IEnumerable<ExportSettings.FieldSetting> fieldsToExpand)
        {
            var sb = new StringBuilder();
            if (_settings.FieldNames.Any())
            {
                foreach (var s in _settings.FieldNames)
                {
                    sb.AppendFormat(", {0}", s);
                }
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

            var stepLength = Math.Min(_itemsPerStep, _ids.Length - StartFrom + 1);
            var stepIds = new int[stepLength];
            Array.Copy(_ids, StartFrom - 1, stepIds, 0, stepLength);

            var orderBy = string.IsNullOrEmpty(_settings.OrderByField) ? IdentifierFieldName : _settings.OrderByField;
            var filter = $"base.content_item_id in ({string.Join(",", stepIds)}) and base.archive = 0";
            return ArticleRepository.GetArticlesForExport(_contentId, _settings.Extensions, sb.ToString(), filter, 1, _itemsPerStep, orderBy, fieldsToExpand);
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
                    switch (f.ExactType)
                    {
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

        private static string FormatFieldValue(string value)
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

        private string FormatFieldValue(DataRow article, string value, Field field, IReadOnlyDictionary<int, Dictionary<int, string>> m2MValues)
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
                    value = MultistepActionHelper.DateCultureFormat(value, CultureInfo.CurrentCulture.Name, _settings.Culture);
                }
                else if ((field.Type.DbType == DbType.Double || field.Type.DbType == DbType.Decimal) && field.RelationType != RelationType.ManyToMany)
                {
                    value = MultistepActionHelper.NumericCultureFormat(value, CultureInfo.CurrentCulture.Name, _settings.Culture);
                    if (value.Contains(_settings.Delimiter))
                    {
                        value = $"\"{value}\"";
                    }
                }
            }

            if (field != null && field.RelationType == RelationType.ManyToMany)
            {
                value = string.Empty;
                Dictionary<int, string> mappings;
                if (m2MValues.TryGetValue(field.LinkId.Value, out mappings) && mappings.Any())
                {
                    var key = field.ContentId == _contentId ? IdentifierFieldName : string.Format(FieldNameHeaderTemplate, field.Content.Name, IdentifierFieldName);
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

        private string[] GetHeaderNames(IEnumerable<Content> extensionContents)
        {
            var result = new List<string>();
            var fields = GetBaseFields().Select(s => s.Name);
            result.AddRange(fields);

            foreach (var content in extensionContents)
            {
                fields = GetExtensionFields(content, FieldNameHeaderTemplate);
                result.AddRange(fields);
            }

            return result.ToArray();
        }

        private string[] GetFieldNames(IEnumerable<Content> extensionContents)
        {
            var result = new List<string>();
            var fields = GetBaseFields().Select(s => string.Format(BaseFieldNameQueryTemplate, s.Name));
            result.AddRange(fields);

            foreach (var content in extensionContents)
            {
                fields = GetExtensionFields(content, FieldNameQueryTemplate);
                result.AddRange(fields);
            }

            return result.ToArray();
        }

        private IEnumerable<Field> GetBaseFields()
        {
            return from f in FieldRepository.GetList(_settings.ContentId, false)
                   where f.ExactType != FieldExactTypes.M2ORelation && (_settings.AllFields || _settings.CustomFieldIds.Contains(f.Id))
                   select f;
        }

        private IEnumerable<string> GetExtensionFields(Content content, string template)
        {
            return (from f in content.Fields
                    where f.ExactType != FieldExactTypes.M2ORelation & !f.Aggregated && (_settings.AllFields || _settings.CustomFieldIds.Contains(f.Id))
                    select string.Format(template, content.Name, f.Name))
                   .Concat(new[] { string.Format(template, content.Name, IdentifierFieldName) });
        }

        private static string GetExtensions(IEnumerable<Content> extensionContents)
        {
            var sb = new StringBuilder();
            foreach (var content in extensionContents)
            {
                var name = content.Fields.Single(f => f.Aggregated).Name;
                sb.AppendFormat(ExtensionQueryTemplate, content.Id, content.Name, name);
            }

            return sb.ToString();
        }

        private void UpdateExportSettings()
        {
            _settings.FieldNames = GetFieldNames(ExtensionContents);
            _settings.HeaderNames = GetHeaderNames(ExtensionContents);
            _settings.Extensions = GetExtensions(ExtensionContents);
        }
    }
}
