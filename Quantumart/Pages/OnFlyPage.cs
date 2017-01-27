using Quantumart.QPublishing.OnScreen;

namespace Quantumart.QPublishing.Pages
{
    public class OnFlyPage : RSPage
    {
        [RemoteScriptingMethod]
        public string DecreaseStatus(string itemId)
        {
            return OnFly.DecreaseStatus(int.Parse(itemId));
        }

        [RemoteScriptingMethod]
        public string UpdateArticle(string itemId, string attrName, string uploadUrl, string siteUrl, string attrValue)
        {
            return OnFly.UpdateArticle(int.Parse(itemId), attrName, uploadUrl, siteUrl, attrValue);
        }

        [RemoteScriptingMethod]
        public string CreateLikeArticle(string itemId, string contentId, string siteId)
        {
            return OnFly.CreateLikeArticle(int.Parse(itemId), int.Parse(contentId), int.Parse(siteId));
        }
    }
}
