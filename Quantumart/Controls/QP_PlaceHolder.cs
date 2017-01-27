using System;
using Quantumart.QPublishing.Pages;

namespace Quantumart.QPublishing.Controls
{

    public sealed class PlaceHolder : QUserControlBase
    {
        protected override void OnInit(EventArgs e)
        {
            var page = Page as QPage;
            if (page != null)
            {
                if (simple)
                {
                    page.ShowObjectSimple(calls, this);
                }
                else
                {
                    page.ShowObject(calls, this);

                }
            }
            else
            {
                var mobilePage = Page as QMobilePage;
                if (mobilePage != null)
                {
                    if (simple)
                    {
                        mobilePage.ShowObjectSimple(calls, this);
                    }
                    else
                    {
                        mobilePage.ShowObject(calls, this);
                    }
                }
            }
        }
    }
}
