using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Utils;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;

namespace Quantumart.QP8.WebMvc.Backend
{
    public class JsLanguage : IHttpHandler
	{
	    private const string LangScriptFolderPath = "/Scripts/Quantumart/languages/";
		public bool IsReusable => false;

		private readonly List<string> _scriptFileNames = new List<string>() {
			"BackendCommon.Lang.js",
			"BackendActionExecutor.Lang.js",
			"BackendHome.Lang.js",
			"BackendEditingArea.Lang.js",
			"BackendPopupWindow.Lang.js",
			"BackendEntityEditor.Lang.js",
			"BackendTabStrip.Lang.js",
			"BackendContextMenu.Lang.js",
			"BackendToolbar.Lang.js",
			"BackendFileField.Lang.js",
			"BackendActionLink.Lang.js",
			"BackendEntityDataList.Lang.js",
			"BackendSearchBlock.Lang.js",
			"BackendBreadCrumbs.Lang.js",
			"BackendPager.Lang.js",
			"BackendFileList.Lang.js",
			"BackendEntityGrid.Lang.js",
			"BackendMultistepAction.Lang.js",
			"BackendPermission.Lang.js",
			"BackendDirectLinkExecutor.Lang.js",
			"BackendExpandedContainer.Lang.js",
			"BackendEntityEditorAutoSaver.Lang.js",
			"BackendImageCropResizeClient.Lang.js",
			"BackendCommunacation.Lang.js",
            "BackendDocumentHost.Lang.js"
        };

	    public void ProcessRequest(HttpContext context)
		{
			var response = context.Response;
			var server = context.Server;

			var cultureName = Thread.CurrentThread.CurrentCulture.Name.ToLowerInvariant();
			var useMinifiedScripts = !HttpContext.Current.IsDebuggingEnabled;

			response.ContentType = "text/javascript";
			foreach (var scriptFileName in _scriptFileNames)
			{
				response.Write(GetLanguageScriptCode(server, scriptFileName, cultureName, useMinifiedScripts));
			}
		}

		/// <summary>
		/// Возврашает код локализационного скрипта
		/// </summary>
		/// <param name="server">объект HttpServerUtility</param>
		/// <param name="scriptName">имя скрипта</param>
		/// <param name="cultureName">название текущей культуры</param>
		/// <param name="useMinifiedScript">признак, разрешающий минимизацию скрипта</param>
		/// <returns>код локализационного скрипта</returns>
		private string GetLanguageScriptCode(HttpServerUtility server, string scriptName, string cultureName, bool useMinifiedScript)
		{
			var scriptCode = string.Empty;
			var scriptPath = GetLanguageScriptPath(server, scriptName, cultureName, useMinifiedScript);

			if (scriptPath.Length > 0)
			{
				scriptCode = File.ReadAllText(scriptPath);
			}

			return scriptCode;
		}

		/// <summary>
		/// Возвращает путь к локализационному скрипту
		/// </summary>
		/// <param name="server">объект HttpServerUtility</param>
		/// <param name="scriptName">имя скрипта</param>
		/// <param name="cultureName">название текущей культуры</param>
		/// <param name="useMinifiedScript">признак, разрешающий минимизацию скрипта</param>
		/// <returns>путь к локализационному скрипту</returns>
		private string GetLanguageScriptPath(HttpServerUtility server, string scriptName, string cultureName, bool useMinifiedScript)
		{
			var scriptPath = string.Empty;

			var scriptNameWithoutExtension = Path.GetFileNameWithoutExtension(scriptName);
			var scriptExtension = Path.GetExtension(scriptName);

			var neutralScriptPath = GetPhysicalFilePath(server, scriptName);
			var neutralMinifiedScriptPath = GetPhysicalFilePath(server, scriptNameWithoutExtension + (useMinifiedScript ? ".min" : "") + scriptExtension);
			var localizedScriptPath = GetPhysicalFilePath(server, scriptNameWithoutExtension + (cultureName.Length > 0 ? "." + cultureName : "") + scriptExtension);
			var localizedMinifiedScriptPath = GetPhysicalFilePath(server, scriptNameWithoutExtension + (cultureName.Length > 0 ? "." + cultureName : "") + (useMinifiedScript ? ".min" : "") + scriptExtension);

			if (cultureName.Length > 0)
			{
				if (useMinifiedScript && File.Exists(localizedMinifiedScriptPath))
				{
					return localizedMinifiedScriptPath;
				}

				if (File.Exists(localizedScriptPath))
				{
					return localizedScriptPath;
				}
			}

			if (useMinifiedScript && File.Exists(neutralMinifiedScriptPath))
			{
				return neutralMinifiedScriptPath;
			}

			if (File.Exists(neutralScriptPath))
			{
				return neutralScriptPath;
			}

			return scriptPath;
		}

		/// <summary>
		/// Возвращает физический путь к файлу
		/// </summary>
		/// <param name="server">объект HttpServerUtility</param>
		/// <param name="scriptName">имя скрипта</param>
		/// <returns>физический путь к файлу</returns>
		public string GetPhysicalFilePath(HttpServerUtility server, string scriptName)
		{
			var filePath = server.MapPath(PathUtility.Combine(SitePathHelper.GetCurrentRootUrl(), LangScriptFolderPath, scriptName));
			return filePath;
		}
	}
}
