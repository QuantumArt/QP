using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.MultistepActions.Csv;
using Quantumart.QP8.BLL.Services.MultistepActions.Import;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Helpers
{
    public static class MultistepActionHelper
    {
        public static int GetStepCount(int itemsCount, int itemsPerStep)
        {
            var stepCount = itemsCount / itemsPerStep;
            stepCount = itemsCount % itemsPerStep > 0 ? ++stepCount : stepCount;
            return stepCount;
        }

        public static Dictionary<string, string> ParseListOfParameters(List<string> prms)
        {
            var result = new Dictionary<string, string>();
            foreach (var param in prms)
            {
                var keyValuePair = param.Split('=');
                if (keyValuePair.Length == 2)
                {
                    if (!result.ContainsKey(keyValuePair[0].Replace("Data.", string.Empty)))
                    {
                        result.Add(keyValuePair[0].Replace("Data.", string.Empty), keyValuePair[1]);
                    }
                }
            }

            return result;
        }

        public static string NumericCultureFormat(string value, string fromCulture, string toCulture)
        {
            if (!double.TryParse(value, NumberStyles.Any, CultureInfo.GetCultureInfo(fromCulture).NumberFormat, out double result) && value != "NULL")
            {
                throw new FormatException(string.Format(ImportStrings.NumberFormatError, value));
            }
            if (value == "NULL")
            {
                return string.Empty;
            }

            value = result.ToString(CultureInfo.GetCultureInfo(toCulture).NumberFormat);
            return value;
        }

        public static string O2MFormat(string value)
        {
            if (int.TryParse(value, out int res))
            {
                return res.ToString();
            }

            throw new FormatException(string.Format(ImportStrings.O2MFormatError, value));
        }

        public static string BoolFormat(string value)
        {
            if (value == "False")
            {
                return "0";
            }

            if (value == "True")
            {
                return "1";
            }

            if (new[] { "1", "0", "NULL", string.Empty }.Contains(value))
            {
                return value;
            }

            throw new FormatException(string.Format(ImportStrings.BoolFormatError, value));
        }

        public static IEnumerable<int> M2MFormat(string value)
        {
            var ids = value.Trim(',', '"', ';').Split(',', ';');
            foreach (var val in ids.Where(val => !string.IsNullOrEmpty(val)))
            {
                if (int.TryParse(val, out int res))
                {
                    yield return res;
                }
                else
                {
                    throw new FormatException(string.Format(ImportStrings.M2MFormatError, val));
                }
            }
        }

        public static string DateCultureFormat(string value, string fromCulture, string toCulture)
        {
            if (!DateTime.TryParse(value, CultureInfo.GetCultureInfo(fromCulture).DateTimeFormat, DateTimeStyles.None, out DateTime result) && value != "NULL")
            {
                throw new FormatException(string.Format(ImportStrings.DateTimeFormatError, value));
            }

            if (value == "NULL")
            {
                return string.Empty;
            }

            value = result.ToString(CultureInfo.GetCultureInfo(toCulture).DateTimeFormat);
            return value;
        }

        public static List<string> GetFileFields(ImportSettings setts, FileReader reader)
        {
            reader.CopyFileToTempDir();
            var firstline = reader.Lines.Select(n => n.Value).Where(s => !s.StartsWith("sep=")).Take(1).ToList();
            if (!firstline.Any())
            {
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

        public static string GetXmlFromDataRows(IEnumerable<DataRow> rows, string columnName)
        {
            var source = $"source_{columnName}_id";
            var destination = $"destination_{columnName}_id";
            return GetXmlFromDataRows(rows, source, destination);
        }

        public static string GetXmlFromDataRows(IEnumerable<DataRow> rows, string sourceColumnName, string destinationColumnName)
        {
            var xDocument = new XDocument();
            var items = new XElement("items");
            xDocument.Add(items);

            foreach (var row in rows)
            {
                var itemXml = new XElement("item");
                itemXml.Add(new XAttribute("sourceId", row[sourceColumnName].ToString()));
                itemXml.Add(new XAttribute("destinationId", row[destinationColumnName].ToString()));
                xDocument.Root?.Add(itemXml);
            }

            return xDocument.ToString();
        }
    }
}
