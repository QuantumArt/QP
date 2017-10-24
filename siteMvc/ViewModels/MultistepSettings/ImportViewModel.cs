using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Enums.Csv;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.MultistepActions.Import;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.ViewModels.MultistepSettings
{
    public class ImportViewModel : ExportImportModel
    {
        public const string FieldPrefix = "Field_";
        private const string IdPrefix = FieldPrefix + "Id_";

        public override string ActionCode => Constants.ActionCode.ImportArticles;

        public override string EntityTypeCode => Constants.EntityTypeCode.Content;

        public int ContentId { get; set; }

        public UploaderType UploaderType => UploaderTypeHelper.UploaderType;

        public bool AllowUpload => true;

        [LocalizedDisplayName("ImportNoHeaders", NameResourceType = typeof(MultistepActionStrings))]
        public bool NoHeaders { get; set; }

        [LocalizedDisplayName("ImportAction", NameResourceType = typeof(MultistepActionStrings))]
        public int ImportAction { get; set; } = (int)CsvImportMode.InsertAndUpdate;

        public List<ListItem> ImportActions => new List<ListItem>
        {
            new ListItem(((int) CsvImportMode.InsertAll).ToString(), UserStrings.ArticlesInsertAll),
            new ListItem(((int) CsvImportMode.InsertNew).ToString(), UserStrings.ArticlesInsertNew),
            new ListItem(((int) CsvImportMode.InsertAndUpdate).ToString(), UserStrings.ArticlesInsertAndUpdate),
            new ListItem(((int) CsvImportMode.Update).ToString(), UserStrings.ArticlesUpdate),
            new ListItem(((int) CsvImportMode.UpdateIfChanged).ToString(), UserStrings.ArticlesUpdateIfChanged)
        };

        [LocalizedDisplayName("UniqueFieldToUpdate", NameResourceType = typeof(MultistepActionStrings))]
        public string UniqueFieldToUpdate { get; set; }

        [LocalizedDisplayName("UniqueContentFieldToUpdate", NameResourceType = typeof(MultistepActionStrings))]
        public string UniqueContentFieldId { get; set; }

        public List<ListItem> UniqueContentFieldsToUpdate
        {
            get
            {
                var content = ContentService.Read(ContentId);
                return new[]
                    {
                        new ListItem(string.Empty, FieldName.ContentItemId)
                    }
                    .Concat(content.Fields.Where(f => f.ExactType != FieldExactTypes.M2ORelation && f.IsUnique)
                        .Select(f => new ListItem(f.Id.ToString(), f.Name)))
                    .ToList();
            }
        }

        public BLL.Field UniqueContentField { get; set; }

        [LocalizedDisplayName("DownloadedFile", NameResourceType = typeof(MultistepActionStrings))]
        public string FileName { get; set; } = MultistepActionStrings.NoFile;

        public List<KeyValuePair<string, BLL.Field>> NewFieldsList { get; set; }

        public Dictionary<int, string> UniqueAggregatedFieldsToUpdate { get; set; }

        public void SetCorrespondingFieldName(FormCollection collection)
        {
            NewFieldsList = new List<KeyValuePair<string, BLL.Field>>();
            UniqueAggregatedFieldsToUpdate = new Dictionary<int, string>();
            foreach (var key in collection.AllKeys.Where(s => s.StartsWith(FieldPrefix)))
            {
                if (key.StartsWith(IdPrefix))
                {
                    if (int.TryParse(key.Replace(IdPrefix, string.Empty), out var contentId))
                    {
                        UniqueAggregatedFieldsToUpdate[contentId] = collection[key];
                    }
                }
                else if (key.StartsWith(FieldPrefix))
                {
                    if (int.TryParse(key.Replace(FieldPrefix, string.Empty), out var fieldId))
                    {
                        var field = FieldRepository.GetById(fieldId);
                        if (field != null)
                        {
                            NewFieldsList.Add(new KeyValuePair<string, BLL.Field>(collection[key], field));
                        }
                    }
                }
            }

            if (int.TryParse(UniqueContentFieldId, out var uniqueFieldId))
            {
                UniqueContentField = FieldRepository.GetById(uniqueFieldId);
            }
        }

        public ImportSettings GetImportSettingsObject(int parentId, int id) => new ImportSettings(parentId, id)
        {
            Culture = ((CsvCulture)int.Parse(Culture)).Description(),
            Delimiter = char.Parse(((CsvDelimiter)int.Parse(Delimiter)).Description()),
            Encoding = ((CsvEncoding)int.Parse(Encoding)).Description(),
            LineSeparator = ((CsvLineSeparator)int.Parse(LineSeparator)).Description(),
            FileName = FileName,
            UniqueFieldToUpdate = UniqueFieldToUpdate,
            UniqueContentField = UniqueContentField,
            NoHeaders = NoHeaders,
            ImportAction = ImportAction,
            FieldsList = NewFieldsList,
            UniqueAggregatedFieldsToUpdate = UniqueAggregatedFieldsToUpdate
        };

        public ImportFieldGroupViewModel FieldGroup
        {
            get
            {
                var content = ContentService.Read(ContentId);
                var fields = content.Fields.Where(f => f.ExactType != FieldExactTypes.M2ORelation);
                return Update(new ImportFieldGroupViewModel(MultistepActionStrings.MappingFields), fields);
            }
        }

        private static ImportFieldGroupViewModel Update(ImportFieldGroupViewModel groupModel, IEnumerable<BLL.Field> fields)
        {
            foreach (var field in fields)
            {
                var item = new ExtendedListItem
                {
                    Text = field.Name,
                    Value = field.Id.ToString(),
                    Description = field.Name,
                    Required = field.Required,
                    IsIdentifier = false,
                    IsAggregated = false,
                    Unique = field.IsUnique
                };

                if (field.IsClassifier)
                {
                    var classifierGroup = new ImportFieldGroupViewModel("Classifier");
                    classifierGroup.Fields.Add(item);

                    var contents = field.Content.AggregatedContents.Where(c => c.Fields.Any(f => f.Aggregated && f.ClassifierId == field.Id)).ToList();
                    var extensionFields = contents.SelectMany(c => c.Fields.Where(f => f.ExactType != FieldExactTypes.M2ORelation)).ToList();
                    foreach (var content in contents)
                    {
                        var contentExtensionFields = extensionFields.Where(ef => ef.ContentId == content.Id).ToList();
                        var contentGroup = UpdateExtension(new ImportFieldGroupViewModel(content.Name), contentExtensionFields);
                        classifierGroup.Groups.Add(contentGroup);
                    }

                    groupModel.Groups.Add(classifierGroup);
                }
                else if (!field.Aggregated)
                {
                    groupModel.Fields.Add(item);
                }
            }

            return groupModel;
        }

        private static ImportFieldGroupViewModel UpdateExtension(ImportFieldGroupViewModel groupModel, IList<BLL.Field> fields)
        {
            var content = fields.Select(f => f.Content).FirstOrDefault();
            if (content != null)
            {
                groupModel.Fields.Add(new ExtendedListItem
                {
                    Text = content.Name + "." + FieldName.ContentItemId,
                    Value = "Id_" + content.Id,
                    Description = "Id",
                    Required = false,
                    IsIdentifier = true,
                    IsAggregated = true
                });
            }

            foreach (var field in fields)
            {
                var item = new ExtendedListItem
                {
                    Text = $"{field.Content.Name}.{field.Name}",
                    Value = field.Id.ToString(),
                    Description = field.Name,
                    Required = field.Required,
                    IsIdentifier = false,
                    IsAggregated = true,
                    Unique = field.IsUnique
                };

                if (!field.Aggregated)
                {
                    groupModel.Fields.Add(item);
                }
            }

            return groupModel;
        }
    }

    public class ImportFieldGroupViewModel
    {
        public ImportFieldGroupViewModel(string name)
        {
            Name = name;
            Fields = new List<ExtendedListItem>();
            Groups = new List<ImportFieldGroupViewModel>();
        }

        public string Name { get; }

        public List<ExtendedListItem> Fields { get; }

        public List<ImportFieldGroupViewModel> Groups { get; }
    }
}
