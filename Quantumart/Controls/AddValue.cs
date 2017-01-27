using System;
using System.Web.UI;
using Quantumart.QPublishing.Pages;

namespace Quantumart.QPublishing.Controls
{
    public class AddValue : UserControl
    {
        // ReSharper disable once InconsistentNaming
        public string key { get; set; }

        // ReSharper disable once InconsistentNaming
        public string value { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            ((QPage)Page).AddValue(key, value);
        }
    }
}
