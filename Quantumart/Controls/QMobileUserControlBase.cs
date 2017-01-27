using System.Web.UI.MobileControls;

namespace Quantumart.QPublishing.Controls
{
    public class QMobileUserControlBase : MobileUserControl, IQUserControlBase
    {
        public string calls { get; set; } = string.Empty;

        public bool simple { get; set; }
    }
}
