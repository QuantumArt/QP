using System.Text;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.WebMvc.ViewModels.DirectLink
{
    public class DirectLinkOptions
    {
        public string CustomerCode { get; set; }

        public string ActionCode { get; set; }

        public string EntityTypeCode { get; set; }

        public int? EntityId { get; set; }

        public int? ParentEntityId { get; set; }

        public string ToUrlParams()
        {
            if (IsDefined())
            {
                var sb = new StringBuilder();
                sb.AppendFormat("actionCode={0}&entityTypeCode={1}", ActionCode, EntityTypeCode);
                if (!string.IsNullOrEmpty(CustomerCode))
                {
                    sb.AppendFormat("&customerCode={0}", CustomerCode);
                }

                if (EntityId.HasValue)
                {
                    sb.AppendFormat("&entityId={0}", EntityId);
                }

                if (ParentEntityId.HasValue)
                {
                    sb.AppendFormat("&parentEntityId={0}", ParentEntityId);
                }

                return sb.ToString();
            }

            return string.Empty;
        }

        public string AddToUrl(string url)
        {
            if (IsDefined())
            {
                if (Url.IsQueryEmpty(url))
                {
                    return url + "?" + ToUrlParams();
                }

                return url + "&" + ToUrlParams();
            }

            return url;
        }

        public bool IsDefined() => !string.IsNullOrEmpty(ActionCode) && !string.IsNullOrEmpty(EntityTypeCode) && (EntityId.HasValue || ParentEntityId.HasValue);
    }
}
