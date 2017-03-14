using System;

namespace Quantumart.QP8.BLL.Interfaces.Services
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
    }
}
