using Microsoft.AspNetCore.Mvc;

namespace Quantumart.QP8.WebMvc.ViewModels.CustomAction
{
    public class ProxyViewModel
    {
        [ModelBinder(Name="url")]
        public string Url { get; set; }

        [ModelBinder(Name="actionCode")]
        public string ActionCode { get; set; }

        [ModelBinder(Name="level")]
        public int Level { get; set; }

        [ModelBinder(Name="ids")]
        public int[] Ids { get; set; }

        [ModelBinder(Name="parentEntityId")]
        public int? ParentEntityId { get; set; }
    }
}
