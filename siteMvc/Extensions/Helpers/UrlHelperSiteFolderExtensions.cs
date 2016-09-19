using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public static class UrlHelperSiteFolderExtensions
    {
        private static string GetCurrentTheme()
        {
            var themeName = "default";
            var session = HttpContext.Current.Session;
            if (session["theme"] != null)
            {
                themeName = session["theme"].ToString();
            }

            return themeName;
        }

        /// <summary>
        /// Возвращает путь к директории, в которой хранятся изображения общие для всех тем
        /// </summary>
        /// <param name="url">URL хелпер</param>
        /// <returns>путь к директории, в которой хранятся изображения общие для всех тем</returns>
        public static string GetCommonRootImageFolderUrl(this UrlHelper url)
        {
            return SitePathHelper.GetCommonRootImageFolderUrl();
        }

        /// <summary>
        /// Возвращает путь к директории, в которой хранятся изображения указанной темы
        /// </summary>
        /// <param name="url">URL хелпер</param>
        /// <returns>путь к директории, в которой хранятся изображения указанной темы</returns>
        public static string GetThemeRootImageFolderUrl(this UrlHelper url)
        {
            return SitePathHelper.GetThemeRootImageFolderUrl(GetCurrentTheme());
        }

        /// <summary>
        /// Возвращает путь к директории, в которой хранятся маленькие пиктограммы указанной темы
        /// </summary>
        /// <param name="url">URL хелпер</param>
        /// <returns>путь к директории, в которой хранятся маленькие пиктограммы указанной темы</returns>
        public static string GetThemeSmallIconsImageFolderUrl(this UrlHelper url)
        {
            return SitePathHelper.GetThemeSmallIconsImageFolderUrl(GetCurrentTheme());
        }

        /// <summary>
        /// Возвращает путь к директории, в которой хранятся индикаторы AJAX-загрузки указанной темы
        /// </summary>
        /// <param name="url">URL хелпер</param>
        /// <returns>путь к директории, в которой хранятся индикаторы AJAX-загрузки указанной темы</returns>
        public static string GetThemeAjaxLoaderIconsImageFolderUrl(this UrlHelper url)
        {
            return SitePathHelper.GetThemeAjaxLoaderIconsImageFolderUrl(GetCurrentTheme());
        }
    }
}
