
using System.Web.UI;

namespace Quantumart.QPublishing.Controls
{

    public abstract class QUserControlBase : UserControl, IQUserControlBase
    {
        public string calls { get; set; } = "";

        public bool simple { get; set; } = false;
    }
}