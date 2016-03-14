using System;
using System.Text;
using System.Web;

namespace Quantumart.QP8.Utils
{
	public static class RequestExtensions
	{
		/// <summary>
		/// Проверяет наличие параметра в строке запроса URL
		/// </summary>
		/// <param name="name">название параметра</param>
		/// <returns>результат проверки (true - присутствует в строке запроса; false - не присутствует)</returns>
		public static bool CheckParameterPresenceInQueryString(this HttpRequest source, string name)
		{
			bool result = false; // результирующая переменная
			string rawQueryString = source.ServerVariables["QUERY_STRING"]; // необработанная строка запроса URL

			if (rawQueryString.Length > 0)
			{
				int sharpSymbolPosition = rawQueryString.IndexOf('#'); // позиция символа решетки
				string queryString = ""; // обработанная строка запроса URL

				if (sharpSymbolPosition > -1)
				{
					queryString = rawQueryString.Substring(0, sharpSymbolPosition);
				}
				else
				{
					queryString = rawQueryString;
				}

				if (queryString.Length > 0)
				{
					string[] parameterList = new string[0]; // список параметров строки запроса
					string parameter = ""; // параметр
					int parameterIndex = 0; // индекс параметра
					int parameterCount = 0; // количество параметров
					string parameterName = ""; // название параметра
					int signOfEqualityPosition = -1; // позиция знака равенства

					parameterList = queryString.Split('&');
					parameterCount = parameterList.Length;

					for (parameterIndex = 0; parameterIndex < parameterCount; parameterIndex++)
					{
						parameter = parameterList[parameterIndex];
						signOfEqualityPosition = parameter.IndexOf('=');

						if (signOfEqualityPosition > -1)
						{
							parameterName = parameter.Substring(0, signOfEqualityPosition);
						}
						else
						{
							parameterName = parameter;
						}

						if (parameterName.ToLower() == name.ToLower())
						{
							result = true;

							break;
						}
					}

					parameterList = null;
				}
			}

			return result;
		}
	}
}
