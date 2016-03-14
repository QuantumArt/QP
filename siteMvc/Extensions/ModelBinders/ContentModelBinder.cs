using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL;
using System.Web.Mvc;
using System.Linq.Expressions;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
	public class ContentModelBinder : QpModelBinder
	{
		public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			Content content = base.BindModel(controllerContext, bindingContext) as Content;
			if (!content.UseVersionControl)
				bindingContext.ModelState.Remove(GetModelPropertyName(bindingContext, () => content.MaxNumOfStoredVersions));
			return content;
		}

		protected override void BindProperty(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext, System.ComponentModel.PropertyDescriptor propertyDescriptor)
		{
			Content model = bindingContext.Model as Content;
			Expression<Func<IEnumerable<int>>> p = () => model.UnionSourceContentIDs;
			if (propertyDescriptor.Name == p.GetPropertyName())
			{
				string value = controllerContext.HttpContext.Request[GetModelPropertyName(bindingContext, p)];
				model.UnionSourceContentIDs = Converter.ToIdArray(value);					
			}
			else
				base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
		}
		
		protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			Content content = (bindingContext.Model as Content);
			content.DoCustomBinding();
			base.OnModelUpdated(controllerContext, bindingContext);
		}
	}
}
