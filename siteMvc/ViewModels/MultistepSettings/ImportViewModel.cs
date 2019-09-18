using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Enums.Csv;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Services.ContentServices;
using Quantumart.QP8.BLL.Services.MultistepActions.Import;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.WebMvc.ViewModels.MultistepSettings
{
    public class ImportViewModel : ExportImportModel
    {
        public const string FieldPrefix = "Field_";
        private const string IdPrefix = FieldPrefix + "Id_";

        public override string ActionCode => Constants.ActionCode.ImportArticles;

        public override string EntityTypeCode => Constants.EntityTypeCode.Content;

        public int ContentId { get; set; }

        public bool AllowUpload => true;

        [Display(Name = "ImportNoHeaders", ResourceType = typeof(MultistepActionStrings))]
        public bool NoHeaders { get; set; }

        [Display(Name = "ImportAction", ResourceType = typeof(MultistepActionStrings))]
        public int ImportAction { get; set; } = (int)CsvImportMode.InsertAndUpdate;

        public List<ListItem> ImportActions => new List<ListItem>
        {
            new ListItem(((int)CsvImportMode.InsertAll).ToString(), UserStrings.ArticlesInsertAll),
            new ListItem(((int)CsvImportMode.InsertNew).ToString(), UserStrings.ArticlesInsertNew),
            new ListItem(((int)CsvImportMode.InsertAndUpdate).ToString(), UserStrings.ArticlesInsertAndUpdate),
            new ListItem(((int)CsvImportMode.Update).ToString(), UserStrings.ArticlesUpdate),
            new ListItem(((int)CsvImportMode.UpdateIfChanged).ToString(), UserStrings.ArticlesUpdateIfChanged)
        };

        [Display(Name = "UniqueFieldToUpdate", ResourceType = typeof(MultistepActionStrings))]
        public string UniqueFieldToUpdate { get; set; }

        [Display(Name = "UniqueContentFieldToUpdate", ResourceType = typeof(MultistepActionStrings))]
        public string UniqueContentFieldId { get; set; }

        [ValidateNever]
        [BindNever]
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

        [ValidateNever]
        [BindNever]
        public BLL.Field UniqueContentField { get; set; }

        [Display(Name = "DownloadedFile", ResourceType = typeof(MultistepActionStrings))]
        public string FileName { get; set; } = MultistepActionStrings.NoFile;

        [ValidateNever]
        [BindNever]
        public List<KeyValuePair<string, int>> NewFieldsList { get; set; }

        public Dictionary<int, string> UniqueAggregatedFieldsToUpdate { get; set; }

        public void SetCorrespondingFieldName(IFormCollection collection)
        {
            NewFieldsList = new List<KeyValuePair<string, int>>();
            UniqueAggregatedFieldsToUpdate = new Dictionary<int, string>();
            foreach (var key in collection.Keys.Where(s => s.StartsWith(FieldPrefix)))
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
                        NewFieldsList.Add(new KeyValuePair<string, int>(collection[key], fieldId));
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
            UniqueContentFieldId = UniqueContentField?.Id ?? 0,
            NoHeaders = NoHeaders,
            ImportAction = ImportAction,
            FieldsList = NewFieldsList,
            UniqueAggregatedFieldsToUpdate = UniqueAggregatedFieldsToUpdate
        };

        [ValidateNever]
        [BindNever]
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

        [ValidateNever]
        public List<ImportFieldGroupViewModel> Groups { get; }
    }
}
