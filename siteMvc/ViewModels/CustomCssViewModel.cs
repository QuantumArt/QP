using System.Collections.Generic;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class CustomCssViewModel
    {
        public CustomCssViewModel(IEnumerable<BLL.StatusType> statuses)
        {
            Statuses = statuses;
        }

        public IEnumerable<BLL.StatusType> Statuses { get; }
    }
}
