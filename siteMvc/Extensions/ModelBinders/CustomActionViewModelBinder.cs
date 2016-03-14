using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.WebMvc.ViewModels.CustomAction;
using System.Linq.Expressions;
using Quantumart.QP8.Utils;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
	public class CustomActionViewModelBinder : QpModelBinder
	{
		protected override void BindProperty(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext, System.ComponentModel.PropertyDescriptor propertyDescriptor)
		{
			CustomActionViewModel model = bindingContext.Model as CustomActionViewModel;
			
			Expression<Func<IEnumerable<int>>> siteIDs = () => model.SelectedSiteIDs;
			Expression<Func<IEnumerable<int>>> contentIDs = () => model.SelectedContentIDs;

			if (propertyDescriptor.Name == siteIDs.GetPropertyName())
			{
				string value = controllerContext.HttpContext.Request[GetModelPropertyName(bindingContext, siteIDs)];
				model.SelectedSiteIDs = Converter.ToIdArray(value);
			}
			else if (propertyDescriptor.Name == contentIDs.GetPropertyName())
			{
				string value = controllerContext.HttpContext.Request[GetModelPropertyName(bindingContext, contentIDs)];
				model.SelectedContentIDs = Converter.ToIdArray(value);
			}
			else
				base.BindProperty(controllerContext, bindingContext, propertyDescriptor);			
		}

		protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			CustomActionViewModel model = bindingContext.Model as CustomActionViewModel;
			model.DoCustomBinding();
			base.OnModelUpdated(controllerContext, bindingContext);
		}
	}
}