using Microsoft.AspNetCore.Mvc;

namespace Quantumart.QP8.WebMvc.ViewModels.Article
{
    public class RelationSearchViewModel
    {
        [BindProperty(Name="parentEntityId")]
        public int ParentEntityId { get; set; }

        [BindProperty(Name="fieldID")]
        public int FieldId { get; set; }

        [BindProperty(Name="elementIdPrefix")]
        public string ElementIdPrefix { get; set; }

        [BindProperty(Name="IDs")]
        public int[] Ids { get; set; }
    }
}
