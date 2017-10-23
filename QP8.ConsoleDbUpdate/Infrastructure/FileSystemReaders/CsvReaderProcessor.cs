using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using QP8.Infrastructure;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.BLL.Models.CsvDbUpdate;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.FileSystemReaders
{
    public class CsvReaderProcessor
    {
        internal static IEnumerable<CsvDbUpdateModel> Process(IList<string> filePathes, CsvConfiguration csvConfiguration)
        {
            Ensure.Argument.NotNull(csvConfiguration, nameof(csvConfiguration));

            Program.Logger.Debug($"Begin parsing documents: {filePathes.ToJsonLog()}");
            var orderedFilePathes = new List<string>();
            foreach (var path in filePathes)
            {
                if (!File.Exists(path) && !Directory.Exists(path))
                {
                    throw new FileNotFoundException("Неправильно указан путь к файлам записанных действий: " + path);
                }

                if ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    orderedFilePathes.AddRange(GetOrderedDirectoryFilePathes(path));
                }
                else
                {
                    orderedFilePathes.Add(path);
                }
            }

            Program.Logger.Info($"Total files will be processed: {orderedFilePathes.Count}.");
            Program.Logger.Debug($"Documents will be processed in next order: {orderedFilePathes.ToJsonLog()}");
            return ParseDocuments(orderedFilePathes, csvConfiguration);
        }

        private static IEnumerable<CsvDbUpdateModel> ParseDocuments(IEnumerable<string> filePathes, CsvConfiguration csvConfiguration)
        {
            foreach (var path in filePathes)
            {
                using (var stream = new StreamReader(path, csvConfiguration.Encoding, true))
                using (var csv = new CsvReader(stream, csvConfiguration))
                {
                    var csvFieldsData = new Dictionary<int, IList<CsvDbUpdateFieldModel>>();
                    while (csv.Read())
                    {
                        csvFieldsData.Add(csv.Row, csv.FieldHeaders.Zip(csv.CurrentRecord, (fieldName, fieldValue) => new CsvDbUpdateFieldModel
                        {
                            Name = fieldName,
                            Value = fieldValue == "NULL" ? null : fieldValue
                        }).ToList());
                    }

                    yield return new CsvDbUpdateModel
                    {
                        ContentId = GetContentIdFromFileName(path),
                        Fields = csvFieldsData
                    };
                }
            }
        }

        private static IEnumerable<string> GetOrderedDirectoryFilePathes(string absDirPath)
        {
            return new CsvReaderSettings(Directory.EnumerateFiles(absDirPath, "*.csv", SearchOption.TopDirectoryOnly).OrderBy(fn => fn).ToList()).FilePathes;
        }

        private static int GetContentIdFromFileName(string path) => Convert.ToInt32(Path.GetFileName(path)?.Split('_').Skip(1).First());
    }
}
