using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL.Helpers
{
    public class SitePathHelper
    {
        /// <summary>
        /// Возвращает URL, по которому расположен бэкенд
        /// </summary>
        /// <returns>URL бэкенда</returns>
        public static string GetCurrentRootUrl() => SitePathRepository.GetCurrentRootUrl();

        /// <summary>
        /// Возвращает путь к директории, в которой хранятся изображения общие для всех тем
        /// </summary>
        /// <returns>путь к директории, в которой хранятся изображения общие для всех тем</returns>
        public static string GetCommonRootImageFolderUrl() => SitePathRepository.GetCommonRootImageFolderUrl();

        /// <summary>
        /// Возвращает путь к директории, в которой хранятся изображения указанной темы
        /// </summary>
        /// <param name="themeName">название темы</param>
        /// <returns>путь к директории, в которой хранятся изображения указанной темы</returns>
        public static string GetThemeRootImageFolderUrl(string themeName) => SitePathRepository.GetThemeRootImageFolderUrl(themeName);

        /// <summary>
        /// Возвращает путь к директории, в которой хранятся маленькие пиктограммы указанной темы
        /// </summary>
        /// <param name="themeName">название темы</param>
        /// <returns>путь к директории, в которой хранятся маленькие пиктограммы указанной темы</returns>
        public static string GetThemeSmallIconsImageFolderUrl(string themeName) => SitePathRepository.GetThemeSmallIconsImageFolderUrl(themeName);

        /// <summary>
        /// Возвращает путь к директории, в которой хранятся маленькие пиктограммы типов файлов
        /// </summary>
        /// <param name="themeName">название темы</param>
        public static string GetThemeSmallFileTypeIconFolderUrl(string themeName) => SitePathRepository.GetThemeSmallFileTypeIconFolderUrl(themeName);

        /// <summary>
        /// Возвращает путь к директории, в которой хранятся большие пиктограммы типов файлов
        /// </summary>
        /// <param name="themeName">название темы</param>
        public static string GetThemeBigFileTypeIconFolderUrl(string themeName) => SitePathRepository.GetThemeBigFileTypeIconFolderUrl(themeName);

        /// <summary>
        /// Возвращает путь к директории, в которой хранятся индикаторы AJAX-загрузки указанной темы
        /// </summary>
        /// <param name="themeName">название темы</param>
        /// <returns>путь к директории, в которой хранятся индикаторы AJAX-загрузки указанной темы</returns>
        public static string GetThemeAjaxLoaderIconsImageFolderUrl(string themeName) => SitePathRepository.GetThemeAjaxLoaderIconsImageFolderUrl(themeName);
    }
}
