using System.ComponentModel;

namespace Quantumart.QP8.BLL.Enums.Csv
{
    public enum CsvLineSeparator
    {
        [Description("\r\n")]
        Windows,

        [Description("\r")]
        MacOs,

        [Description("\n")]
        Unix
    }
}
