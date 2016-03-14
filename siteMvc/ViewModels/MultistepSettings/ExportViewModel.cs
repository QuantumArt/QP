using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using System.Collections.Generic;
using System.Linq;
using Con = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class ExportViewModel : ExportImportModel
    {
		public ExportViewModel()
		{
			AllFields = true;
		}

		public override string ActionCode => Con.ActionCode.ExportArticles;

        public override string EntityTypeCode => Con.EntityTypeCode.Content;

        public int ContentId { get; set; }

        #region Properties

        [LocalizedDisplayName("OrderByField", NameResourceType = typeof(MultistepActionStrings))]
        public string OrderByField { get; set; }

        public List<ListItem> Fields => ArticleService.GetListOfFieldsToSort(ContentId);

        public string CustomFieldsElementId => UniqueId("customFieldsPicker");

        public string FieldsToExpandElementId => UniqueId("fieldsToExpandPicker");

        public int[] Ids { get; set; }

		[LocalizedDisplayName("ExcludeSystemFields", NameResourceType = typeof(ImportStrings))]
		public bool ExcludeSystemFields { get; set; }

		[LocalizedDisplayName("AllFields", NameResourceType = typeof(ImportStrings))]
		public bool AllFields { get; set; }

		[LocalizedDisplayName("CustomFields", NameResourceType = typeof(ImportStrings))]
		public IEnumerable<int> CustomFields { get; set; }

		[LocalizedDisplayName("FieldsToExpand", NameResourceType = typeof(ImportStrings))]
		public IEnumerable<int> FieldsToExpand { get; set; }
        #endregion

		public new IEnumerable<ListItem> GetList(IEnumerable<int> ids)
		{
			var result = Enumerable.Empty<ListItem>();
		    if (ids != null && ids.Any())
		    {
		        result = FieldService.GetList(ids.ToArray()).Select(s => new ListItem(s.Id.ToString(), s.Name)).ToArray();
		    }

		    return result;
		}
	}
}
