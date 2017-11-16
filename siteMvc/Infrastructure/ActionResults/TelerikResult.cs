using System.Collections;
using System.Web.Mvc;
using Newtonsoft.Json.Serialization;
using QP8.Infrastructure.Extensions;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Infrastructure.ActionResults
{
    public class TelerikResult : ContentResult
    {
        public TelerikResult(IEnumerable data, int totalCount)
            : this(new GridModel { Data = data, Total = totalCount })
        {
        }

        public TelerikResult(GridModel model)
        {
            Content = model.ToJsonLog(new DefaultContractResolver());
            ContentType = "application/json";
        }
    }
}
