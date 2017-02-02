using Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.ViewModels.MultistepSettings
{
    public class ExtendedListItem : SimpleListItem
    {
        public string Description { get; set; }

        public bool Required { get; set; }

        public bool IsIdentifier { get; set; }

        public bool IsAggregated { get; set; }

        public bool Unique { get; set; }

        public bool BrokenDataIntegrity { get; set; }
    }
}
