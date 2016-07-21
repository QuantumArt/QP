using Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public abstract class AreaViewModel : ViewModel
    {
        public override MainComponentType MainComponentType => MainComponentType.Area;

        public override string MainComponentId => UniqueId("Area");
    }
}
