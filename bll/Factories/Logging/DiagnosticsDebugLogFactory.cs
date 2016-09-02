using System;
using Quantumart.QP8.BLL.Adapters.Logging;
using Quantumart.QP8.BLL.Interfaces.Logging;

namespace Quantumart.QP8.BLL.Factories.Logging
{
    /// <summary>
    /// Creates a Debug Logger, that logs all messages to: System.Diagnostics.Debug
    /// Made public so its testable
    /// </summary>
	public class DiagnosticsDebugLogFactory : ILogFactory
    {
        private readonly bool _debugEnabled;

        public DiagnosticsDebugLogFactory(bool debugEnabled = false)
        {
            _debugEnabled = debugEnabled;
        }

        public ILog GetLogger()
        {
            return new DiagnosticsDebugLogger { IsDebugEnabled = _debugEnabled };
        }

        public ILog GetLogger(Type type)
        {
            return new DiagnosticsDebugLogger { IsDebugEnabled = _debugEnabled };
        }

        public ILog GetLogger(string typeName)
        {
            return new DiagnosticsDebugLogger { IsDebugEnabled = _debugEnabled };
        }
    }
}
