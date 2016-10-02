using System;
using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class ArticleScheduleModelBinder : QpModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var item = base.BindModel(controllerContext, bindingContext) as ArticleSchedule;

            // Если это не повторяющееся событие, то удалить все сообщения связанные с item.Recurring
            if (item.ScheduleType != ScheduleTypeEnum.Recurring)
            {
                var recuringPrefix = GetModelPropertyName(bindingContext, () => item.Recurring);
                var keys = bindingContext.ModelState.Keys.Where(k => k.StartsWith(recuringPrefix)).ToArray();
                foreach (var key in keys)
                {
                    bindingContext.ModelState.Remove(key);
                }
            }

            if (item.ScheduleType != ScheduleTypeEnum.OneTimeEvent)
            {
                bindingContext.ModelState.Remove(GetModelPropertyName(bindingContext, () => item.StartDate));
                bindingContext.ModelState.Remove(GetModelPropertyName(bindingContext, () => item.EndDate));
            }
            else
            {
                if (item.StartRightNow)
                {
                    bindingContext.ModelState.Remove(GetModelPropertyName(bindingContext, () => item.StartDate));
                }

                if (item.WithoutEndDate)
                {
                    bindingContext.ModelState.Remove(GetModelPropertyName(bindingContext, () => item.EndDate));
                }
            }

            return item;
        }

        protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var item = bindingContext.Model as ArticleSchedule;
            if (item.StartRightNow && item.WithoutEndDate)
            {
                item.ScheduleType = ScheduleTypeEnum.Visible;
            }

            if (item.Article.Delayed)
            {
                item.StartDate = item.PublicationDate;
            }

            if (item.WithoutEndDate || item.Article.Delayed)
            {
                item.EndDate = ArticleScheduleConstants.Infinity;
            }

            if (item.StartDate < DateTime.Now || item.StartRightNow)
            {
                item.StartDate = DateTime.Now.AddSeconds(10);
            }

            base.OnModelUpdated(controllerContext, bindingContext);
        }
    }
}
