using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.ComponentModel;
using System.Globalization;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Resources;
using System.Linq.Expressions;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
	public class QpModelBinder : DefaultModelBinder
	{
	   
		protected override void SetProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, System.ComponentModel.PropertyDescriptor propertyDescriptor, object value)
		{
			if (propertyDescriptor.PropertyType == typeof(string))
			{
				string stringValue = (string)value;
				stringValue = (!String.IsNullOrEmpty(stringValue)) ? stringValue.Trim() : stringValue;
				value = (String.IsNullOrEmpty(stringValue)) ? null : stringValue;
			}
			base.SetProperty(controllerContext, bindingContext, propertyDescriptor, value);
		}

		public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{							
			var val = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
			if (val != null)
			{
				var attemptedValue = val.AttemptedValue;
				bindingContext.ModelState.SetModelValue(bindingContext.ModelName, val);

				if (!ValidateFormat(bindingContext.ModelType, attemptedValue))					
				{
					bindingContext.ModelState.AddModelError(bindingContext.ModelName, String.Format(ArticleStrings.InvalidDateTimeFormat, bindingContext.ModelMetadata.DisplayName));
					return bindingContext.Model;
				}
				else
				{
					if (bindingContext.ModelType == typeof(TimeSpan) || bindingContext.ModelType == typeof(TimeSpan?))
					{
						DateTime? dt = Converter.ToNullableDateTime(attemptedValue);
						if (dt.HasValue)
							return dt.Value.TimeOfDay;
						else if (bindingContext.ModelType == typeof(TimeSpan?))
							return null;
					}
				}
			}
				
			return base.BindModel(controllerContext, bindingContext);
		}

		protected bool ValidateFormat(Type type, string attemptedValue)
		{
			if (
				(type == typeof(DateTime) && !Converter.CanParse<DateTime>(attemptedValue)) ||
				(type == typeof(DateTime?) && !String.IsNullOrEmpty(attemptedValue) && !Converter.CanParse<DateTime>(attemptedValue)) ||
				(type == typeof(TimeSpan) && !Converter.CanParse<TimeSpan>(attemptedValue)) ||
				(type == typeof(TimeSpan?) && !String.IsNullOrEmpty(attemptedValue) && !Converter.CanParse<TimeSpan>(attemptedValue)) 
			)
			{
				return false;
			}
			else if (type == typeof(int) && !Converter.CanParse<int>(attemptedValue))
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		/// <summary>
		/// Получить полное имя свойства в моделе
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="bindingContext"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		protected string GetModelPropertyName<T>(ModelBindingContext bindingContext, Expression<Func<T>> e)
		{
			if (String.IsNullOrWhiteSpace(bindingContext.ModelName))
				return e.GetPropertyName();
			else
				return String.Concat(bindingContext.ModelName, ".", e.GetPropertyName());
		}
	}
}
