using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Quantumart.QP8.WebMvc.ViewModels.Workflow;
using System.Text;
using Quantumart.QP8.BLL;
using System.Web.Mvc;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
	public class WorkflowModelBinder : QpModelBinder
	{
		protected override void BindProperty(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext, System.ComponentModel.PropertyDescriptor propertyDescriptor)
		{
			WorkflowViewModel model = bindingContext.Model as WorkflowViewModel;
			Expression<Func<IEnumerable<int>>> p = () => model.ActiveContentsIds;
			if (propertyDescriptor.Name == p.GetPropertyName())
			{
				string value = controllerContext.HttpContext.Request[GetModelPropertyName(bindingContext, p)];
				model.ActiveContentsIds = Converter.ToIdArray(value);
			}
			else
				base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
		}

		protected override void OnModelUpdated(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext)
		{
			WorkflowViewModel model = bindingContext.Model as WorkflowViewModel;
			model.DoCustomBinding();
			base.OnModelUpdated(controllerContext, bindingContext);
		}
	}
}