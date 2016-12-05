using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.Logging.Services;
using Quantumart.QP8.Logging.Web.Repository;

namespace Quantumart.QP8.Logging.Web.Services
{
    public class LogReader : ILogReader
    {
        public string Read()
        {
            return Read(null);
        }

        public string Read(IEnumerable<string> listeners)
        {
            var text = HttpContextRepository.GetCurrent()
                .Where(itm => listeners == null || listeners.Contains(itm.Listener))
                .Select(itm => itm.Text);

            return string.Join(Environment.NewLine, text);
        }
    }
}
