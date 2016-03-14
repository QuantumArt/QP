using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Export
{
    public class ExportSettings : IMultistepActionParams
    {
        private int siteId { get; set; }
        private int[] ids { get; set; }
        private static DateTime dateForFileName;

        public ExportSettings(int siteId, int[] ids)
        {
            dateForFileName = DateTime.Now;
            this.siteId = siteId;
            this.ids = ids;
			fieldsToExpand = new Lazy<Field[]>(() => GetFieldsToExpand());
			fieldsToExpandSettings = new Lazy<IEnumerable<FieldSetting>>(() => GetFieldsToExpandSettings()); 

        }

        public int ContentId { get; set; }

        public string Encoding { get; set; }

        public string Culture { get; set; }

        public char Delimiter { get; set; }

        public string LineSeparator { get; set; }

		public bool AllFields { get; set; }

		public bool ExcludeSystemFields { get; set; }
		
		public int[] CustomFieldIds { get; set; }

		public int[] FieldIdsToExpand { get; set; }

		private Lazy<Field[]> fieldsToExpand;

		private Lazy<IEnumerable<FieldSetting>> fieldsToExpandSettings;
 
		public Field[] FieldsToExpand
		{
			get
			{
				return fieldsToExpand.Value;
			}
		}

		public IEnumerable<FieldSetting> FieldsToExpandSettings
		{
			get
			{
				return fieldsToExpandSettings.Value;
			}
		}

        private string orderByField = String.Empty;
        public string OrderByField
        {
            get
            {
                return orderByField;
            }
            set
            {
                orderByField = value.Replace("ID", "content_item_id");
            }
        }
        public string UploadFilePath
        {
            get
            {
                string fileName = String.Format("content_{0}_{1}_{2}_{3}_{4}_{5}_{6}.csv",
                                                                                      this.ContentId,
                                                                                      dateForFileName.Year,
                                                                                      dateForFileName.Month,
                                                                                      dateForFileName.Day,
                                                                                      dateForFileName.Hour,
                                                                                      dateForFileName.Minute,
                                                                                      dateForFileName.Second);

                return String.Format("{0}\\{1}", QPConfiguration.TempDirectory, fileName);
            }
        }
		public string[] FieldNames { get; set; }
		public string[] HeaderNames { get; set; }
		public string Exstensions { get; set; }

		private Field[] GetFieldsToExpand()
		{
			return (FieldIdsToExpand != null && FieldIdsToExpand.Any()) ? FieldRepository.GetList(FieldIdsToExpand).ToArray() : new Field[] { };
		}

		private IEnumerable<FieldSetting> GetFieldsToExpandSettings()
		{
			return FieldsToExpand.Select(n => new {
				Field = n,
				DisplayField = GetDisplayField(n),
				DisplayFields = ContentRepository.GetDisplayFields(n.RelateToContentId.Value, n).Select(rm => new
				{
					Field = rm,
                    DisplayField = GetDisplayField(rm)
				})
			}).Select(
				(n, i) => new FieldSetting(n.Field, i + 1, n.DisplayField)
				{
					Related = n.DisplayFields.Select((m, j) => new FieldSetting(m.Field, (i + 1) * 100 + j + 1, m.DisplayField)).ToList()
				}
			).ToArray();
		}

		private Field GetDisplayField(Field n)
		{
			if (n.ExactType == FieldExactTypes.M2MRelation)
				return ContentRepository.GetTitleField(n.RelateToContentId.Value);
			else if (n.ExactType == FieldExactTypes.O2MRelation)
				return n.Relation;
			else
				return null;
        }

		public class FieldSetting
		{
			public int Id { get; set; }
			public int ContentId { get; set; }
			public string Name { get; set; }
			public int Order { get; set; }
			public int RelatedContentId { get; set; }
			public string RelatedContentName { get; set; }

			public string RelatedAttributeName { get; set; }
			public int RelatedAttributeId { get; set; }

			public FieldSetting()
			{

			}
            public FieldSetting(Field field, int order, Field displayField)
			{
				Id = field.Id;
				ContentId = field.ContentId;
				Name = field.Name;
				Order = order;
				LinkId = field.LinkId ?? 0;
				ExactType = field.ExactType;
				RelatedContentId = (displayField != null) ? displayField.ContentId : 0;
				RelatedContentName = (displayField != null) ? displayField.Content.Name : null;
				RelatedAttributeName = (displayField != null) ? displayField.Name : null;
				RelatedAttributeId = (displayField != null) ? displayField.Id : 0;
            }
			public string Alias
			{
				get
				{
					return String.Format("rel_{0}_{1}", Order, RelatedContentId);
				}
			}

			public string TableAlias
			{
				get
				{
					return String.Format("rel_{0}", Order, RelatedContentId);
				}
			}

			public bool IsM2M
			{
				get
				{
					return ExactType == Constants.FieldExactTypes.M2MRelation;
                }
			}

			public string CsvColumnName
			{
				get
				{
					return String.Format("{0}.{1}", RelatedContentName, Related.Count() > 1 ? "Title" : RelatedAttributeName);
				}
			}

			public int? LinkId { get; set; }
			public IEnumerable<FieldSetting> Related { get; set; }

			public FieldExactTypes ExactType { get; set; }
			
		}

    }
}
