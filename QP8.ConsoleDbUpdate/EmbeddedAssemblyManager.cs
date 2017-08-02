using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Helpers;

namespace Quantumart.QP8.ConsoleDbUpdate
{
    public class EmbeddedAssemblyManager
    {
        private static Dictionary<string, Assembly> _mapFullnameToAssembly;
        private static Dictionary<string, Assembly> _mapShortNameToAssembly;

        public static void LoadAssembliesAndAttachEvents()
        {
            LoadVendorAssemblies();
            LoadQpAssemblies();

            AppDomain.CurrentDomain.AssemblyResolve += (obj, args) => Get(args.Name);
            ConsoleHelpers.WriteLineDebug();
        }

        public static void Load(string embeddedResourceName, string fileName)
        {
            ConsoleHelpers.WriteDebug($"Loading assembly. EmbeddedResourceName: {embeddedResourceName}. FileName: {fileName}");

            if (_mapFullnameToAssembly == null)
            {
                _mapFullnameToAssembly = new Dictionary<string, Assembly>();
            }

            if (_mapShortNameToAssembly == null)
            {
                _mapShortNameToAssembly = new Dictionary<string, Assembly>();
            }

            using (var stm = Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedResourceName))
            {
                if (stm == null)
                {
                    throw new Exception(embeddedResourceName + " is not found in Embedded Resources");
                }

                var ba = new byte[(int)stm.Length];
                stm.Read(ba, 0, (int)stm.Length);
                try
                {
                    var asm = Assembly.Load(ba);
                    _mapFullnameToAssembly.Add(asm.FullName, asm);
                    _mapShortNameToAssembly.Add(asm.ManifestModule.ScopeName.Substring(0, asm.ManifestModule.ScopeName.Length - 4), asm);
                }
                catch
                {
                    ConsoleHelpers.WriteLineDebug("Loading was failed. Try to load from temp path");
                    LoadFromTempPath(fileName, ba);
                }

                ConsoleHelpers.WriteLineDebug(" .. Ok");
            }
        }

        private static void LoadFromTempPath(string fileName, byte[] ba)
        {
            string tempFile;
            var fileOk = false;
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                var fileHash = BitConverter.ToString(sha1.ComputeHash(ba)).Replace("-", string.Empty);
                tempFile = Path.GetTempPath() + fileName;
                if (File.Exists(tempFile))
                {
                    var bb = File.ReadAllBytes(tempFile);
                    var fileHash2 = BitConverter.ToString(sha1.ComputeHash(bb)).Replace("-", string.Empty);
                    fileOk = fileHash == fileHash2;
                }
            }

            if (!fileOk)
            {
                File.WriteAllBytes(tempFile, ba);
            }

