
using System.Web.UI.MobileControls;

namespace Quantumart.QPublishing.Controls
{

    public class QMobileUserControlBase : MobileUserControl, IQUserControlBase
    {
        public string calls { get; set; } = "";

        public bool simple { get; set; }
    }
}