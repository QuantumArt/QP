using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
	/// <summary>
	/// Custom Model Binder для десериализации из строки с JSON
	/// </summary>
	public class JsonStringModelBinder<T> : IModelBinder
	{
		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			string key = bindingContext.ModelName;
			var val = bindingContext.ValueProvider.GetValue(key);
			if (val != null && !string.IsNullOrEmpty(val.AttemptedValue))
			{
				bindingContext.ModelState.SetModelValue(key, val);

				string incomingString = null;
				if (val.RawValue is string[])
					incomingString = ((string[])val.RawValue)[0];
				else if (val.RawValue is string)
					incomingString = (string)val.RawValue;
				else
					throw new InvalidCastException(key + " is not string[] or string");

				try
				{
					return new JavaScriptSerializer().Deserialize<T>(incomingString);
				}
				catch(Exception exp)
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