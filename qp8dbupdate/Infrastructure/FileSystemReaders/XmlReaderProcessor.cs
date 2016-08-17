using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.FileSystemReaders
{
    internal static class XmlReaderProcessor
    {
        internal static string Process(IList<string> filePathes)
        {
            return Process(filePathes, null);
        }

        internal static string Process(IList<string> filePathes, string configPath)
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
            return CombineMultipleDocumentsWithSameRoot(orderedFilePathes.Select(XDocument.Load).ToList()).ToStringWithDeclaration();
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

        private static IEnumerable<string> GetOrderedDirectoryFilePathes(string absDirPath, string absOrRelativeConfigPath)
        {
            var config = string.IsNullOrWhiteSpace(absOrRelativeConfigPath)
                ? GetDefaultXmlReaderSettings(absDirPath)
                : GetXmlReaderSettings(absDirPath, absOrRelativeConfigPath);

            return config.FilePathes;
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static XDocument CombineMultipleDocumentsWithSameRoot(IList<XDocument> xmlDocuments)
        {
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
