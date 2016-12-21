namespace EntityFramework6.Test.DataContext
{
    public interface IQPLibraryService
    {
        string GetUrl(string input, string className, string propertyName);

        string GetUploadPath(string input, string className, string propertyName);

        string ReplacePlaceholders(string text);
    }
}