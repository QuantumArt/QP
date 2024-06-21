using System;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Services.ArticleServices
{
    public interface IArticleService
    {
        int GetArticleIdByGuid(string guid);

        int GetArticleIdByGuid(Guid guid);

        int GetArticleIdByGuidOrDefault(Guid guid);

        Guid GetArticleGuidById(string id);

        Guid GetArticleGuidById(int id);

        Guid[] GetArticleGuidsByIds(int[] ids);

        int[] GetArticleIdsByGuids(Guid[] guids);

        Guid? GetArticleGuidByIdOrDefault(int id);

        CopyResult Copy(Article article, bool? boundToExternal, bool disableNotifications, Guid? guidForSubstitution);

        MessageResult Remove(int contentId, int id, bool fromArchive, bool? boundToExternal, bool disableNotifications);
        MessageResult Remove(int contentId, int[] ids, bool fromArchive, bool? boundToExternal, bool disableNotifications);
        MessageResult Remove(int contentId, Article articleToRemove, bool fromArchive, bool? boundToExternal, bool disableNotifications);
        MessageResult MoveToArchive(int id, bool? boundToExternal, bool disableNotifications);
        MessageResult MoveToArchive(Article articleToArchive, bool? boundToExternal, bool disableNotifications);
        MessageResult MoveToArchive(int contentId, int[] ids, bool? boundToExternal, bool disableNotifications);
        MessageResult RestoreFromArchive(Article articleToRestore, bool? boundToExternal, bool disableNotifications);
        MessageResult RestoreFromArchive(int contentId, int[] ids, bool? boundToExternal, bool disableNotifications);
        MessageResult Publish(int contentId, int[] ids, bool? boundToExternal, bool disableNotifications);
        Article Create(Article article, string backendActionCode, bool? boundToExternal, bool disableNotifications);
        Article Update(Article article, string backendActionCode, bool? boundToExternal, bool disableNotifications);

    }
}
