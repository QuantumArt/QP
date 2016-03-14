using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Constants;

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

		public string CustomFilter { get; set; }

    }
}
