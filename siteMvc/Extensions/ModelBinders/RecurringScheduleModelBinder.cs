using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class RecurringScheduleModelBinder : QpModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var item = base.BindModel(controllerContext, bindingContext) as RecurringSchedule;
            if (item.RepetitionNoEnd)
            {
                bindingContext.ModelState.Remove(GetModelPropertyName(bindingContext, () => item.RepetitionEndDate));
            }

            if (item.ShowLimitationType != ShowLimitationType.EndTime)
            {
                bindingContext.ModelState.Remove(GetModelPropertyName(bindingContext, () => item.ShowEndTime));
            }

            return item;
        }
    }
}
