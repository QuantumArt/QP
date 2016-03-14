using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web;
using Quantumart.QP8;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.WebMvc.Backend
{
	public class JsLanguage : IHttpHandler
	{
		const string LANG_SCRIPT_FOLDER_PATH = "/Scripts/Languages/";

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
			"BackendTraceWindow.Lang.js",
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

		public bool IsReusable
		{
			get
			{
				return false;
			}
		}

		public void ProcessRequest(HttpContext context)
		{
			HttpResponse response = context.Response;
			HttpServerUtility server = context.Server;

			string cultureName = Thread.CurrentThread.CurrentCulture.Name.ToLowerInvariant();
			bool useMinifiedScripts = !HttpContext.Current.IsDebuggingEnabled;

			response.ContentType = "text/javascript";
			foreach (string scriptFileName in this._scriptFileNames)
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
			string scriptCode = String.Empty;
			string scriptPath = GetLanguageScriptPath(server, scriptName, cultureName, useMinifiedScript);

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
			string scriptPath = String.Empty;

			string scriptNameWithoutExtension = Path.GetFileNameWithoutExtension(scriptName);
			string scriptExtension = Path.GetExtension(scriptName);

			string neutralScriptPath = GetPhysicalFilePath(server, scriptName);
			string neutralMinifiedScriptPath = GetPhysicalFilePath(server,
				scriptNameWithoutExtension + 
				(useMinifiedScript ? ".min" : "") + 
				scriptExtension
			);
			string localizedScriptPath = GetPhysicalFilePath(server,
				scriptNameWithoutExtension + 
				(cultureName.Length > 0 ? "." + cultureName : "") + 
				scriptExtension
			);
			string localizedMinifiedScriptPath = GetPhysicalFilePath(server,
				scriptNameWithoutExtension +
				(cultureName.Length > 0 ? "." + cultureName : "") +
				(useMinifiedScript ? ".min" : "") + 
				scriptExtension
			);

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
			string filePath = server.MapPath(
				PathUtility.Combine(SitePathHelper.GetCurrentRootUrl(), LANG_SCRIPT_FOLDER_PATH, scriptName)
			);

			return filePath;
		}
	}
}