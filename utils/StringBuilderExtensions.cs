using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Quantumart.QP8.Utils
{
	public static class StringBuilderExtensions
	{
		private static Regex _formatPlaceholderRegExp = new Regex(@"\{[0-9]\}", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

		public static StringBuilder AppendFormatLine(this StringBuilder sb)
		{
			return sb.AppendLine();
		}

		public static StringBuilder AppendFormatLine(this StringBuilder sb, string format, params object[] args)  
		{
			if (_formatPlaceholderRegExp.IsMatch(format))
			{
				return sb.AppendFormat(format, args).AppendLine();
			}
			else
			{
				return sb.AppendLine(format.Replace("{{", "{").Replace("}}", "}"));
			}
		}
	}
}
