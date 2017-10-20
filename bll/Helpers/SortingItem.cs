using System;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Helpers
{
	public class SortingItem
	{
		private static string[] delimeter = { "] " };
		private static char[] ecranSymb = { '[', ']' };
		private static string ascending = "asc";
		private static string descending = "desc";		

		public string Order { get; set; }

		public string Field { get; set; }

		public bool Invalid { get; set; }

		/// <summary>
		/// Создает экземпляр SortingItem из строки
		/// </summary>
		/// <param name="inputString">строка формата "[Name] asc/desc"</param>
		/// <returns></returns>
		public static SortingItem CreatefromString(string inputString)
		{
			var result = new SortingItem();
			var parts = inputString.Contains("[") && inputString.Contains("]") ? inputString.Split(delimeter, StringSplitOptions.None) : inputString.Split(' ');
			
			if ((parts[0] == null || parts[1] == null) || (parts[1] != ascending && parts[1] != descending))
			{
				result.Invalid = true;
				return result;
			}

			result.Field = parts[0].Trim(ecranSymb);
			result.Order = parts[1] == ascending ? TemplateStrings.Ascending : TemplateStrings.Descending;

			return result;
		}

		public string ShortSortingOrderName => Order == TemplateStrings.Ascending ? ascending : descending;
	}
}
