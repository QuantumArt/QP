using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Models.XmlDbUpdate;
using Quantumart.QP8.BLL.Repository.XmlDbUpdate;
using Quantumart.QP8.BLL.Services.XmlDbUpdate;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Enums;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Helpers;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.FileSystemReaders
{
    internal static class XmlReaderProcessor
    {
        internal static string Process(IList<string> filePathes, string configPath, XmlSettingsModel settingsTemp = null)
        {
            Program.Logger.Debug($"Begin parsing documents: {filePathes.ToJsonLog()} with next config: {configPath}");
            var orderedFilePathes = new List<string>();
            foreach (var path in filePathes)
            {
                if (!File.Exists(path) && !Directory.Exists(path))
                {
                    throw new FileNotFoundException("Неправильно указан путь к файлам записанных действий: " + path);
                }

                if ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    orderedFilePathes.AddRange(GetOrderedDirectoryFilePathes(path, configPath));
                }
                else
                {
                    orderedFilePathes.Add(path);
                }
            }

            Program.Logger.Debug($"Documents will be processed in next order: {orderedFilePathes.ToJsonLog()}");
            var filteredOrderedFilePathes = new List<string>();
            //TODO: DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!! And remove unusing references then.
            #region DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!!
            if (settingsTemp == null)
            {
                filteredOrderedFilePathes = orderedFilePathes;
            }
            else
            {
                foreach (var ofp in orderedFilePathes)
                {
                    var logService = new XmlDbUpdateLogService(new XmlDbUpdateLogRepository(), new XmlDbUpdateActionsLogRepository());
                    var xmlString = File.ReadAllText(ofp, Encoding.UTF8);
                    var logEntry = new XmlDbUpdateLogModel
                    {
                        Body = xmlString,
                        FileName = ofp,
                        Applied = DateTime.Now,
                        UserId = 1
                    };

                    var md5 = MD5.Create();
                    var inputBytes = Encoding.UTF8.GetBytes(xmlString);
                    var hash = md5.ComputeHash(inputBytes);
                    var sb = new StringBuilder();
                    foreach (var t in hash)
                    {
                        sb.Append(t.ToString("X2"));
                    }

                    logEntry.Hash = sb.ToString();

                    var identityTypes = new HashSet<string>();
                    if (!settingsTemp.DisableFieldIdentity)
                    {
                        identityTypes.Add(EntityTypeCode.Field);
                        identityTypes.Add(EntityTypeCode.ContentLink);
                    }

                    if (!settingsTemp.DisableContentIdentity)
                    {
                        identityTypes.Add(EntityTypeCode.Content);
                        identityTypes.Add(EntityTypeCode.ContentGroup);
                    }

                    Program.Logger.Debug($"Old version (Mixed) compatability enabled. Check hash {logEntry.Hash} in database.");
                    using (new QPConnectionScope(QPConfiguration.ConfigConnectionString(QPContext.CurrentCustomerCode), identityTypes))
                    {
                        if (logService.IsFileAlreadyReplayed(logEntry.Hash))
                        {
                            Program.Logger.Debug($"Current xml document {ofp} already applied and exist at XmlDbUpdate database.");
                            continue;
                        }
                    }



                    inputBytes = Encoding.UTF8.GetBytes(xmlString.Replace("\r\n", "\n"));
                    hash = md5.ComputeHash(inputBytes);
                    sb = new StringBuilder();
                    foreach (var t in hash)
                    {
                        sb.Append(t.ToString("X2"));
                    }

                    logEntry.Hash = sb.ToString();

                    Program.Logger.Debug($"Old version (Unix) compatability enabled. Check hash {logEntry.Hash} in database.");
                    using (new QPConnectionScope(QPConfiguration.ConfigConnectionString(QPContext.CurrentCustomerCode), identityTypes))
                    {
                        if (logService.IsFileAlreadyReplayed(logEntry.Hash))
                        {
                            Program.Logger.Debug($"Current xml document {ofp} already applied and exist at XmlDbUpdate database.");
                            continue;
                        }
                    }



                    inputBytes = Encoding.UTF8.GetBytes(xmlString.Replace("\n", "\r\n"));
                    hash = md5.ComputeHash(inputBytes);
                    sb = new StringBuilder();
                    foreach (var t in hash)
                    {
                        sb.Append(t.ToString("X2"));
                    }

                    logEntry.Hash = sb.ToString();

                    Program.Logger.Debug($"Old version (Windows) compatability enabled. Check hash {logEntry.Hash} in database.");
                    using (new QPConnectionScope(QPConfiguration.ConfigConnectionString(QPContext.CurrentCustomerCode), identityTypes))
                    {
                        if (logService.IsFileAlreadyReplayed(logEntry.Hash))
                        {
                            Program.Logger.Debug($"Current xml document {ofp} already applied and exist at XmlDbUpdate database.");
                            continue;
                        }
                    }



                    filteredOrderedFilePathes.Add(ofp);
                }
            }
            #endregion DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!!

            Program.Logger.Debug($"Documents will be processed in next order: {filteredOrderedFilePathes.ToJsonLog()}");
            return CombineMultipleDocumentsWithSameRoot(filteredOrderedFilePathes.Select(XDocument.Load).ToList()).ToStringWithDeclaration(SaveOptions.DisableFormatting);
        }

        private static IEnumerable<string> GetOrderedDirectoryFilePathes(string absDirPath, string absOrRelativeConfigPath)
        {
            var config = string.IsNullOrWhiteSpace(absOrRelativeConfigPath)
                ? GetDefaultXmlReaderSettings(absDirPath)
                : GetXmlReaderSettings(absDirPath, absOrRelativeConfigPath);

            return config.FilePathes;
        }

        private static XmlReaderSettings GetDefaultXmlReaderSettings(string absDirPath)
        {
            return new XmlReaderSettings(Directory.EnumerateFiles(absDirPath, "*.xml", SearchOption.TopDirectoryOnly).OrderBy(fn => fn).ToList());
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static XmlReaderSettings GetXmlReaderSettings(string absDirPath, string absOrRelativeConfigPath)
        {
            var configPath = absOrRelativeConfigPath;
            if (!File.Exists(configPath))
            {
                configPath = Path.Combine(absDirPath, absOrRelativeConfigPath);
                if (!File.Exists(configPath))
                {
                    throw new FileNotFoundException("Неправильно указан путь к файлу конфигурации: " + configPath);
                }
            }

            var xmlData = XDocument.Load(configPath);
            var actionNodes = xmlData.Root.Elements(XmlReaderSettings.ConfigElementNodeName).Where(el => el.NodeType != XmlNodeType.Comment);
            return new XmlReaderSettings(actionNodes.Select(node => Path.Combine(absDirPath, node.Attribute(XmlReaderSettings.ConfigElementPathAttribute).Value)).ToList());
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static XDocument CombineMultipleDocumentsWithSameRoot(IList<XDocument> xmlDocuments)
        {
            if (!xmlDocuments.Any())
            {
                Program.Logger.Warn("There are no xml files for replay or all of them already used before.");
                ConsoleHelpers.ExitProgram(ExitCode.Success);
            }

            if (xmlDocuments.Count == 1)
            {
                return xmlDocuments.Single();
            }

            var root = new XDocument(xmlDocuments[0].Root);
            root.Root.RemoveNodes();

            return xmlDocuments.Aggregate(root, (result, xd) =>
            {
                result.Root.Add(xd.Root.Elements());
                return result;
            });
        }
    }
}
