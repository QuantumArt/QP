using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Resources;
using System.Text.RegularExpressions;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Helpers
{
	public static class ExternalCssHelper
	{

		public static readonly string Delimiter = ";";
		
		/// <summary>
		/// Парсит строку и возвращает лист ExternalCss
		/// </summary>
		/// <param name="extCss">строка ExternalCss</param>
		/// <returns></returns>
		public static List<ExternalCss> GenerateExternalCss(string extCss)
		{
			return string.IsNullOrEmpty(extCss) ? new List<ExternalCss>() : extCss.Split(Delimiter.ToCharArray()).Where(s => !string.IsNullOrEmpty(s)).Select(x => new ExternalCss { Url = x, Invalid = false }).ToList();
		}

		/// <summary>
		/// Создает строку из листа ExternalCss
		/// </summary>
		/// <param name="jsonCss">лист ExternalCss</param>
		/// <returns></returns>
		public static string ConvertToString(IEnumerable<ExternalCss> jsonCss)
		{
			return string.Join(Delimiter, jsonCss.Select(x => x.Url).ToArray());
		}

		/// <summary>
		/// Принимает 2 строки и возвращает лист ExternalCss из непустой строки
		/// </summary>
		/// <param name="parentExtCss"></param>
		/// <param name="priorExtCss"></param>
		/// <returns></returns>
		public static List<ExternalCss> GenerateExternalCss(string parentExtCss, string priorExtCss)
		{
			return string.IsNullOrEmpty(priorExtCss) ? GenerateExternalCss(parentExtCss) : GenerateExternalCss(priorExtCss);
		}

		internal static void ValidateExternalCss(IEnumerable<ExternalCss> cssItems, RulesException errors)
		{
			foreach (var item in cssItems)
				item.Invalid = false;

			var duplicateUrls = cssItems.GroupBy(css => css.Url).Where(g => g.Count() > 1).Select(x => x.Key).ToArray();

			var externalCssUrlsArray = cssItems.ToArray();
			for (int i = 0; i < externalCssUrlsArray.Length; i++)
				ExternalCssHelper.ValidateExternalCss(externalCssUrlsArray[i], errors, i + 1, duplicateUrls);		
		}

		internal static void ValidateExternalCss(ExternalCss externalCss, RulesException errors, int index, string[] duplicateUrls)
		{
			if (!string.IsNullOrWhiteSpace(externalCss.Url) && duplicateUrls.Contains(externalCss.Url))
			{
				errors.ErrorForModel(String.Format(VisualEditorStrings.ExternalCssUrlDuplicate, index));
				externalCss.Invalid = true;
			}

			if (string.IsNullOrWhiteSpace(externalCss.Url))
			{
				errors.ErrorForModel(String.Format(VisualEditorStrings.ExternalCssUrlRequired, index));
				externalCss.Invalid = true;
			}

			if (!string.IsNullOrWhiteSpace(externalCss.Url) && !Regex.IsMatch(externalCss.Url, RegularExpressions.ExternalCssUrl))
			{
				errors.ErrorForModel(String.Format(VisualEditorStrings.ExternalCssUrlNotValid, index));
				externalCss.Invalid = true;
			}

			if (externalCss.Url.Length > 255)
			{
				errors.ErrorForModel(String.Format(VisualEditorStrings.ExternalCssUrlMaxLengthExceeded, index));
				externalCss.Invalid = true;
			}
		}

		public static string CreateConfigString(IEnumerable<ExternalCss> list)
		{			
			string result = String.Join(",", list.Select(s => String.Format("'{0}'", s.Url)));
			return result;
		}
	}
}
