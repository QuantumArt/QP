using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.BLL.Enums.Csv;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Services.MultistepActions.Export;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Csv
{
    public class CsvWriter
    {
        private const string IdentifierFieldName = FieldName.ContentItemId;

        private const string FieldNameHeaderTemplate = "{0}.{1}";

        private readonly int _siteId;
        private readonly int _contentId;
        private readonly ExportSettings _settings;
        private readonly int[] _ids;
        private int _processedItemsCount;
        private int _step;
        private StringBuilder _sb;
        private int _itemsPerStep;
        private readonly IEnumerable<Content> _extensionContents;

        public CsvWriter(int siteId, int contentId, int[] ids, IEnumerable<Content> extensionContents, ExportSettings settings)
        {
            _siteId = siteId;
            _contentId = contentId;
            _settings = settings;
            _ids = ids;
            _extensionContents = extensionContents;
        }

        public bool CsvReady => _itemsPerStep * (_step + 1) >= _ids.Length;


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

        private int StartFrom => _step * _itemsPerStep + 1;

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
                    FieldName.ContentId,
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

            var fields = _extensionContents.SelectMany(c => c.Fields).Concat(FieldRepository.GetFullList(_contentId)).ToList();
            var ids = articles.AsEnumerable().Select(n => Convert.ToInt32(n["content_item_id"])).ToArray();
            var extensionIdsMap = _extensionContents.ToDictionary(c => c.Id, c => articles
                .AsEnumerable()
                .Select(n => Converter.ToNullableInt32(n[string.Format(FieldNameHeaderTemplate, c.Name, IdentifierFieldName)]))
                .Where(n => n.HasValue)
                .Select(n => n.Value)
                .ToArray()
            );

            if (articles.Any())
            {
                var dict = fields
                    .Where(n => n.ExactType == FieldExactTypes.M2MRelation && articles[0].Table.Columns.Contains(n.ContentId == _contentId ? n.Name : string.Format(FieldNameHeaderTemplate, n.Content.Name, n.Name)))
                    .Select(n => new { LinkId = n.LinkId.Value, n.ContentId })
                    .ToDictionary(n => $"{n.LinkId}_{n.ContentId}", m => ArticleRepository.GetLinkedItemsMultiple(m.LinkId, m.ContentId == _contentId ? ids : extensionIdsMap[m.ContentId], true));

                var m2oFields = fields.Where(w => w.ExactType == FieldExactTypes.M2ORelation).ToArray();
                foreach (var field in m2oFields)
                {
                    var m2ODisplayFieldName = ContentRepository.GetTitleName(field.BackRelation.ContentId);
                    var m2OValues = ArticleRepository.GetM2OValues(
                        articles.AsEnumerable().Select(
                            n => Convert.ToInt32(n["content_item_id"])
                        ).ToList(),
                        field.BackRelation.ContentId,
                        field.Id,
                        field.BackRelation.Name,
                        m2ODisplayFieldName
                    );

                    foreach (var value in m2OValues)
                    {
                        var key = value.Key.Item1 + "_" + value.Key.Item2;
                        dict.Add(key, new Dictionary<int, string>() { { value.Key.Item1, string.Join(",", value.Value) } });
                    }
                }

                foreach (var article in articles)
                {
                    _sb.AppendFormat("{0}{1}", article["content_item_id"], _settings.Delimiter);
                    foreach (DataColumn column in article.Table.Columns)
                    {
                        var value = article[column.ColumnName].ToString();
                        var field = fields.FirstOrDefault(f => f.ContentId == _contentId
                            ? string.Equals(f.Name, column.ColumnName, StringComparison.InvariantCultureIgnoreCase)
                            : string.Equals(string.Format(FieldNameHeaderTemplate, f.Content.Name, f.Name), column.ColumnName, StringComparison.InvariantCultureIgnoreCase));
                        var alias = aliases.FirstOrDefault(n => aliases.Contains(column.ColumnName, StringComparer.InvariantCultureIgnoreCase));
                        if (!string.IsNullOrEmpty(alias))
                        {
                            _sb.AppendFormat("{0}{1}", FormatFieldValue(value), _settings.Delimiter);
                        }
                        else if (field != null)
                        {
                            _sb.AppendFormat("{0}{1}", FormatFieldValue(article, value, field, dict), _settings.Delimiter);
                        }
                        else if (_extensionContents.Any(c => string.Equals(string.Format(FieldNameHeaderTemplate, c.Name, IdentifierFieldName), column.ColumnName, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            _sb.AppendFormat("{0}{1}", string.IsNullOrEmpty(value) ? "NULL" : value, _settings.Delimiter);
                        }
                    }

                    if (!_settings.ExcludeSystemFields)
                    {
                        foreach (var fieldValue in new[]
                        {
                            MultistepActionHelper.DateCultureFormat(
                                article[FieldName.Created].ToString(), CultureInfo.CurrentCulture.Name, _settings.Culture),
                            MultistepActionHelper.DateCultureFormat(
                                article[FieldName.Modified].ToString(), CultureInfo.CurrentCulture.Name, _settings.Culture
                                ),
                            article[FieldName.UniqueId].ToString(),
                            _contentId.ToString(),
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

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            PathHelper.EnsureUploadPathCreated();
            using (var sw = new StreamWriter(_settings.UploadFilePath, true, Encoding.GetEncoding(_settings.Encoding)))
            {
                sw.Write(_sb.ToString());
            }

            _processedItemsCount = articles.Count;
        }

        private List<DataRow> GetArticlesForExport(IEnumerable<ExportSettings.FieldSetting> fieldsToExpand)
        {
            var dbType = QPContext.DatabaseType;
            var sb = new StringBuilder();
            if (_settings.FieldNames.Any())
            {
                foreach (var setting in _settings.FieldNames)
                {
                    sb.AppendFormat($", {setting}");
                }
            }

            var ns = SqlQuerySyntaxHelper.DbSchemaName(dbType);
            foreach (var field in fieldsToExpand)
            {
                var fieldAlias = SqlQuerySyntaxHelper.EscapeEntityName(dbType, field.Alias);
                var tableName = field.FromExtension ? $"ex_{field.ContentId}" : "base";
                var conversion = dbType == DatabaseType.Postgres ? "::integer" : "";
                var articleIdField = $"{tableName}.content_item_id{conversion}";

                if (field.ExcludeFromSQLRequest)
                {
                    sb.AppendFormat($", NULL as {fieldAlias}");
                }
                else if (field.ExactType == FieldExactTypes.O2MRelation)
                {
                    var partSeparator = dbType == DatabaseType.Postgres ? " || '; ' || " : " + '; ' + ";
                    var fieldExpression = string.Join(partSeparator, GetParts(field));
                    sb.Append($", {fieldExpression} as {fieldAlias}");
                }
                else if (field.ExactType == FieldExactTypes.M2ORelation)
                {
                    sb.Append($", {ns}qp_m2o_titles({articleIdField}, {field.RelatedAttributeId}, {field.BackRelatedAttributeId}, 255) as {fieldAlias}");
                }
                else
                {
                    sb.Append($", {ns}qp_link_titles({field.LinkId}, {articleIdField}, {field.RelatedAttributeId}, 255) as {fieldAlias}");
                }
            }

            var stepLength = Math.Min(_itemsPerStep, _ids.Length - StartFrom + 1);
            var stepIds = new int[stepLength];
            Array.Copy(_ids, StartFrom - 1, stepIds, 0, stepLength);

            var orderBy = string.IsNullOrEmpty(_settings.OrderByField) ? IdentifierFieldName : _settings.OrderByField;
            var archive = _settings.IsArchive ? "1": "0";
            var filter = $"base.content_item_id in ({string.Join(",", stepIds)}) and ci.archive = {archive}";
            return ArticleRepository.GetArticlesForExport(_contentId, _settings.ExtensionsStr, sb.ToString(), filter, 1, _itemsPerStep, orderBy, fieldsToExpand);
        }

        private static IEnumerable<string> GetParts(ExportSettings.FieldSetting field)
        {
            var parts = new List<string>();
            if (field.Related == null || !field.Related.Any())
            {
                parts.Add($"{field.RelationTableAlias}.content_item_id");
            }
            else
            {
                var dbType = QPContext.DatabaseType;
                var ns = SqlQuerySyntaxHelper.DbSchemaName(dbType);
                var contentItemIdField = dbType == DatabaseType.Postgres ? "content_item_id::integer" : "content_item_id";
                foreach (var f in field.Related)
                {
                    switch (f.ExactType)
                    {
                        case FieldExactTypes.M2MRelation:
                            parts.Add($"{ns}qp_link_titles({f.LinkId.Value}, {field.RelationTableAlias}.{contentItemIdField}, {f.RelatedAttributeId}, 255)");
                            break;
                        case FieldExactTypes.O2MRelation:
                            var o2mName = $"{f.RelationTableAlias}.{SqlQuerySyntaxHelper.EscapeEntityName(dbType, f.RelatedAttributeName)}";
                            parts.Add(SqlQuerySyntaxHelper.CastToString(dbType, o2mName));
                           break;
                        case FieldExactTypes.M2ORelation:
                            parts.Add($"{ns}qp_m2o_titles({field.RelationTableAlias}.{contentItemIdField}, {f.RelatedAttributeId}, {f.BackRelatedAttributeId}, 255)");
                            break;
                        default:
                            var fieldName = $"{field.RelationTableAlias}.{SqlQuerySyntaxHelper.EscapeEntityName(dbType, f.Name)}";
                            if (new[]
                                {
                                    FieldExactTypes.Date,
                                    FieldExactTypes.DateTime,
                                    FieldExactTypes.Time,
                                    FieldExactTypes.Textbox,
                                    FieldExactTypes.VisualEdit,
                                    FieldExactTypes.Numeric,
                                    FieldExactTypes.Classifier
                                }.Contains(f.ExactType))
                            {
                                fieldName = SqlQuerySyntaxHelper.CastToString(dbType, fieldName);
                            }
                            parts.Add($"coalesce({fieldName}, '')");
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

        private string FormatFieldValue(DataRow article, string value, Field field, IReadOnlyDictionary<string, Dictionary<int, string>> valuesWithRelation)
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

            if (field != null && (field.RelationType == RelationType.ManyToMany || field.RelationType == RelationType.ManyToOne))
            {
                value = string.Empty;
                var mapValue = field.RelationType == RelationType.ManyToMany ?
                    $"{field.LinkId}_{field.ContentId}" :
                    article["content_Item_id"] + "_" + field.Id;
                if (valuesWithRelation.TryGetValue(mapValue, out var mappings) && mappings.Any())
                {
                    var key = field.ContentId == _contentId ? IdentifierFieldName : string.Format(FieldNameHeaderTemplate, field.Content.Name, IdentifierFieldName);
                    if (int.TryParse(article[key].ToString(), out var id))
                    {
                        if (mappings.TryGetValue(id, out var items))
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
                fields =  GetExtensionFields(content, FieldNameHeaderTemplate, QPContext.DatabaseType);
                result.AddRange(fields);
            }

            return result.ToArray();
        }

        private string Escape(DatabaseType dbType, string expression) => SqlQuerySyntaxHelper.EscapeEntityName(dbType, expression);
        private string[] GetFieldNames(IEnumerable<Content> extensionContents)
        {
            var result = new List<string>();
            var dbType = QPContext.DatabaseType;
            var fields = GetBaseFields().Select(s => $"base.{SqlQuerySyntaxHelper.EscapeEntityName(dbType, s.Name)}");
            result.AddRange(fields);

            foreach (var content in extensionContents)
            {
                var template = $"ex_{content.Id}.{Escape(dbType, "{1}")} {Escape(dbType, "{0}.{1}")}";
                fields = GetExtensionFields(content, template, dbType);
                result.AddRange(fields);
            }

            return result.ToArray();
        }

        private IEnumerable<Field> GetBaseFields() => from f in FieldRepository.GetList(_settings.ContentId, false)
            where _settings.AllFields || _settings.CustomFieldIds.Contains(f.Id)
            select f;

        private IEnumerable<string> GetExtensionFields(Content content, string template, DatabaseType databaseType)
        {
            return (from f in content.Fields
                    where (f.ExactType != FieldExactTypes.M2ORelation) & !f.Aggregated && (_settings.AllFields || _settings.CustomFieldIds.Contains(f.Id))
                    select string.Format(template, FixName(databaseType, content.Name),FixName(databaseType, f.Name) ))
                .Concat(new[] { string.Format(template, FixName(databaseType, content.Name),FixName(databaseType, IdentifierFieldName)) });
        }

        private string FixName(DatabaseType dbType, string name) => dbType == DatabaseType.Postgres ? name.ToLower() : name;

        private static string GetExtensions(IEnumerable<Content> extensionContents)
        {
            var sb = new StringBuilder();
            foreach (var content in extensionContents)
            {
                var name = content.Fields.Single(f => f.Aggregated).Name;
                var dbType = QPContext.DatabaseType;
                var tableName = $"content_{content.Id}_united";
                var tableAlias = $"ex_{content.Id}";
                var fieldName = SqlQuerySyntaxHelper.EscapeEntityName(dbType, name);
                sb.AppendLine($" left join {tableName} {tableAlias} ON {tableAlias}.{fieldName} = base.content_item_id");
            }

            return sb.ToString();
        }

        private void UpdateExportSettings()
        {
            _settings.FieldNames = GetFieldNames(_extensionContents).ToList();
            _settings.HeaderNames = GetHeaderNames(_extensionContents).ToList();
            _settings.ExtensionsStr = GetExtensions(_extensionContents);
            _settings.Extensions = _extensionContents.Select(s => new ExportSettings.Extension
            {
                ContentId = s.Id,
                RelationFieldName = s.Fields.SingleOrDefault(n => n.Aggregated)?.Name ?? String.Empty
            }).ToList();

        }
    }
}
