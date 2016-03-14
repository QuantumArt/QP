using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using System.Globalization;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
	public class QPCheckedItemModelBinder : DefaultModelBinder
	{
		public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			string key = bindingContext.ModelName;
			var val = bindingContext.ValueProvider.GetValue(key);
			if (val != null && !string.IsNullOrEmpty(val.AttemptedValue))
			{																				
				string[] values = (string[])val.RawValue;
				try
				{
					bindingContext.ModelState.Remove(key);
					if (values.Length == 2)
					{
						bindingContext.ModelState.SetModelValue(key, new ValueProviderResult(new string[] { "true" }, "true", CultureInfo.InvariantCulture));
						return new QPCheckedItem { Value = values[0] };
					}
					else
					{
						bindingContext.ModelState.SetModelValue(key, val);				
						return null;
					}
				}
				catch (Exception exp)
				{
					bindingContext.ModelState.AddModelError(key, String.Format("{2} is not valid.{1}{0}", exp.Message, Environment.NewLine, key));
					return null;
				}
			}
			else
				return null;			
		}
	}
}