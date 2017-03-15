namespace Quantumart.QP8.EntityFramework6.Data
{
    public interface IQPFormService
    {
        string GetFormNameByNetNames(string netContentName, string netFieldName);
        string ReplacePlaceholders(string text);
    }
}