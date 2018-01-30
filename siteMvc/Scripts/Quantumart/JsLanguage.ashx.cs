using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.WebMvc.Scripts.Quantumart
{
    public class JsLanguage : IHttpHandler, IReadOnlySessionState
    {
        private const string LangScriptFolderPath = "/Scripts/Quantumart/languages/";

        public bool IsReusable => false;

        private readonly List<string> _scriptFileNames = new List<string>
        {
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
            "BackendDocumentHost.Lang.js",
            "BackendTextArea.Lang.js",
            "BackendChangePasswordWindow.Lang.js"
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

            return File.Exists(neutralScriptPath) ? neutralScriptPath : scriptPath;
        }

        public string GetPhysicalFilePath(HttpServerUtility server, string scriptName) => server.MapPath(PathUtility.Combine(SitePathHelper.GetCurrentRootUrl(), LangScriptFolderPath, scriptName));
    }
}
