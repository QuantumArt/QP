using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.WebMvc.ViewModels.MultistepSettings
{
    public class ExportViewModel : ExportImportModel
    {
        public ExportViewModel()
        {
            AllFields = true;
        }

        public bool IsArchive { get; set; }

        public override string ActionCode => IsArchive ? Constants.ActionCode.ExportArchiveArticles : Constants.ActionCode.ExportArticles;

        public override string EntityTypeCode => Constants.EntityTypeCode.Content;

        public int ContentId { get; set; }

        [Display(Name = "OrderByField", ResourceType = typeof(MultistepActionStrings))]
        public string OrderByField { get; set; }

        public List<ListItem> Fields => ArticleService.GetListOfFieldsToSort(ContentId);

        public string CustomFieldsElementId => UniqueId("customFieldsPicker");

        public string FieldsToExpandElementId => UniqueId("fieldsToExpandPicker");

        public int[] Ids { get; set; }

        [Display(Name = "ExcludeSystemFields", ResourceType = typeof(ImportStrings))]
        public bool ExcludeSystemFields { get; set; }

        [Display(Name = "AllFields", ResourceType = typeof(ImportStrings))]
        public bool AllFields { get; set; }

        [Display(Name = "CustomFields", ResourceType = typeof(ImportStrings))]
        public int[] CustomFields { get; set; }

        [Display(Name = "FieldsToExpand", ResourceType = typeof(ImportStrings))]
        public int[] FieldsToExpand { get; set; }
    }
}
