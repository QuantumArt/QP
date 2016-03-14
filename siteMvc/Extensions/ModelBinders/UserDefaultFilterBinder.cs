using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
	public class UserDefaultFilterBinder : QpModelBinder
	{
		protected override void BindProperty(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext, System.ComponentModel.PropertyDescriptor propertyDescriptor)
		{
			UserDefaultFilter model = bindingContext.Model as UserDefaultFilter;
			Expression<Func<IEnumerable<int>>> p = () => model.ArticleIDs;

			if (propertyDescriptor.Name == p.GetPropertyName())
			{
				string value = controllerContext.HttpContext.Request[GetModelPropertyName(bindingContext, p)];
				model.ArticleIDs = Converter.ToIdArray(value);
			}
			else
				base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
		}
	}
}