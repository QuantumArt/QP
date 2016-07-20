using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using qp8dbupdate.Infrastructure.Extensions;

namespace qp8dbupdate
{
    internal static class XmlReaderProcessor
    {
        internal static string GetNodesToReplay(string absDataPath, string absOrRelativeConfigPath)
        {
            if (!File.Exists(absDataPath) && !Directory.Exists(absDataPath))
            {
                throw new FileNotFoundException("Неправильно указан путь к файлам записанных действий: " + absDataPath);
            }

            var parsedDocument = (File.GetAttributes(absDataPath) & FileAttributes.Directory) == FileAttributes.Directory
                ? ReadDirectoryFiles(absDataPath, absOrRelativeConfigPath)
                : ReadSingleFile(absDataPath);

            return FilterFromSubRootNodeDuplicates(parsedDocument).ToStringWithDeclaration();
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

        private static XDocument ReadSingleFile(string absFilePath)
        {
            return XDocument.Load(absFilePath);
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static XDocument ReadDirectoryFiles(string absDirPath, string absOrRelativeConfigPath)
        {
            var config = string.IsNullOrWhiteSpace(absOrRelativeConfigPath)
                ? GetDefaultXmlReaderSettings(absDirPath)
                : GetXmlReaderSettings(absDirPath, absOrRelativeConfigPath);

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
