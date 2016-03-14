using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.ViewModels.Field;
using System.Linq.Expressions;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
	public class FieldViewModelBinder : QpModelBinder
	{
		protected override void BindProperty(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext, System.ComponentModel.PropertyDescriptor propertyDescriptor)
		{
			FieldViewModel model = bindingContext.Model as FieldViewModel;
			Expression<Func<IEnumerable<int>>> p = () => model.DefaultArticleIds;
			if (propertyDescriptor.Name == p.GetPropertyName())
			{
				string value = controllerContext.HttpContext.Request[GetModelPropertyName(bindingContext, p)];
				model.DefaultArticleIds = Converter.ToIdArray(value);
			}
			else
				base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
		}

		protected override void OnModelUpdated(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext)
		{
			base.OnModelUpdated(controllerContext, bindingContext);
			FieldViewModel model = bindingContext.Model as FieldViewModel;
			model.DoCustomBinding();
			model.Update();
		}
	}
}