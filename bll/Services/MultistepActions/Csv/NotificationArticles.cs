using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Csv;

public class NotificationArticles
{
    public string NotificationCode { get; set; }
    public List<int> ArticleIds { get; set; }

    public NotificationArticles(List<int> articleIds, string notificationCode)
    {
        ArticleIds = articleIds;
        NotificationCode = notificationCode;
    }
}
