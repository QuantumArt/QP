using System.Collections.Generic;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models
{
    internal class ApplicationSettings
    {
        internal string CustomerCode { get; set; }

        internal DatabaseType DbType { get; set; }

        internal IList<string> FilePathes { get; set; } = new List<string>();

        internal string ConnectionString { get; set; }

        internal string SaveFilePath { get; set; }
    }
}
