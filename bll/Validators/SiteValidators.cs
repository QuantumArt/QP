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
            {
                return !string.IsNullOrEmpty(objectToValidate);
            }

		    return true;
		}
	}
}
