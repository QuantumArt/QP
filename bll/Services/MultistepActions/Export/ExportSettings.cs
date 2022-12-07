using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QPublishing.Info;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Export
{
    public class ExportSettings : IMultistepActionParams
    {
        public ExportSettings()
        {
            _fieldsToExpand = new Lazy<Field[]>(GetFieldsToExpand);
            _fieldsToExpandSettings = new Lazy<IEnumerable<FieldSetting>>(GetFieldsToExpandSettings);
            CustomFieldIds = new List<int>();
            FieldIdsToExpand = new List<int>();
            FieldNames = new List<string>();
            HeaderNames = new List<string>();
            Extensions = new List<Extension>();
        }

        public DateTime DateTimeForFileName { get; set; }

        public int ContentId { get; set; }

        public int SiteId { get; set; }

        public bool IsArchive { get; set; }

        public string Encoding { get; set; }

        public string Culture { get; set; }

        public char Delimiter { get; set; }

        public string LineSeparator { get; set; }

        public bool AllFields { get; set; }

        public bool ExcludeSystemFields { get; set; }

        public List<int> CustomFieldIds { get; set; }

        public List<int> FieldIdsToExpand { get; set; }

        [JsonIgnore]
        public Field[] FieldsToExpand => _fieldsToExpand.Value;

        [JsonIgnore]
        public IEnumerable<FieldSetting> FieldsToExpandSettings => _fieldsToExpandSettings.Value;

        private string _orderByField = string.Empty;
        private readonly Lazy<Field[]> _fieldsToExpand;
        private readonly Lazy<IEnumerable<FieldSetting>> _fieldsToExpandSettings;

        [JsonIgnore]
        public string OrderByField
        {
            get => _orderByField;
            set => _orderByField = value.Replace("ID", "content_item_id");
        }

        [JsonIgnore]
        public string UploadFilePath
        {
            get
            {
                var fileName = $"content_{ContentId}_{DateTimeForFileName.Year}_{DateTimeForFileName.Month}_{DateTimeForFileName.Day}_{DateTimeForFileName.Hour}_{DateTimeForFileName.Minute}_{DateTimeForFileName.Second}.csv";
                return $"{QPConfiguration.TempDirectory}{Path.DirectorySeparatorChar}{fileName}";
            }
        }

        public List<string> FieldNames { get; set; }

        public List<string> HeaderNames { get; set; }

        public string ExtensionsStr { get; set; }

        public List<Extension> Extensions { get; set; }

        private Field[] GetFieldsToExpand() => FieldIdsToExpand != null && FieldIdsToExpand.Any() ? FieldRepository.GetList(FieldIdsToExpand).ToArray() : new Field[] { };

        private IEnumerable<FieldSetting> GetFieldsToExpandSettings()
        {
            return FieldsToExpand.Select(n => new
            {
                Field = n,
                DisplayField = GetDisplayField(n),
                DisplayFields = ContentRepository.GetDisplayFields(n.RelateToContentId.Value, n).Select(rm => new
                {
                    Field = rm,
                    DisplayField = GetDisplayField(rm)
                })
            }).Select((n, i) => new FieldSetting(n.Field, i + 1, n.DisplayField, ContentId, Extensions)
            {
                Related = n.DisplayFields.Select((m, j) => new FieldSetting(m.Field, (i + 1) * 100 + j + 1, m.DisplayField, ContentId, Extensions)).ToList()
            }).ToArray();
        }

        private static Field GetDisplayField(Field n)
        {
            if (n.ExactType == FieldExactTypes.M2MRelation || n.ExactType == FieldExactTypes.M2ORelation)
            {
                return ContentRepository.GetTitleField(n.RelateToContentId.Value);
            }

            if (n.ExactType == FieldExactTypes.O2MRelation)
            {
                return n.Relation;
            }

            return null;
        }

        public class FieldSetting
        {
            public FieldSetting(Field field, int order, Field displayField, int exportedContent, IEnumerable<Extension> extensionsList)
            {
                Id = field.Id;
                ContentId = field.ContentId;
                ContentName = field.Content.Name;
                Name = field.Name;
                Order = order;
                LinkId = field.LinkId ?? 0;
                ExactType = field.ExactType;
                IsRelation = field.Relation != null;
                RelatedContentId = displayField?.ContentId ?? 0;
                RelatedContentName = displayField?.Content.Name;
                RelatedAttributeName = displayField?.Name;
                RelatedAttributeId = displayField?.Id ?? 0;
                BackRelatedAttributeId = field.BackRelation?.Id ?? 0;
                var extensionContent = extensionsList.SingleOrDefault(w => w.ContentId == field.ContentId);
                FromExtension = extensionContent != null;
                RelationByField = extensionContent != null ? extensionContent.RelationFieldName : field.BackRelation?.Name ?? string.Empty;
                ExcludeFromSQLRequest = field.ContentId != exportedContent && ! FromExtension;
            }

            public int Id { get; set; }

            public int ContentId { get; set; }

            public string ContentName { get; set; }

            public string Name { get; set; }

            public int Order { get; set; }

            public int RelatedContentId { get; set; }

            public string RelatedContentName { get; set; }

            public string RelatedAttributeName { get; set; }

            public int RelatedAttributeId { get; set; }

            public int BackRelatedAttributeId { get; set; }

            public bool IsRelation { get; set; }

            public string Alias => $"rel_{Order}_{RelatedContentId}";

            public string RelationTableAlias => $"rel_{Order}";

            public bool IsM2M => ExactType == FieldExactTypes.M2MRelation;

            public string CsvColumnName => $"{Name}.{(Related.Count() > 1 ? "Title" : RelatedAttributeName)}";

            public int? LinkId { get; set; }

            public IEnumerable<FieldSetting> Related { get; set; }

            public FieldExactTypes ExactType { get; set; }

            public bool FromExtension { get; set; }

            public string RelationByField { get; set; }

            public bool ExcludeFromSQLRequest { get; set; }
        }

        public class Extension
        {
            public int ContentId { get; set; }

            public string RelationFieldName { get; set; }

        }
    }
}