            var asm = Assembly.LoadFile(tempFile);
            _mapFullnameToAssembly.Add(asm.FullName, asm);
            _mapShortNameToAssembly.Add(asm.ManifestModule.ScopeName.Substring(0, asm.ManifestModule.ScopeName.Length - 4), asm);
        }

        public static Assembly Get(string assemblyFullName)
        {
            ConsoleHelpers.WriteDebug($"Resolving assembly: {assemblyFullName}");
            if (_mapFullnameToAssembly == null || _mapFullnameToAssembly.Count == 0)
            {
                return null;
            }

            if (_mapFullnameToAssembly.ContainsKey(assemblyFullName))
            {
                ConsoleHelpers.WriteLineDebug(" .. Ok");
                return _mapFullnameToAssembly[assemblyFullName];
            }

            if (_mapShortNameToAssembly.ContainsKey(assemblyFullName))
            {
                ConsoleHelpers.WriteLineDebug(" .. Ok");
                return _mapShortNameToAssembly[assemblyFullName];
            }

            if (assemblyFullName.Contains(","))
            {
                var shortenedName = assemblyFullName.Substring(0, assemblyFullName.IndexOf(",", StringComparison.Ordinal));
                if (_mapShortNameToAssembly.ContainsKey(shortenedName))
                {
                    ConsoleHelpers.WriteLineDebug(" .. Ok");
                    return _mapShortNameToAssembly[shortenedName];
                }
            }

            ConsoleHelpers.WriteLineDebug(" .. Fail");
            throw new Exception($"{assemblyFullName} couldn't be successfully resolved");
        }

        internal static void LoadVendorAssemblies()
        {
            Load("Quantumart.QP8.ConsoleDbUpdate.References.System.Web.Helpers.dll", "System.Web.Helpers.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.System.Web.Razor.dll", "System.Web.Helpers.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.System.Web.WebPages.dll", "System.Web.WebPages.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.System.Web.WebPages.Razor.dll", "System.Web.WebPages.Razor.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.System.Web.WebPages.Deployment.dll", "System.Web.WebPages.Deployment.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Microsoft.Owin.dll", "Microsoft.Owin.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Microsoft.AspNet.SignalR.Core.dll", "Microsoft.AspNet.SignalR.Core.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Newtonsoft.Json.dll", "Newtonsoft.Json.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.System.Web.Mvc.dll", "System.Web.Helpers.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Microsoft.Web.Infrastructure.dll", "Microsoft.Web.Infrastructure.dll");

            Load("Quantumart.QP8.ConsoleDbUpdate.References.Microsoft.Practices.ServiceLocation.dll", "Microsoft.Practices.ServiceLocation.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Microsoft.Practices.Unity.dll", "Microsoft.Practices.Unity.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Microsoft.Practices.Unity.Interception.dll", "Microsoft.Practices.Unity.Interception.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Microsoft.Practices.Unity.Configuration.dll", "Microsoft.Practices.Unity.Configuration.dll");

            Load("Quantumart.QP8.ConsoleDbUpdate.References.Microsoft.Practices.EnterpriseLibrary.Data.dll", "Microsoft.Practices.EnterpriseLibrary.Data.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Microsoft.Practices.EnterpriseLibrary.Common.dll", "Microsoft.Practices.EnterpriseLibrary.Common.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Microsoft.Practices.EnterpriseLibrary.Validation.dll", "Microsoft.Practices.EnterpriseLibrary.Validation.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.dll", "Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.dll");

            Load("Quantumart.QP8.ConsoleDbUpdate.References.AutoMapper.dll", "AutoMapper.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.EFExtensions.dll", "EFExtensions.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Irony.dll", "Irony.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Castle.Core.dll", "Castle.Core.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Moq.dll", "Moq.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.RazorGenerator.Mvc.dll", "RazorGenerator.Mvc.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Telerik.Web.Mvc.dll", "Telerik.Web.Mvc.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.WebActivatorEx.dll", "WebActivatorEx.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Mono.Options.dll", "Mono.Options.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.CsvHelper.dll", "CsvHelper.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.NLog.dll", "NLog.dll");
        }

        internal static void LoadQpAssemblies()
        {
            Load("Quantumart.QP8.ConsoleDbUpdate.References.QP8.Infrastructure.dll", "QP8.Infrastructure.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.QP8.Infrastructure.Web.dll", "QP8.Infrastructure.Web.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.QP8.Infrastructure.Logging.dll", "QP8.Infrastructure.Logging.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.QA.Configuration.dll", "QA.Configuration.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.QA.Validation.Xaml.dll", "QA.Validation.Xaml.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.QA.Validation.Xaml.Extensions.dll", "QA.Validation.Xaml.Extensions.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Quantumart.dll", "Quantumart.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Quantumart.QP8.Assembling.dll", "Quantumart.QP8.Assembling.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Quantumart.QP8.Constants.dll", "Quantumart.QP8.Constants.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Quantumart.QP8.DAL.dll", "Quantumart.QP8.DAL.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Quantumart.QP8.Resources.dll", "Quantumart.QP8.Resources.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Quantumart.QP8.Utils.dll", "Quantumart.QP8.Utils.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Quantumart.QP8.Validators.dll", "Quantumart.QP8.Validators.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Quantumart.QP8.Security.dll", "Quantumart.QP8.Security.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Quantumart.QP8.BLL.dll", "Quantumart.QP8.BLL.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Quantumart.QP8.Merger.dll", "Quantumart.QP8.Merger.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Quantumart.QP8.Configuration.dll", "Quantumart.QP8.Configuration.dll");
            Load("Quantumart.QP8.ConsoleDbUpdate.References.Quantumart.QP8.WebMvc.dll", "Quantumart.QP8.WebMvc.dll");
        }
    }
}
