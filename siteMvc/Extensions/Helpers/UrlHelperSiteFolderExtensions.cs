using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public static class UrlHelperSiteFolderExtensions
    {
        private static string GetCurrentTheme()
        {
            return QPConfiguration.Options.DefaultTheme;
        }

        /// <summary>
        /// Возвращает путь к директории, в которой хранятся изображения указанной темы
        /// </summary>
        /// <param name="url">URL хелпер</param>
        /// <returns>путь к директории, в которой хранятся изображения указанной темы</returns>
        public static string GetThemeRootImageFolderUrl(this IUrlHelper url)
        {
            return SitePathHelper.GetThemeRootImageFolderUrl(GetCurrentTheme());
        }

        /// <summary>
        /// Возвращает путь к директории, в которой хранятся маленькие пиктограммы указанной темы
        /// </summary>
        /// <param name="url">URL хелпер</param>
        /// <returns>путь к директории, в которой хранятся маленькие пиктограммы указанной темы</returns>
        public static string GetThemeSmallIconsImageFolderUrl(this IUrlHelper url)
        {
            return SitePathHelper.GetThemeSmallIconsImageFolderUrl(GetCurrentTheme());
        }

        /// <summary>
        /// Возвращает путь к директории, в которой хранятся индикаторы AJAX-загрузки указанной темы
        /// </summary>
        /// <param name="url">URL хелпер</param>
        /// <returns>путь к директории, в которой хранятся индикаторы AJAX-загрузки указанной темы</returns>
        public static string GetThemeAjaxLoaderIconsImageFolderUrl(this IUrlHelper url)
        {
            return SitePathHelper.GetThemeAjaxLoaderIconsImageFolderUrl(GetCurrentTheme());
        }
    }
}
