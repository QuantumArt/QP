using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.BLL.Models.CsvDbUpdate;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.FileSystemReaders
{
    public class CsvReaderProcessor
    {
        internal static IEnumerable<CsvDbUpdateModel> Process(IList<string> filePathes, CsvConfiguration csvConfiguration)
        {
            Program.Logger.Info($"Total files will be processed: {filePathes.Count}.");
            Program.Logger.Debug($"Documents will be processed in next order: {filePathes.ToJsonLog()}");
            return ParseDocuments(filePathes, csvConfiguration);
        }

        internal static IEnumerable<CsvDbUpdateModel> Process(string inputData, CsvConfiguration csvConfiguration)
        {
            using StringReader sr = new(inputData);
            using CsvReader csv = new(sr, csvConfiguration);
            yield return ParseCsvReader(csv);
        }

        private static IEnumerable<CsvDbUpdateModel> ParseDocuments(IEnumerable<string> filePathes, CsvConfiguration csvConfiguration)
        {
            foreach (string path in filePathes)
            {
                using StreamReader stream = new(path, csvConfiguration.Encoding, true);
                using CsvReader csv = new(stream, csvConfiguration);
                yield return ParseCsvReader(csv, path);
            }
        }

        private static CsvDbUpdateModel ParseCsvReader(CsvReader csv, string path = null)
        {
            Dictionary<int, IList<CsvDbUpdateFieldModel>> csvFieldsData = new();
            while (csv.Read())
            {
                csvFieldsData.Add(csv.Row, csv.FieldHeaders.Zip(csv.CurrentRecord, (fieldName, fieldValue) => new CsvDbUpdateFieldModel
                {
                    Name = fieldName,
                    Value = fieldValue == "NULL" ? null : fieldValue
                }).ToList());
            }

            string contentIdValue = csvFieldsData.SelectMany(f => f.Value).FirstOrDefault(f => f.Name.ToUpper() == "CONTENT_ID")?.Value;
            return new CsvDbUpdateModel
            {
                ContentId = string.IsNullOrWhiteSpace(contentIdValue) ? GetContentIdFromFileName(path) : int.Parse(contentIdValue),
                Fields = csvFieldsData
            };
        }

        private static int GetContentIdFromFileName(string path)
        {
            return string.IsNullOrWhiteSpace(path)
                ? throw new CsvBadDataException("Should specify \"content_id\" either at a csv column or at a filename")
                : Convert.ToInt32(Path.GetFileName(path).Split('_').Skip(1).First());
        }
    }
}
