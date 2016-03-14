using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.WebMvc.ViewModels;
using System.Linq.Expressions;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
	public class UserGroupViewModelBinder : QpModelBinder
	{
		protected override void BindProperty(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext, System.ComponentModel.PropertyDescriptor propertyDescriptor)
		{
			UserGroupViewModel model = bindingContext.Model as UserGroupViewModel;
			Expression<Func<IEnumerable<int>>> bindedUserIDs = () => model.BindedUserIDs;

			if (propertyDescriptor.Name == bindedUserIDs.GetPropertyName())
			{
				string value = controllerContext.HttpContext.Request[GetModelPropertyName(bindingContext, bindedUserIDs)];
				model.BindedUserIDs = Converter.ToIdArray(value);
			}
			else
				base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
		}
		protected override void OnModelUpdated(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext)
		{
			UserGroupViewModel model = bindingContext.Model as UserGroupViewModel;
			model.DoCustomBinding();
			base.OnModelUpdated(controllerContext, bindingContext);
		}
	}
}