using Quantumart.QP8.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.DAL.DTO
{
    public class FieldPageOptions : PageOptionsBase
    {
        public int ContentId { get; set; }

		public FieldSelectMode Mode { get; set; }

    }
}
