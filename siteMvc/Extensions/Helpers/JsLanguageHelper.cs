using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public class JsLanguageHelper
    {
        private const string LangScriptFolderPath = "/Scripts/Quantumart/languages/";
        private IWebHostEnvironment _environment;

        public JsLanguageHelper(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

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

        public string GetResult()
        {
            var sb = new StringBuilder();
            var cultureName = CultureInfo.CurrentCulture.Name.ToLowerInvariant();

            foreach (var scriptFileName in _scriptFileNames)
            {
                sb.Append(GetLanguageScriptCode(scriptFileName, cultureName, false));
            }

            return sb.ToString();
        }

        private string GetLanguageScriptCode(string scriptName, string cultureName, bool useMinifiedScript)
        {
            var scriptCode = string.Empty;
            var scriptPath = GetLanguageScriptPath(scriptName, cultureName, useMinifiedScript);

            if (scriptPath.Length > 0)
            {
                scriptCode = File.ReadAllText(scriptPath);
            }

            return scriptCode;
        }

        private string GetLanguageScriptPath(string scriptName, string cultureName, bool useMinifiedScript)
        {
            var scriptPath = string.Empty;
            var scriptNameWithoutExtension = Path.GetFileNameWithoutExtension(scriptName);
            var scriptExtension = Path.GetExtension(scriptName);
            var neutralScriptPath = GetPhysicalFilePath(scriptName);
            var neutralMinifiedScriptPath = GetPhysicalFilePath(scriptNameWithoutExtension + (useMinifiedScript ? ".min" : "") + scriptExtension);
            var localizedScriptPath = GetPhysicalFilePath(scriptNameWithoutExtension + (cultureName.Length > 0 ? "." + cultureName : "") + scriptExtension);
            var localizedMinifiedScriptPath = GetPhysicalFilePath(scriptNameWithoutExtension + (cultureName.Length > 0 ? "." + cultureName : "") + (useMinifiedScript ? ".min" : "") + scriptExtension);
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

        public string GetPhysicalFilePath(string scriptName)
        {
            return _environment.ContentRootFileProvider.GetFileInfo(
                PathUtility.Combine(LangScriptFolderPath, scriptName)
            ).PhysicalPath;
        }
    }
}
