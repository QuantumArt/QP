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
        private readonly bool _traceEnabled;
        private readonly bool _debugEnabled;
        private readonly bool _infoEnabled;
        private readonly bool _warnEnabled;
        private readonly bool _errorEnabled;
        private readonly bool _fatalEnabled;

        public DiagnosticsLogFactory(
            bool traceEnabled = true,
            bool debugEnabled = true,
            bool infoEnabled = true,
            bool warnEnabled = true,
            bool errorEnabled = true,
            bool fatalEnabled = true)
        {
            _traceEnabled = traceEnabled;
            _debugEnabled = debugEnabled;
            _infoEnabled = infoEnabled;
            _warnEnabled = warnEnabled;
            _errorEnabled = errorEnabled;
            _fatalEnabled = fatalEnabled;
        }

        public ILog GetLogger()
        {
            return new DiagnosticsDebugLogger
            {
                IsTraceEnabled = _traceEnabled,
                IsDebugEnabled = _debugEnabled,
                IsInfoEnabled = _infoEnabled,
                IsWarnEnabled = _warnEnabled,
                IsErrorEnabled = _errorEnabled,
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
