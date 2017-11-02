using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            ConsoleHelpers.WriteLineDebug("Prepare to load embedded assemblies");
            var embeddedResourceNames = typeof(Program).Assembly.GetManifestResourceNames();
            var defaultNamespace = typeof(Program).Namespace ?? "ConsoleDbUpdate";
            foreach (var resourceName in embeddedResourceNames.Where(resourceName => resourceName.EndsWith(".dll")))
            {
                Load(resourceName, resourceName.Replace($"{defaultNamespace}.EmbeddedResources.", string.Empty));
            }

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
    }
}
