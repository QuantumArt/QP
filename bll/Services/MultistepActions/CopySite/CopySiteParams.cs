using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Services.MultistepActions.CopySite
{
    public class CopySiteParams : IMultistepActionSettings
    {
        public bool AllowAction { get { return true; } }

        public int StagesCount
        {
            get { return 8; }
        }
    }
}
