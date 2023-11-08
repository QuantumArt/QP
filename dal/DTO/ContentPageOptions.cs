using Quantumart.QP8.Constants;
using System.Collections.Generic;

namespace Quantumart.QP8.DAL.DTO
{
    public class ContentPageOptions : PageOptionsBase
    {
        public int? GroupId { get; set; }

        public int? SiteId { get; set; }

        public string ContentName { get; set; }

        public bool IsVirtual { get; set; }

        public ContentSelectMode Mode { get; set; }

        public bool UseSecurity { get; set; }

        public int UserId { get; set; }

        public bool IsAdmin { get; set; }

        public int LanguageId { get; set; }

        public Dictionary<string, object[]> CustomFilter { get; set; }
    }
}
