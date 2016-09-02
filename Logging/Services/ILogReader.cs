using System.Collections.Generic;

namespace Quantumart.QP8.Logging.Services
{
    public interface ILogReader
    {
        string Read();

        string Read(IEnumerable<string> listeners);
    }
}
