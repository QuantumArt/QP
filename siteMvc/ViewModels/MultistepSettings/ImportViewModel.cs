using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Im = Quantumart.QP8.BLL.Services.MultistepActions.Import;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Con = Quantumart.QP8.Constants;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels
{
	public class ImportViewModel : ExportImportModel
	{
		#region Constants
		public const string FieldPrefix = "Field_";
		private const string IdPrefix = FieldPrefix + "Id_";
		#endregion

		public override string ActionCode
		{
			get
			{
				return Con.ActionCode.ImportArticles;
			}
		}

		public override string EntityTypeCode
		{
			get { return Con.EntityTypeCode.Content; }
		}

		#region Properties

		public int ContentId { get; set; }

		public UploaderType UploaderType
		{
			get
			{
				return UploaderTypeHelper.UploaderType;
			}
		}

		public bool AllowUpload { get { return true; } }

		[LocalizedDisplayName("ImportNoHeaders", NameResourceType = typeof(MultistepActionStrings))]
		public bool NoHeaders { get; set; }

		private int importAction = (int)Im.ImportActions.InsertAndUpdate;

		[LocalizedDisplayName("ImportAction", NameResourceType = typeof(MultistepActionStrings))]
		public int ImportAction
		{
			get
			{
				return importAction;
			}
			set
			{
				importAction = value;
			}
		}
		public List<ListItem> ImportActions
		{
			get
			{
				return new List<ListItem>()
				{
					new ListItem(((int)Im.ImportActions.InsertAll).ToString(), UserStrings.ArticlesInsertAll),
					new ListItem(((int)Im.ImportActions.InsertNew).ToString(), UserStrings.ArticlesInsertNew),
					new ListItem(((int)Im.ImportActions.InsertAndUpdate).ToString(), UserStrings.ArticlesInsertAndUpdate),
					new ListItem(((int)Im.ImportActions.Update).ToString(), UserStrings.ArticlesUpdate),
					new ListItem(((int)Im.ImportActions.UpdateIfChanged).ToString(), UserStrings.ArticlesUpdateIfChanged)
				};
			}
		}

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
					new ListItem[] { new ListItem(string.Empty, ArticleStrings.CONTENT_ITEM_ID) }
					.Concat(
					content.Fields
					.Where(f => f.ExactType != FieldExactTypes.M2ORelation && f.IsUnique)
					.Select(f => new ListItem(f.Id.ToString(), f.Name))
					).ToList();
			}
		}
		public BLL.Field UniqueContentField { get; set; }

		private string _downloadedFile = MultistepActionStrings.NoFile;

		[LocalizedDisplayName("DownloadedFile", NameResourceType = typeof(MultistepActionStrings))]
		public string FileName
		{
			get
			{
				return _downloadedFile;
			}
			set
			{
				_downloadedFile = value;
			}
		}

		public List<KeyValuePair<string, BLL.Field>> NewFieldsList { get; set; }
		public Dictionary<int, string> UniqueAggregatedFieldsToUpdate { get; set; }

		public void SetCorrespondingFieldName(FormCollection collection)
		{
			NewFieldsList = new List<KeyValuePair<string, BLL.Field>>();
			UniqueAggregatedFieldsToUpdate = new Dictionary<int, string>();

			foreach (string key in collection.AllKeys.Where(s => s.StartsWith(FieldPrefix)))
			{
				if (key.StartsWith(IdPrefix))
				{
					int contentId = 0;
					if (Int32.TryParse(key.Replace(IdPrefix, ""), out contentId))
					{
						UniqueAggregatedFieldsToUpdate[contentId] = collection[key];
					}
				}
				else if (key.StartsWith(FieldPrefix))
				{
					int fieldId = 0;
					if (Int32.TryParse(key.Replace(FieldPrefix, ""), out fieldId))
					{
						BLL.Field field = FieldService.GetById(fieldId);
						if (field != null)
							NewFieldsList.Add(new KeyValuePair<string, BLL.Field>(collection[key], field));
					}
				}
			}

			int uniqueFieldId;
			if (Int32.TryParse(UniqueContentFieldId, out uniqueFieldId))
			{
				UniqueContentField = FieldService.GetById(uniqueFieldId);
			}
		}

		public Im.ImportSettings GetImportSettingsObject(int parentId, int id)
		{
			return new Im.ImportSettings(parentId, id)
			{
				Culture = MultistepActionHelper.GetCulture(this.Culture),
				Delimiter = MultistepActionHelper.GetDelimiter(this.Delimiter),
				Encoding = MultistepActionHelper.GetEncoding(this.Encoding),
				LineSeparator = MultistepActionHelper.GetLineSeparator(this.LineSeparator),
				FileName = this.FileName,
				UniqueFieldToUpdate = this.UniqueFieldToUpdate,
				UniqueContentField = this.UniqueContentField,
				NoHeaders = this.NoHeaders,
				ImportAction = this.ImportAction,
				FieldsList = this.NewFieldsList,
                UniqueAggregatedFieldsToUpdate = this.UniqueAggregatedFieldsToUpdate
			};
		}

		public int BlockedFieldId { get; set; }
		#endregion

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
				var fields = content.Fields.Where(f => f.ExactType != FieldExactTypes.M2ORelation);
				Update(rootGroup, fields, false);
				return rootGroup;
			}
		}

		private void Update(ImportFieldGroupViewModel groupModel, IEnumerable<BLL.Field> fields, bool exstension)
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
				string text = exstension ? string.Format("{0}.{1}", field.Content.Name, field.Name) : field.Name;
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
						//Update(contentGroup, content.FieldGroups, true);
						var exstensionFields = content.Fields.Where(f => f.ExactType != FieldExactTypes.M2ORelation);
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

		private bool CheckFieldForDataIntegrity(BLL.Field field)
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
		public List<ExstendedListItem> Fields { get; private set; }
		public List<ImportFieldGroupViewModel> Groups { get; private set; }
	}

	public class ExstendedListItem : SimpleListItem
	{
		public string Description { get; set; }
		public bool Required { get; set; }
		public bool IsIdentifier { get; set; }
		public bool IsAggregated { get; set; }
		public bool Unique { get; set; }
		public bool BrokenDataIntegrity { get; set; }
	}
}