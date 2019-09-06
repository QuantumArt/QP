using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Enums.Csv;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.MultistepSettings
{
    public class ExportImportModel : EntityViewModel
    {
        [Display(Name = "ImportCulture", ResourceType = typeof(MultistepActionStrings))]
        public string Culture { get; set; }

        [Display(Name = "ImportEncoding", ResourceType = typeof(MultistepActionStrings))]
        public string Encoding { get; set; }

        [Display(Name = "FieldDelimiter", ResourceType = typeof(MultistepActionStrings))]
        public string Delimiter { get; set; } = "1";

        [Display(Name = "LineSeparator", ResourceType = typeof(MultistepActionStrings))]
        public string LineSeparator { get; set; }

        public List<ListItem> Locales => new List<ListItem>
        {
            new ListItem(((int)CsvCulture.RuRu).ToString(), UserStrings.Russian),
            new ListItem(((int)CsvCulture.EnUs).ToString(), UserStrings.English)
        };

        public List<ListItem> Charsets => new List<ListItem>
        {
            new ListItem(((int)CsvEncoding.Windows1251).ToString(), "Windows-1251"),
            new ListItem(((int)CsvEncoding.Utf8).ToString(), "UTF-8"),
            new ListItem(((int)CsvEncoding.Utf16).ToString(), "UTF-16"),
            new ListItem(((int)CsvEncoding.Koi8R).ToString(), "KOI8-R"),
            new ListItem(((int)CsvEncoding.Cp866).ToString(), "DOS (Cyrillic)")
        };

        public List<ListItem> Delimiters => new List<ListItem>
        {
            new ListItem(((int)CsvDelimiter.Comma).ToString(), MultistepActionStrings.ImportComma),
            new ListItem(((int)CsvDelimiter.Semicolon).ToString(), MultistepActionStrings.ImportSemicolon),
            new ListItem(((int)CsvDelimiter.Tab).ToString(), MultistepActionStrings.ImportTab)
        };

        public List<ListItem> LineSeparators => new List<ListItem>
        {
            new ListItem(((int)CsvLineSeparator.Windows).ToString(), MultistepActionStrings.CRLF),
            new ListItem(((int)CsvLineSeparator.MacOs).ToString(), MultistepActionStrings.CR),
            new ListItem(((int)CsvLineSeparator.Unix).ToString(), MultistepActionStrings.LF)
        };

        public override string EntityTypeCode => throw new NotImplementedException();

        public override string ActionCode => throw new NotImplementedException();

        public IEnumerable<ListItem> GetList(IEnumerable<int> ids)
        {
            var result = Enumerable.Empty<ListItem>();
            if (ids != null)
            {
                var idsList = ids.ToList();
                if (idsList.Any())
                {
                    result = FieldService.GetList(idsList.ToArray()).Select(s => new ListItem(s.Id.ToString(), s.Name)).ToArray();
                }
            }

            return result;
        }
    }
}
