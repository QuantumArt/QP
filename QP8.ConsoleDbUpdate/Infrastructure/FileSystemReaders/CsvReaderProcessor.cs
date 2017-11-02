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

        internal static IEnumerable<CsvDbUpdateModel> Process(string inputData, CsvConfiguration csvConfiguration)
        {
            using (var sr = new StringReader(inputData))
            using (var csv = new CsvReader(sr, csvConfiguration))
            {
                yield return ParseCsvReader(csv);
            }
        }

        private static IEnumerable<CsvDbUpdateModel> ParseDocuments(IEnumerable<string> filePathes, CsvConfiguration csvConfiguration)
        {
            foreach (var path in filePathes)
            {
                using (var stream = new StreamReader(path, csvConfiguration.Encoding, true))
                using (var csv = new CsvReader(stream, csvConfiguration))
                {
                    yield return ParseCsvReader(csv, path);
                }
            }
        }

        private static CsvDbUpdateModel ParseCsvReader(CsvReader csv, string path = null)
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

            var contentIdValue = csvFieldsData.SelectMany(f => f.Value).FirstOrDefault(f => f.Name.ToUpper() == "CONTENT_ID")?.Value;
            return new CsvDbUpdateModel
            {
                ContentId = string.IsNullOrWhiteSpace(contentIdValue) ? GetContentIdFromFileName(path) : int.Parse(contentIdValue),
                Fields = csvFieldsData
            };
        }

        private static IEnumerable<string> GetOrderedDirectoryFilePathes(string absDirPath)
        {
            return new CsvReaderSettings(Directory.EnumerateFiles(absDirPath, "*.csv", SearchOption.TopDirectoryOnly).OrderBy(fn => fn).ToList()).FilePathes;
        }

        private static int GetContentIdFromFileName(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new CsvBadDataException("Should specify \"content_id\" either at a csv column or at a filename");
            }

            return Convert.ToInt32(Path.GetFileName(path).Split('_').Skip(1).First());
        }
    }
}
