using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.FileSystemReaders
{
    internal static class XmlReaderProcessor
    {
        internal static string Process(IList<string> filePathes, string configPath, XmlSettingsModel settingsTemp = null)
        {
            Program.Logger.Debug($"Begin parsing documents: {filePathes.ToJsonLog()} with next config: {configPath ?? "<default>"}");
            var orderedFilePathes = new List<string>();
            foreach (var path in filePathes)
            {
                if (!File.Exists(path) && !Directory.Exists(path))
                {
                    throw new FileNotFoundException($"Неправильно указан путь к файлам записанных действий: {path}");
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

            Program.Logger.Info($"Total files readed from disk: {orderedFilePathes.Count}.");
            Program.Logger.Debug($"Documents will be processed in next order: { orderedFilePathes.ToJsonLog()}.");

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
                    var xmlString = XDocument.Parse(File.ReadAllText(ofp, Encoding.UTF8)).ToStringWithDeclaration(SaveOptions.DisableFormatting);
                    var logEntry = new XmlDbUpdateLogModel
                    {
                        Body = xmlString,
                        FileName = ofp,
                        Applied = DateTime.Now,
                        UserId = 1,
                        Hash = HashHelpers.CalculateMd5Hash(xmlString)
                    };

                    Program.Logger.Debug($"Old version (Windows) compatability enabled for {ofp}. Check hash {logEntry.Hash} in database.");
                    using (new QPConnectionScope(QPConfiguration.GetConnectionString(QPContext.CurrentCustomerCode), CommonHelpers.GetDbIdentityInsertOptions(settingsTemp.DisableFieldIdentity, settingsTemp.DisableContentIdentity)))
                    {
                        if (logService.IsFileAlreadyReplayed(logEntry.Hash))
                        {
                            Program.Logger.Warn($"XmlDbUpdate (old) conflict: current xml document(s) already applied, exist at database and will be skipped. Entry: {logEntry.ToJsonLog()}");
                            continue;
                        }
                    }

                    filteredOrderedFilePathes.Add(ofp);
                }
            }
            #endregion DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!!

            Program.Logger.Info($"Skipped files count: {orderedFilePathes.Count - filteredOrderedFilePathes.Count}.");
            Program.Logger.Info($"Total files will be processed: {filteredOrderedFilePathes.Count}.");
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
