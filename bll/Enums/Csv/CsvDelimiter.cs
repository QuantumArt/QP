using System.ComponentModel;

namespace Quantumart.QP8.BLL.Enums.Csv
{
    public enum CsvDelimiter
    {
        [Description(",")]
        Comma,

        [Description(";")]
        Semicolon,

        [Description("\t")]
        Tab
    }
}
