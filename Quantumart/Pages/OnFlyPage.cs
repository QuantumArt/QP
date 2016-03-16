using System;
using Quantumart.QPublishing.OnScreen;

namespace Quantumart.QPublishing.Pages
{
    public class OnFlyPage : RSPage
    {

        [RemoteScriptingMethod()]
        public string DecreaseStatus(string itemId)
        {
            return OnFly.DecreaseStatus(Int32.Parse(itemId));
        }

        [RemoteScriptingMethod()]
        public string UpdateArticle(string itemId, string attrName, string uploadUrl, string siteUrl, string attrValue)
        {
            return OnFly.UpdateArticle(Int32.Parse(itemId), attrName, uploadUrl, siteUrl, attrValue);
        }

        [RemoteScriptingMethod()]
        public string CreateLikeArticle(string itemId, string contentId, string siteId)
        {
            return OnFly.CreateLikeArticle(Int32.Parse(itemId), Int32.Parse(contentId), Int32.Parse(siteId));
        }

    }
}