using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace qp8dbupdate.Infrastructure
{
    public class EmbeddedAssembly
    {
        private static Dictionary<string, Assembly> _mapFullnameToAssembly;
        private static Dictionary<string, Assembly> _mapShortNameToAssembly;

        public static void Load(string embeddedResource, string fileName)
        {
            if (_mapFullnameToAssembly == null)
            {
                _mapFullnameToAssembly = new Dictionary<string, Assembly>();
            }

            if (_mapShortNameToAssembly == null)
            {
                _mapShortNameToAssembly = new Dictionary<string, Assembly>();
            }

            byte[] ba;
            Assembly asm;
            var curAsm = Assembly.GetExecutingAssembly();
            using (var stm = curAsm.GetManifestResourceStream(embeddedResource))
            {
                // Either the file is not existed or it is not mark as embedded resource
                if (stm == null)
                {
                    throw new Exception(embeddedResource + " is not found in Embedded Resources.");
                }

                // Get byte[] from the file from embedded resource
                ba = new byte[(int)stm.Length];
                stm.Read(ba, 0, (int)stm.Length);
                try
                {
                    // Add the assembly/dll into dictionary
                    asm = Assembly.Load(ba);
                    _mapFullnameToAssembly.Add(asm.FullName, asm);
                    _mapShortNameToAssembly.Add(asm.ManifestModule.ScopeName.Substring(0, asm.ManifestModule.ScopeName.Length - 4), asm);

                    return;
                }
                catch
                {
                    // Purposely do nothing
                    // Unmanaged dll or assembly cannot be loaded directly from byte[]
                    // Let the process fall through for next part
                }
            }

            bool fileOk;
            string tempFile;
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
                else
                {
                    fileOk = false;
                }
            }

            if (!fileOk)
            {
                File.WriteAllBytes(tempFile, ba);
            }

            asm = Assembly.LoadFile(tempFile);
            _mapFullnameToAssembly.Add(asm.FullName, asm);
            _mapShortNameToAssembly.Add(asm.ManifestModule.ScopeName.Substring(0, asm.ManifestModule.ScopeName.Length - 4), asm);
        }

        public static Assembly Get(string assemblyFullName)
        {
            if (_mapFullnameToAssembly == null || _mapFullnameToAssembly.Count == 0)
            {
                return null;
            }

            if (_mapFullnameToAssembly.ContainsKey(assemblyFullName))
            {
                return _mapFullnameToAssembly[assemblyFullName];
            }

            if (_mapShortNameToAssembly.ContainsKey(assemblyFullName))
            {
                return _mapShortNameToAssembly[assemblyFullName];
            }

            return null;
        }

        internal static void LoadMvcAssemblies()
        {
            Load("qp8dbupdate.References.System.Web.Helpers.dll", "System.Web.Helpers.dll");
            Load("qp8dbupdate.References.System.Web.Razor.dll", "System.Web.Helpers.dll");
            Load("qp8dbupdate.References.System.Web.WebPages.dll", "System.Web.WebPages.dll");
            Load("qp8dbupdate.References.System.Web.WebPages.Razor.dll", "System.Web.WebPages.Razor.dll");
            Load("qp8dbupdate.References.System.Web.WebPages.Deployment.dll", "System.Web.WebPages.Deployment.dll");
            Load("qp8dbupdate.References.System.Web.Mvc.dll", "System.Web.Helpers.dll");
            Load("qp8dbupdate.References.Microsoft.Owin.dll", "Microsoft.Owin.dll");
            Load("qp8dbupdate.References.Microsoft.AspNet.SignalR.Core.dll", "Microsoft.AspNet.SignalR.Core.dll");
            Load("qp8dbupdate.References.Microsoft.Practices.EnterpriseLibrary.Logging.dll", "Microsoft.Practices.EnterpriseLibrary.Logging.dll");
            Load("qp8dbupdate.References.Newtonsoft.Json.dll", "Newtonsoft.Json.dll");
        }

        internal static void LoadQpAssemblies()
        {
            Load("qp8dbupdate.References.Microsoft.Practices.Unity.dll", "Microsoft.Practices.Unity.dll");
            Load("qp8dbupdate.References.Microsoft.Practices.Unity.Configuration.dll", "Microsoft.Practices.Unity.Configuration.dll");
            Load("qp8dbupdate.References.Microsoft.Practices.ServiceLocation.dll", "Microsoft.Practices.ServiceLocation.dll");
            Load("qp8dbupdate.References.Microsoft.Practices.Unity.Interception.dll", "Microsoft.Practices.Unity.Interception.dll");
            Load("qp8dbupdate.References.Microsoft.Practices.EnterpriseLibrary.Common.dll", "Microsoft.Practices.EnterpriseLibrary.Common.dll");
            Load("qp8dbupdate.References.Microsoft.Practices.EnterpriseLibrary.Validation.dll", "Microsoft.Practices.EnterpriseLibrary.Validation.dll");
            Load("qp8dbupdate.References.Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.dll", "Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.dll");
            Load("qp8dbupdate.References.AutoMapper.dll", "AutoMapper.dll");
            Load("qp8dbupdate.References.EFExtensions.dll", "EFExtensions.dll");
            Load("qp8dbupdate.References.Irony.dll", "Irony.dll");
            Load("qp8dbupdate.References.Moq.dll", "Moq.dll");
            Load("qp8dbupdate.References.RazorGenerator.Mvc.dll", "RazorGenerator.Mvc.dll");
            Load("qp8dbupdate.References.Telerik.Web.Mvc.dll", "Telerik.Web.Mvc.dll");
            Load("qp8dbupdate.References.WebActivatorEx.dll", "WebActivatorEx.dll");
            Load("qp8dbupdate.References.Quantumart.QP8.Assembling.dll", "Quantumart.QP8.Assembling.dll");
            Load("qp8dbupdate.References.Quantumart.dll", "Quantumart.dll");
            Load("qp8dbupdate.References.QA.Configuration.dll", "QA.Configuration.dll");
            Load("qp8dbupdate.References.QA.Validation.Xaml.dll", "QA.Validation.Xaml.dll");
            Load("qp8dbupdate.References.QA.Validation.Xaml.Extensions.dll", "QA.Validation.Xaml.Extensions.dll");
            Load("qp8dbupdate.References.Quantumart.QP8.DAL.dll", "Quantumart.QP8.DAL.dll");
            Load("qp8dbupdate.References.Quantumart.QP8.Constants.dll", "Quantumart.QP8.Constants.dll");
            Load("qp8dbupdate.References.Quantumart.QP8.Resources.dll", "Quantumart.QP8.Resources.dll");
            Load("qp8dbupdate.References.Quantumart.QP8.Utils.dll", "Quantumart.QP8.Utils.dll");
            Load("qp8dbupdate.References.Quantumart.QP8.Validators.dll", "Quantumart.QP8.Validators.dll");
            Load("qp8dbupdate.References.Quantumart.QP8.Security.dll", "Quantumart.QP8.Security.dll");
            Load("qp8dbupdate.References.Quantumart.QP8.BLL.dll", "Quantumart.QP8.BLL.dll");
            Load("qp8dbupdate.References.Quantumart.QP8.Configuration.dll", "Quantumart.QP8.Configuration.dll");
            Load("qp8dbupdate.References.Quantumart.QP8.Logging.Loggers.dll", "Quantumart.QP8.Logging.Loggers.dll");
            Load("qp8dbupdate.References.Quantumart.QP8.Logging.Web.dll", "Quantumart.QP8.Logging.Web.dll");
            Load("qp8dbupdate.References.Quantumart.QP8.Logging.dll", "Quantumart.QP8.Logging.dll");
            Load("qp8dbupdate.References.Quantumart.QP8.WebMvc.dll", "Quantumart.QP8.WebMvc.dll");
        }
    }
}
