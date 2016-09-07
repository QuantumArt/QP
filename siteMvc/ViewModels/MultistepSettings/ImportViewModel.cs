using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.MultistepSettings;

namespace Quantumart.QP8.WebMvc.ViewModels
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
        public int ImportAction { get; set; } = (int)BLL.Enums.ImportActions.InsertAndUpdate;

        public List<ListItem> ImportActions => new List<ListItem>
        {
            new ListItem(((int)BLL.Enums.ImportActions.InsertAll).ToString(), UserStrings.ArticlesInsertAll),
            new ListItem(((int)BLL.Enums.ImportActions.InsertNew).ToString(), UserStrings.ArticlesInsertNew),
            new ListItem(((int)BLL.Enums.ImportActions.InsertAndUpdate).ToString(), UserStrings.ArticlesInsertAndUpdate),
            new ListItem(((int)BLL.Enums.ImportActions.Update).ToString(), UserStrings.ArticlesUpdate),
            new ListItem(((int)BLL.Enums.ImportActions.UpdateIfChanged).ToString(), UserStrings.ArticlesUpdateIfChanged)
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
                return
                    new[] { new ListItem(string.Empty, ArticleStrings.CONTENT_ITEM_ID) }
                    .Concat(
                    content.Fields
                    .Where(f => f.ExactType != Constants.FieldExactTypes.M2ORelation && f.IsUnique)
                    .Select(f => new ListItem(f.Id.ToString(), f.Name))
                    ).ToList();
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
                    int contentId;
                    if (int.TryParse(key.Replace(IdPrefix, ""), out contentId))
                    {
                        UniqueAggregatedFieldsToUpdate[contentId] = collection[key];
                    }
                }
                else if (key.StartsWith(FieldPrefix))
                {
                    int fieldId;
                    if (int.TryParse(key.Replace(FieldPrefix, ""), out fieldId))
                    {
                        var field = FieldRepository.GetById(fieldId);
                        if (field != null)
                        {
                            NewFieldsList.Add(new KeyValuePair<string, BLL.Field>(collection[key], field));
                        }
                    }
                }
            }

            int uniqueFieldId;
            if (int.TryParse(UniqueContentFieldId, out uniqueFieldId))
            {
                UniqueContentField = FieldRepository.GetById(uniqueFieldId);
            }
        }

        public BLL.Services.MultistepActions.Import.ImportSettings GetImportSettingsObject(int parentId, int id)
        {
            return new BLL.Services.MultistepActions.Import.ImportSettings(parentId, id)
            {
                Culture = MultistepActionHelper.GetCulture(Culture),
                Delimiter = MultistepActionHelper.GetDelimiter(Delimiter),
                Encoding = MultistepActionHelper.GetEncoding(Encoding),
                LineSeparator = MultistepActionHelper.GetLineSeparator(LineSeparator),
                FileName = FileName,
                UniqueFieldToUpdate = UniqueFieldToUpdate,
                UniqueContentField = UniqueContentField,
                NoHeaders = NoHeaders,
                ImportAction = ImportAction,
                FieldsList = NewFieldsList,
                UniqueAggregatedFieldsToUpdate = UniqueAggregatedFieldsToUpdate
            };
        }

        public int BlockedFieldId { get; set; }

        public IEnumerable<ListItem> FieldsList
        {
            get
            {
                return ArticleService.GetListOfFieldsForImport(ContentId).Where(n => n.Value != BlockedFieldId.ToString());
            }
        }

        public ImportFieldGroupViewModel FieldGroup
        {
            get
            {
                var rootGroup = new ImportFieldGroupViewModel(MultistepActionStrings.MappingFields);
                var content = ContentService.Read(ContentId);
                var fields = content.Fields.Where(f => f.ExactType != Constants.FieldExactTypes.M2ORelation).ToList();
                Update(rootGroup, fields, false);

                return rootGroup;
            }
        }

        private static void Update(ImportFieldGroupViewModel groupModel, IList<BLL.Field> fields, bool exstension)
        {
            if (exstension)
            {
                var content = fields.Select(f => f.Content).FirstOrDefault();

                if (content != null)
                {
                    groupModel.Fields.Add(new ExstendedListItem
                    {
                        Text = content.Name + ".CONTENT_ITEM_ID",
                        Value = "Id_" + content.Id,
                        Description = "Id",
                        Required = false,
                        IsIdentifier = true,
                        IsAggregated = true
                    });
                }
            }

            foreach (var field in fields)
            {
                var text = exstension ? $"{field.Content.Name}.{field.Name}" : field.Name;
                var item = new ExstendedListItem
                {
                    Text = text,
                    Value = field.Id.ToString(),
                    Description = field.Name,
                    Required = field.Required,
                    IsIdentifier = false,
                    IsAggregated = exstension,
                    Unique = field.IsUnique,
                    BrokenDataIntegrity = !CheckFieldForDataIntegrity(field)
                };

                if (field.IsClassifier)
                {
                    var classifierGroup = new ImportFieldGroupViewModel("Classifier");
                    var contents = field.Content.AggregatedContents.Where(c => c.Fields.Any(f => f.Aggregated && f.ClassifierId == field.Id));
                    foreach (var content in contents)
                    {
                        var contentGroup = new ImportFieldGroupViewModel(content.Name);
                        var exstensionFields = content.Fields.Where(f => f.ExactType != Constants.FieldExactTypes.M2ORelation).ToList();
                        Update(contentGroup, exstensionFields, true);
                        classifierGroup.Groups.Add(contentGroup);
                    }

                    classifierGroup.Fields.Add(item);
                    groupModel.Groups.Add(classifierGroup);
                }
                else if (!field.Aggregated)
                {
                    groupModel.Fields.Add(item);
                }
            }
        }

        private static bool CheckFieldForDataIntegrity(BLL.Field field)
        {
            try
            {
                field.Validate();
            }
            catch
            {
                return false;
            }

            return true;
        }
    }

    public class ImportFieldGroupViewModel
    {
        public ImportFieldGroupViewModel(string name)
        {
            Name = name;
            Fields = new List<ExstendedListItem>();
            Groups = new List<ImportFieldGroupViewModel>();
        }

        public string Name { get; private set; }

        public List<ExstendedListItem> Fields { get; }

        public List<ImportFieldGroupViewModel> Groups { get; }
    }
}
