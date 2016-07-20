using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using qp8dbupdate.Infrastructure.Extensions;

namespace qp8dbupdate
{
    internal static class XmlReaderProcessor
    {
        internal static string GetNodesToReplay(string absDataPath, string configPath)
        {
            if (string.IsNullOrWhiteSpace(absDataPath))
            {
                throw new FileNotFoundException("Неправильно указан путь к файлам записанных действий");
            }

            var parsedDocument = (File.GetAttributes(absDataPath) & FileAttributes.Directory) == FileAttributes.Directory
                ? ReadDirectoryFiles(absDataPath, configPath)
                : ReadSingleFile(absDataPath);

            return FilterFromSubRootNodeDuplicates(parsedDocument).ToStringWithDeclaration();
        }

        private static XmlReaderSettings GetDefaultSettings(string absDirPath)
        {
            return new XmlReaderSettings(Directory.EnumerateFiles(absDirPath, "*.xml", SearchOption.TopDirectoryOnly).OrderBy(fn => fn).ToList());
        }

        private static XmlReaderSettings GetXmlFileSettings(string absDirPath, string configPath)
        {
            var absConfigPath = Path.Combine(absDirPath, configPath);
            if (!File.Exists(absConfigPath))
            {
                throw new FileNotFoundException("Неправильно указан путь к конфигурационному файлу");
            }

            var xmlData = XDocument.Load(absConfigPath);
            if (xmlData.Root == null || xmlData.Root.IsEmpty)
            {
                throw new Exception("Неверный формат конфигурационного файла");
            }

            var actionNodes = xmlData.Root.Elements(XmlReaderSettings.ConfigElementNodeName);
            return new XmlReaderSettings(actionNodes.Select(node => Path.Combine(absDirPath, node.Attribute(XmlReaderSettings.ConfigElementPathAttribute).Value)).ToList());
        }

        private static XDocument ReadSingleFile(string absFilePath)
        {
            return XDocument.Load(absFilePath);
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static XDocument ReadDirectoryFiles(string absDirPath, string configPath)
        {
            var config = string.IsNullOrWhiteSpace(configPath) ? GetDefaultSettings(absDirPath) : GetXmlFileSettings(absDirPath, configPath);
            var xmlFileDatas = config.RecordedXmlFilePathes.Select(XDocument.Load).ToList();
            var root = new XDocument(xmlFileDatas[0].Root);
            root.Root.RemoveNodes();
            return xmlFileDatas.Aggregate(root, (result, xd) =>
            {
                result.Root.Add(xd.Root.Nodes());
                return result;
            });
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static XDocument FilterFromSubRootNodeDuplicates(XDocument document)
        {
            var comparer = new XNodeEqualityComparer();
            var distinctNodes = document.Root.Nodes().Distinct(comparer);
            foreach (var node in distinctNodes)
            {
                document.Root.Nodes().Where(n => comparer.Equals(n, node)).Skip(1).Remove();
            }

            return document;
        }
    }
}
