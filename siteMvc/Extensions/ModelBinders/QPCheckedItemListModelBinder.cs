using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class QpCheckedItemListModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var result = base.BindModel(controllerContext, bindingContext) as IList<QPCheckedItem>;
            return result?.Where(i => i != null).ToList();
        }
    }
}
