using System;
using System.Text;
using System.IO;
using System.Web;
using System.Web.Configuration;
using Quantumart.QP8;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.BLL.Repository
{
	internal class SitePathRepository
	{
		public static string RELATIVE_PATH_TO_COPY = @"\ToCopy";
		public static string RELATIVE_PREVIEW_PATH = @"\temp\preview\objects";
		public static string RELATIVE_NOTIFICATIONS_PATH = @"\qp_notifications";
		public static string RELATIVE_SETTINGS_PATH = @"\qp_settings";
		public static string RELATIVE_CONTENTS_PATH = @"\contents";
		public static string RELATIVE_TEMPLATES_PATH = @"\templates";
		public static string RELATIVE_IMAGES_PATH = @"\images";
		public static string RELATIVE_BIN_PATH = @"\bin";
		public static string RELATIVE_APP_DATA_PATH = @"\App_Data";
		public static string RELATIVE_APP_CODE_PATH = @"\App_Code";

		/// <summary>
		/// Возвращает URL, по которому расположен бэкенд
		/// </summary>
		/// <returns>URL бэкенда</returns>
		internal static string GetCurrentRootUrl()
		{
			// Получаем настройки конфигурации
			QPublishingSection qpConfig = WebConfigurationManager.GetSection("qpublishing") as QPublishingSection;
			return (qpConfig == null) ? String.Empty : qpConfig.BackendUrl;
		}

		/// <summary>
		/// Возвращает путь к директории, в которой раcположены файлы
		/// для копирования в новый и обновляемый сайт
		/// </summary>
		/// <returns>путь</returns>
		internal static string GetDirectoryPathToCopy()
		{
			string rootUrl = GetCurrentRootUrl();
			return (HttpContext.Current == null || String.IsNullOrEmpty(rootUrl)) ? String.Empty : HttpContext.Current.Server.MapPath(rootUrl + RELATIVE_PATH_TO_COPY);
		}

		/// <summary>
		/// Копирует все эталонные файлы из исходной папки в заданую папку
		/// </summary>
		/// <param name="destinationDirectoryPath">путь к папке назначения</param>
		/// <param name="relativeDirectoryPath">относительный путь к папке с эталонными файлами</param>
		internal static void CopySiteDirectory(string destinationDirectoryPath, string relativeDirectoryPath)
		{
			string basePathToCopy = GetDirectoryPathToCopy();
			if (!String.IsNullOrEmpty(basePathToCopy))
			{
				string directoryPath = Utils.PathUtility.Combine(basePathToCopy, relativeDirectoryPath);

				foreach (string filePath in Directory.GetFiles(directoryPath))
				{
					string fileName = Path.GetFileName(filePath);
					string fromFile = Utils.PathUtility.Combine(directoryPath, fileName);
					string toFile = Utils.PathUtility.Combine(destinationDirectoryPath, fileName);
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
			CopyFile(destinationDirectoryPath, Utils.PathUtility.Combine(GetDirectoryPathToCopy(), relativeFilePath));
		}

		/// <summary>
		/// Копирует исходный файл по заданному пути с проверкой на дату модификации и снятием Read-Only атрибута
		/// </summary>
		/// <param name="destinationFilePath">путь назначения</param>
		/// <param name="sourceFilePath">путь источника</param>
		internal static void CopyFile(string destinationFilePath, string sourceFilePath)
		{
			bool performCopy = false;

			if (File.Exists(sourceFilePath))
			{
				if (File.Exists(destinationFilePath))
				{
					DateTime sourceModified = File.GetLastWriteTime(sourceFilePath);
					DateTime destinationModified = File.GetLastWriteTime(destinationFilePath);

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

		public static void CreateDirectories(string basePath)
		{
			Directory.CreateDirectory(basePath);
			Directory.CreateDirectory(Utils.PathUtility.Combine(basePath, RELATIVE_PREVIEW_PATH));
			Directory.CreateDirectory(Utils.PathUtility.Combine(basePath, RELATIVE_NOTIFICATIONS_PATH));
			Directory.CreateDirectory(Utils.PathUtility.Combine(basePath, RELATIVE_SETTINGS_PATH));
		}

		public static void CreateUploadDirectories(string basePath)
		{

			Directory.CreateDirectory(Utils.PathUtility.Combine(basePath, RELATIVE_CONTENTS_PATH));
			Directory.CreateDirectory(Utils.PathUtility.Combine(basePath, RELATIVE_TEMPLATES_PATH));
			Directory.CreateDirectory(Utils.PathUtility.Combine(basePath, RELATIVE_IMAGES_PATH));
		}

		public static void CreateBinDirectory(string binPath)
		{
			Directory.CreateDirectory(binPath);
			SitePathRepository.CopySiteDirectory(binPath, RELATIVE_BIN_PATH);
		}

		/// <summary>
		/// Возвращает путь к директории, в которой хранятся изображения общие для всех тем
		/// </summary>
		/// <returns>путь к директории, в которой хранятся изображения общие для всех тем</returns>
		internal static string GetCommonRootImageFolderUrl()
		{
			return Utils.Url.ToAbsolute("~/Content/Common/");
		}

		/// <summary>
		/// Возвращает путь к директории, в которой хранятся изображения указанной темы
		/// </summary>
		/// <param name="themeName">название темы</param>
		/// <returns>путь к директории, в которой хранятся изображения указанной темы</returns>
		internal static string GetThemeRootImageFolderUrl(string themeName)
		{
			return Utils.Url.ToAbsolute("~/Content/" + themeName + "/");
		}

		/// <summary>
		/// Возвращает путь к директории, в которой хранятся маленькие пиктограммы указанной темы
		/// </summary>
		/// <param name="themeName">название темы</param>
		/// <returns>путь к директории, в которой хранятся маленькие пиктограммы указанной темы</returns>
		internal static string GetThemeSmallIconsImageFolderUrl(string themeName)
		{
			return Utils.Url.ToAbsolute("~/Content/" + themeName + "/icons/16x16/");
		}

		/// <summary>
		/// Возвращает путь к директории, в которой хранятся маленькие пиктограммы типов файлов
		/// </summary>
		/// <param name="themeName">название темы</param>		
		internal static string GetThemeSmallFileTypeIconFolderUrl(string themeName)
		{
			return Utils.Url.ToAbsolute("~/Content/" + themeName + "/icons/16x16/file_types/");
		}

		/// <summary>
		/// Возвращает путь к директории, в которой хранятся большие пиктограммы типов файлов
		/// </summary>
		/// <param name="themeName">название темы</param>		
		internal static string GetThemeBigFileTypeIconFolderUrl(string themeName)
		{
			return Utils.Url.ToAbsolute("~/Content/" + themeName + "/icons/64x64/file_types/");
		}

		/// <summary>
		/// Возвращает путь к директории, в которой хранятся индикаторы AJAX-загрузки указанной темы
		/// </summary>
		/// <param name="themeName">название темы</param>
		/// <returns>путь к директории, в которой хранятся индикаторы AJAX-загрузки указанной темы</returns>
		internal static string GetThemeAjaxLoaderIconsImageFolderUrl(string themeName)
		{
			return Utils.Url.ToAbsolute("~/Content/" + themeName + "/icons/ajax_loaders/");
		}
	}
}
