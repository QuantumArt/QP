using System.ComponentModel;

namespace Quantumart.QP8.BLL.Enums.Csv
{
    public enum CsvEncoding
    {
        [Description("Windows-1251")]
        Windows1251,

        [Description("UTF-8")]
        Utf8,

        [Description("UTF-16")]
        Utf16,

        [Description("KOI8-R")]
        Koi8R,

        [Description("cp866")]
        Cp866
    }
}
