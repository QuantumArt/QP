using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Enums;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.FileSystemReaders
{
    internal static class XmlReaderProcessor
    {
        internal static string Process(IList<string> filePathes)
        {
            Program.Logger.Info($"Total files readed from disk: {filePathes.Count}.");
            Program.Logger.Debug($"Documents will be processed in next order: {filePathes.ToJsonLog()}.");

            return CombineMultipleDocumentsWithSameRoot(filePathes.Select(XDocument.Load).ToList()).ToNormalizedString(SaveOptions.DisableFormatting);
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

            XDocument root = new(xmlDocuments[0].Root);
            root.Root?.RemoveNodes();

            return xmlDocuments.Aggregate(root, (result, xd) =>
            {
                result.Root?.Add(xd.Root?.Elements());
                return result;
            });
        }
    }
}
