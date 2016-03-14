using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.Practices.EnterpriseLibrary.Validation.Properties;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL.Validators
{
	public static class SiteValidators
	{
		/// <summary>
		/// Проверяет ввод DNS для тестового режима
		/// </summary>
		/// <param name="objectToValidate">DNS для тестового режима</param>
		/// <param name="currentTarget">объект Site</param>
		/// <returns>результат проверки (true - DNS введен; false - не введен)</returns>
		public static bool ValidateStageDnsInput(string objectToValidate, Site currentTarget, out string errorMessage)
		{
            errorMessage = null;
            if (currentTarget.SeparateDns)
                return !String.IsNullOrEmpty(objectToValidate);
            else
                return true;
		}
	}
}
