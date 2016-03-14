using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.MultistepActions.Import;
using Quantumart.QP8.BLL.Services.MultistepActions.Import.Csv;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Helpers
{
    public static class MultistepActionHelper
    {
        public static int GetStepCount(int itemsCount, int itemsPerStep)
        {
            int stepCount = itemsCount / itemsPerStep;
            stepCount = (itemsCount % itemsPerStep) > 0 ? ++stepCount : stepCount;
            return stepCount;
        }

        public static Dictionary<string, string> ParseListOfParameters(List<string> prms)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var param in prms)
            {
                string[] keyValuePair = param.Split('=');
                if (keyValuePair != null && keyValuePair.Length == 2)
                {
                    if (!result.ContainsKey(keyValuePair[0].Replace("Data.", "")))
                    {
                        result.Add(keyValuePair[0].Replace("Data.", ""), keyValuePair[1]);
                    }
                }
            }

            return result;
        }
        public static string GetEncoding(string encodingInt)
        {
            string result = String.Empty;
            switch (encodingInt)
            {
                case "0":
                    result = "Windows-1251";
                    break;
                case "1":
                    result = "UTF-8";
                    break;
                case "2":
                    result = "UTF-16";
                    break;
                case "3":
                    result = "KOI8-R";
                    break;
				case "4":
					result = "cp866";
					break;
                default:
                    result = "Windows-1251";
                    break;
            }
            return result;
        }
        public static string GetCulture(string cultureNum)
        {
            string result = String.Empty;
            switch (cultureNum)
            {
                case "0":
                    result = "ru-RU";
                    break;
                case "1":
                    result = "en-US";
                    break;
                default:
                    result = "ru-RU";
                    break;
            }
            return result;
        }

        public static string NumericCultureFormat(string value, string fromCulture, string toCulture)
        {
            Double result;
            if (!Double.TryParse(value, NumberStyles.Any, CultureInfo.GetCultureInfo(fromCulture).NumberFormat, out result) && value != "NULL")
            {
                throw new FormatException(String.Format(ImportStrings.NumberFormatError, value));
            }
            else if (value == "NULL")
            {
                return String.Empty;
            }
            else
            {
                value = result.ToString(CultureInfo.GetCultureInfo(toCulture).NumberFormat);
            }
            return value;
        }

		public static string O2MFormat(string value)
		{
			int res;
			if (Int32.TryParse(value, out res))
				return res.ToString();
			else
				throw new FormatException(String.Format(ImportStrings.O2MFormatError, value));
		}

		public static string BoolFormat(string value)
		{
			if (value == "False")
				return "0";
			else if (value == "True")
				return "1";
			else if (new string[] {"1", "0", "NULL", ""}.Contains(value))
				return value;
			else
				throw new FormatException(String.Format(ImportStrings.BoolFormatError, value));
		}

		public static IEnumerable<int> M2MFormat(string value)
		{
			string[] ids = value.Trim(',', '"', ';').Split(',', ';');
			List<int> values = new List<int>();
			foreach (var val in ids)
			{
				if (!String.IsNullOrEmpty(val))
				{
					int res;
					if (Int32.TryParse(val, out res))
					{
						yield return res;
					}
					else throw new FormatException(String.Format(ImportStrings.M2MFormatError, val));
				}
			}
		}

        public static string DateCultureFormat(string value, string fromCulture, string toCulture)
        {
            DateTime result;
            if (!DateTime.TryParse(value, CultureInfo.GetCultureInfo(fromCulture).DateTimeFormat, DateTimeStyles.None, out result) && value != "NULL")
            {
				throw new FormatException(String.Format(ImportStrings.DateTimeFormatError, value));
            }
            else if (value == "NULL")
            {
                return String.Empty;
            }
            else
            {
                value = result.ToString(CultureInfo.GetCultureInfo(toCulture).DateTimeFormat);
            }
            return value;
        }
        
        public static char GetDelimiter(string delimiterId)
        {
            char delimiterChar;
            switch (delimiterId)
            {
                case "0":
                    delimiterChar = ',';
                    break;
                case "1":
                    delimiterChar = ';';
                    break;
                case "2":
                    delimiterChar = '\t';
                    break;
                default:
                    delimiterChar = ';';
                    break;
            }
            return delimiterChar;
        }
        public static string GetLineSeparator(string separatorId)
        {
            string separator;
            switch (separatorId)
            {
                case "0":
                    separator = "\r";
                    break;
                case "1":
                    separator = "\r\n";
                    break;
                case "2":
                    separator = "\n";
                    break;
                default:
                    separator = "\r";
                    break;
            }
            return separator;
        }
        public static List<string> GetFileFields(ImportSettings setts, FileReader reader)
        {
            reader.CopyFileToTempDir();
			IEnumerable<string> firstline = reader.Lines.Select(n => n.Value).Where(s => !s.StartsWith("sep=")).Take(1);
            if (firstline.Count() == 0) {
                throw new ArgumentException(ImportStrings.NoLines);
            }
            return CsvReader.GetFieldNames(firstline, setts.Delimiter, setts.NoHeaders);
        }
        public static IEnumerable<ListItem> GetLocalesAsListItems()
        {
            return PageTemplateRepository.GetLocalesList().Select(locale => new ListItem { Text = locale.Name, Value = locale.Id.ToString() });
        }

        public static IEnumerable<ListItem> GetCharsetsAsListItems()
        {
            return PageTemplateRepository.GetCharsetsList().Select(charset => new ListItem { Text = charset.Subj, Value = charset.Subj });
        }

        public static string GetXmlFromDataRows(IEnumerable<DataRow> rows, string columnName) {
            string source = String.Format("source_{0}_id", columnName);
            string destination = String.Format("destination_{0}_id", columnName);
            return GetXmlFromDataRows(rows, source, destination);
        }

        public static string GetXmlFromDataRows(IEnumerable<DataRow> rows, string sourceColumnName, string destinationColumnName)
        {
            XDocument xDocument = new XDocument();
            XElement items = new XElement("items");
            xDocument.Add(items);
            foreach (DataRow row in rows)
            {
                XElement itemXML = new XElement("item");
                itemXML.Add(new XAttribute("sourceId", row[sourceColumnName].ToString()));
                itemXML.Add(new XAttribute("destinationId", row[destinationColumnName].ToString()));
                xDocument.Root.Add(itemXML);
            }
            return xDocument.ToString();
        }
    }
}
