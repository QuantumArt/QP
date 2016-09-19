using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class RecurringScheduleModelBinder : QpModelBinder
    {
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var item = base.BindModel(controllerContext, bindingContext) as RecurringSchedule;

            // Для тех полей который скрыты - удалить сообщения об ошибках форматирования
            if (item.RepetitionNoEnd)
            {
                bindingContext.ModelState.Remove(GetModelPropertyName(bindingContext, () => item.RepetitionEndDate));
            }

            if (item.ShowLimitationType != Constants.ShowLimitationType.EndTime)
            {
                bindingContext.ModelState.Remove(GetModelPropertyName(bindingContext, () => item.ShowEndTime));
            }

            return item;
        }
    }
}
