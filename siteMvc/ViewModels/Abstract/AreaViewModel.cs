using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public abstract class AreaViewModel : ViewModel
    {
        public override MainComponentType MainComponentType => MainComponentType.Area;

        public override string MainComponentId => UniqueId("Area");
    }
}
