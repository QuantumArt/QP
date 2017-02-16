using System;
using Quantumart.QP8.BLL.Adapters.Logging;
using Quantumart.QP8.BLL.Interfaces.Logging;

namespace Quantumart.QP8.BLL.Factories.Logging
{
    /// <summary>
    /// Creates a System.Diagnostics logger, that logs all messages to System.Diagnostics.Debug
    /// </summary>
	public class DiagnosticsLogFactory : ILogFactory
    {
        private readonly bool _debugEnabled;
        private readonly bool _infoEnabled;
        private readonly bool _warnEnabled;
        private readonly bool _fatalEnabled;

        public DiagnosticsLogFactory(
            bool debugEnabled = true,
            bool infoEnabled = true,
            bool warnEnabled = true,
            bool fatalEnabled = true)
        {
            _debugEnabled = debugEnabled;
            _infoEnabled = infoEnabled;
            _warnEnabled = warnEnabled;
            _fatalEnabled = fatalEnabled;
        }

        public ILog GetLogger()
        {
            return new DiagnosticsDebugLogger
            {
                IsDebugEnabled = _debugEnabled,
                IsInfoEnabled = _infoEnabled,
                IsWarnEnabled = _warnEnabled,
                IsFatalEnabled = _fatalEnabled,
            };
        }

        public ILog GetLogger(Type type)
        {
            return GetLogger();
        }

        public ILog GetLogger(string typeName)
        {
            return GetLogger();
        }
    }
}
