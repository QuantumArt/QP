using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.DAL.DTO
{
    public class SitePageOptions : PageOptionsBase
    {
        public bool UseSecurity { get; set; }

        public int UserId { get; set; }
    }
}
