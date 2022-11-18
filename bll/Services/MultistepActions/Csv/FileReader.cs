using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Services.MultistepActions.Import;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Csv
{
    public class FileReader
    {
        private readonly ImportSettings _settings;
        private readonly Lazy<IEnumerable<Line>> _lines;

        public FileReader(ImportSettings settings)
        {
            _settings = settings;
            _lines = new Lazy<IEnumerable<Line>>(() => ReadFile(_settings));
        }

        public IEnumerable<Line> Lines => _lines.Value;

        public static IEnumerable<Line> ReadFile(ImportSettings setts)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using StreamReader sr = new(setts.TempFileUploadPath);
            string line;
            CustomStreamReader rdr = new(sr.BaseStream, Encoding.GetEncoding(setts.Encoding), setts.LineSeparator, setts.Delimiter);
            int i = 0;
            int headerNum = 1;
            while (!string.IsNullOrEmpty(line = rdr.ReadLine()))
            {
                i++;
                string value = line.Trim('\r', '\n');
                bool isSep = value.StartsWith("sep=");
                if (isSep)
                {
                    headerNum++;
                }

                bool skip = (!setts.NoHeaders && i == headerNum) || string.IsNullOrEmpty(value) || isSep;
                yield return new Line { Value = value, Number = i, Skip = skip };
            }
        }

        public int RowsCount()
        {
            return Lines.Count(s => !s.Skip);
        }
    }
}
