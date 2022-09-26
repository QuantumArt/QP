using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Repository
{
    internal class SitePathRepository
    {
        private static readonly char Sep = Path.DirectorySeparatorChar;
        public static string RELATIVE_PATH_TO_COPY = $"{Sep}ToCopy";
        public static string RELATIVE_PREVIEW_PATH = $"{Sep}temp{Sep}preview{Sep}objects";
        public static string RELATIVE_NOTIFICATIONS_PATH = $"{Sep}qp_notifications";
        public static string RELATIVE_SETTINGS_PATH = $"{Sep}qp_settings";
        public static string RELATIVE_CONTENTS_PATH = $"{Sep}contents";
        public static string RELATIVE_TEMPLATES_PATH = $"{Sep}templates";
        public static string RELATIVE_IMAGES_PATH = $"{Sep}images";
        public static string RELATIVE_BIN_PATH = $"{Sep}bin";
        public static string RELATIVE_APP_DATA_PATH = $"{Sep}App_Data";
        public static string RELATIVE_APP_CODE_PATH = $"{Sep}App_Code";

        private static HttpContext HttpContext => new HttpContextAccessor().HttpContext;

        /// <summary>
        /// Возвращает URL, по которому расположен бэкенд
        /// </summary>
        /// <returns>URL бэкенда</returns>
        internal static string GetCurrentRootUrl()
        {
            var qpConfig = QPConfiguration.Options;
            return qpConfig == null ? string.Empty : qpConfig.BackendUrl;
        }

        /// <summary>
        /// Возвращает путь к директории, в которой раcположены файлы
        /// для копирования в новый и обновляемый сайт
        /// </summary>
        /// <returns>путь</returns>
        internal static string GetDirectoryPathToCopy()
        {
            string rootUrl = GetCurrentRootUrl();

            if (HttpContext == null || string.IsNullOrEmpty(rootUrl))
            {
                return string.Empty;
            }

            var hostingEnvironment = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();

            // TODO: review resolver directory path
            return hostingEnvironment.ContentRootFileProvider.GetFileInfo(rootUrl + RELATIVE_PATH_TO_COPY).PhysicalPath;
        }

        private static IUrlHelper GetUrlHelper()
        {
            return HttpContext.RequestServices.GetRequiredService<IUrlHelper>();
        }

        /// <summary>
        /// Копирует все эталонные файлы из исходной папки в заданую папку
        /// </summary>
        /// <param name="destinationDirectoryPath">путь к папке назначения</param>
        /// <param name="relativeDirectoryPath">относительный путь к папке с эталонными файлами</param>
        internal static void CopySiteDirectory(string destinationDirectoryPath, string relativeDirectoryPath)
        {
            var basePathToCopy = GetDirectoryPathToCopy();
            if (!string.IsNullOrEmpty(basePathToCopy))
            {
                var directoryPath = PathUtility.Combine(basePathToCopy, relativeDirectoryPath);
                foreach (var filePath in Directory.GetFiles(directoryPath))
                {
                    var fileName = Path.GetFileName(filePath);
                    var fromFile = PathUtility.Combine(directoryPath, fileName);
                    var toFile = PathUtility.Combine(destinationDirectoryPath, fileName);
                    CopyFile(toFile, fromFile);
                }
            }
        }

        /// <summary>
        /// Копирует эталонный файл в заданную папку
        /// </summary>
        /// <param name="destinationDirectoryPath">путь к папке назначения</param>
        /// <param name="relativeFilePath">относительный путь к эталонному файлу</param>
        internal static void CopySiteFile(string destinationDirectoryPath, string relativeFilePath)
        {
            CopyFile(destinationDirectoryPath, PathUtility.Combine(GetDirectoryPathToCopy(), relativeFilePath));
        }

        /// <summary>
        /// Копирует исходный файл по заданному пути с проверкой на дату модификации и снятием Read-Only атрибута
        /// </summary>
        /// <param name="destinationFilePath">путь назначения</param>
        /// <param name="sourceFilePath">путь источника</param>
        internal static void CopyFile(string destinationFilePath, string sourceFilePath)
        {
            var performCopy = false;
            if (File.Exists(sourceFilePath))
            {
                if (File.Exists(destinationFilePath))
                {
                    var sourceModified = File.GetLastWriteTime(sourceFilePath);
                    var destinationModified = File.GetLastWriteTime(destinationFilePath);
                    if (destinationModified.CompareTo(sourceModified) < 0)
                    {
                        performCopy = true;
                    }
                }
                else
                {
                    performCopy = true;
                }
            }

            if (performCopy)
            {
                if (File.Exists(destinationFilePath))
                {
                    File.SetAttributes(destinationFilePath, FileAttributes.Normal);
                }

                File.Copy(sourceFilePath, destinationFilePath, true);
                File.SetAttributes(destinationFilePath, FileAttributes.Normal);
            }
        }

        /// <summary>
        /// Возвращает путь к директории, в которой хранятся изображения общие для всех тем
        /// </summary>
        /// <returns>путь к директории, в которой хранятся изображения общие для всех тем</returns>
        internal static string GetCommonRootImageFolderUrl() =>
            GetUrlHelper().Content("~/Static/Common/");

        /// <summary>
        /// Возвращает путь к директории, в которой хранятся изображения указанной темы
        /// </summary>
        /// <param name="themeName">название темы</param>
        /// <returns>путь к директории, в которой хранятся изображения указанной темы</returns>
        internal static string GetThemeRootImageFolderUrl(string themeName) =>
            GetUrlHelper().Content("~/Static/" + themeName + "/");

        /// <summary>
        /// Возвращает путь к директории, в которой хранятся маленькие пиктограммы указанной темы
        /// </summary>
        /// <param name="themeName">название темы</param>
        /// <returns>путь к директории, в которой хранятся маленькие пиктограммы указанной темы</returns>
        internal static string GetThemeSmallIconsImageFolderUrl(string themeName) =>
            GetUrlHelper().Content( "~/Static/" + themeName + "/icons/16x16/");

        /// <summary>
        /// Возвращает путь к директории, в которой хранятся маленькие пиктограммы типов файлов
        /// </summary>
        /// <param name="themeName">название темы</param>
        internal static string GetThemeSmallFileTypeIconFolderUrl(string themeName) =>
            GetUrlHelper().Content( "~/Static/" + themeName + "/icons/16x16/file_types/");

        /// <summary>
        /// Возвращает путь к директории, в которой хранятся большие пиктограммы типов файлов
        /// </summary>
        /// <param name="themeName">название темы</param>
        internal static string GetThemeBigFileTypeIconFolderUrl(string themeName) =>
            GetUrlHelper().Content( "~/Static/" + themeName + "/icons/64x64/file_types/");

        /// <summary>
        /// Возвращает путь к директории, в которой хранятся индикаторы AJAX-загрузки указанной темы
        /// </summary>
        /// <param name="themeName">название темы</param>
        /// <returns>путь к директории, в которой хранятся индикаторы AJAX-загрузки указанной темы</returns>
        internal static string GetThemeAjaxLoaderIconsImageFolderUrl(string themeName) =>
            GetUrlHelper().Content( "~/Static/" + themeName + "/icons/ajax_loaders/");

        public static void CreateDirectories(string basePath)
        {
            Directory.CreateDirectory(basePath);
            Directory.CreateDirectory(PathUtility.Combine(basePath, RELATIVE_PREVIEW_PATH));
            Directory.CreateDirectory(PathUtility.Combine(basePath, RELATIVE_NOTIFICATIONS_PATH));
            Directory.CreateDirectory(PathUtility.Combine(basePath, RELATIVE_SETTINGS_PATH));
        }

        public static void CreateUploadDirectories(string basePath)
        {
            Directory.CreateDirectory(PathUtility.Combine(basePath, RELATIVE_CONTENTS_PATH));
            Directory.CreateDirectory(PathUtility.Combine(basePath, RELATIVE_TEMPLATES_PATH));
            Directory.CreateDirectory(PathUtility.Combine(basePath, RELATIVE_IMAGES_PATH));
        }

        public static void CreateBinDirectory(string binPath)
        {
            Directory.CreateDirectory(binPath);
            CopySiteDirectory(binPath, RELATIVE_BIN_PATH);
        }

    }
}

