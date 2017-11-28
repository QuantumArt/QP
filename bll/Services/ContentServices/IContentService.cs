namespace Quantumart.QP8.BLL.Services.ContentServices
{
    public interface IContentService
    {
        bool IsRelation(int contentId, int fieldId);

        bool IsClassifier(int contentId, int fieldId);
    }
}
