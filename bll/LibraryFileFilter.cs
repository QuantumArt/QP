namespace Quantumart.QP8.BLL
{
    /// <summary>
    /// Фильтр файлов в библиотеке
    /// </summary>
    public class LibraryFileFilter
    {
        public FolderFileType? FileType { get; set; }
        public string FileNameFilter { get; set; }

        public string Mask => string.IsNullOrEmpty(FileNameFilter) ? "*" : string.Format("*{0}*", FileNameFilter);
    }
}
