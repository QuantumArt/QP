using System.Collections.Generic;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.WebMvc.ViewModels.MultistepSettings
{
    public class ExportViewModel : ExportImportModel
    {
        public ExportViewModel()
        {
            AllFields = true;
        }

        public override string ActionCode => Constants.ActionCode.ExportArticles;

        public override string EntityTypeCode => Constants.EntityTypeCode.Content;

        public int ContentId { get; set; }

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
    }
}
