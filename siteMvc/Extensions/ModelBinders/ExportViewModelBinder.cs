using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.WebMvc.ViewModels.CustomAction;
using System.Linq.Expressions;
using Quantumart.QP8.Utils;
using System.Web.Mvc;
using Quantumart.QP8.WebMvc.ViewModels;
using System.ComponentModel;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
	public class ExportViewModelBinder : QpModelBinder
	{
		protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
		{
			ExportViewModel model = bindingContext.Model as ExportViewModel;

			Expression<Func<IEnumerable<int>>> fieldsToExpand = () => model.FieldsToExpand;
			Expression<Func<IEnumerable<int>>> customFields = () => model.CustomFields;

			if (propertyDescriptor.Name == fieldsToExpand.GetPropertyName())
			{
				string value = controllerContext.HttpContext.Request[GetModelPropertyName(bindingContext, fieldsToExpand)];
				model.FieldsToExpand = Converter.ToIdArray(value);
			}
			else if (propertyDescriptor.Name == customFields.GetPropertyName())
			{
				string value = controllerContext.HttpContext.Request[GetModelPropertyName(bindingContext, customFields)];
				model.CustomFields = Converter.ToIdArray(value);
			}
			else
				base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
		}

	}
}