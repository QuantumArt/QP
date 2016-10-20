using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class ExportImportModel : EntityViewModel
    {
        [LocalizedDisplayName("ImportCulture", NameResourceType = typeof(MultistepActionStrings))]
        public string Culture { get; set; }

        [LocalizedDisplayName("ImportEncoding", NameResourceType = typeof(MultistepActionStrings))]
        public string Encoding { get; set; }

        [LocalizedDisplayName("FieldDelimiter", NameResourceType = typeof(MultistepActionStrings))]
        public string Delimiter { get; set; } = "1";

        [LocalizedDisplayName("LineSeparator", NameResourceType = typeof(MultistepActionStrings))]
        public string LineSeparator { get; set; }

        public List<ListItem> Locales => new List<ListItem>
        {
            new ListItem("0", UserStrings.Russian),
            new ListItem("1", UserStrings.English)
        };

        public List<ListItem> Charsets => new List<ListItem>
        {
            new ListItem("0", "Windows-1251"),
            new ListItem("1", "UTF-8"),
            new ListItem("2", "UTF-16"),
            new ListItem("3", "KOI8-R"),
            new ListItem("4", "DOS (Cyrillic)")
        };

        public List<ListItem> Delimiters => new List<ListItem>
        {
            new ListItem("0", MultistepActionStrings.ImportComma),
            new ListItem("1", MultistepActionStrings.ImportSemicolon),
            new ListItem("2", MultistepActionStrings.ImportTab)
        };

        public List<ListItem> LineSeparators => new List<ListItem>
        {
            new ListItem("0", MultistepActionStrings.CR),
            new ListItem("1", MultistepActionStrings.CRLF),
            new ListItem("2", MultistepActionStrings.LF)
        };

        public override string EntityTypeCode
        {
            get { throw new NotImplementedException(); }
        }

        public override string ActionCode
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<ListItem> GetList(IEnumerable<int> ids)
        {
            var result = Enumerable.Empty<ListItem>();
            if (ids != null && ids.Any())
            {
                result = FieldService.GetList(ids.ToArray()).Select(s => new ListItem(s.Id.ToString(), s.Name)).ToArray();
            }

            return result;
        }
    }
}
