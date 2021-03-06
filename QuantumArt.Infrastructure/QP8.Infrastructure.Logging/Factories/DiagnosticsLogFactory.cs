﻿using System;
using QP8.Infrastructure.Helpers;
using QP8.Infrastructure.Logging.Adapters;
using QP8.Infrastructure.Logging.Interfaces;

namespace QP8.Infrastructure.Logging.Factories
{
    /// <summary>
    /// Factory for creating a System.Diagnostics logger
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

        public ILog GetLogger() => GetLogger(AssemblyHelpers.GetAssemblyName());

        public ILog GetLogger(Type type) => type == null ? GetLogger() : GetLogger(type.Name);

        public ILog GetLogger(string loggerName) => string.IsNullOrWhiteSpace(loggerName)
            ? GetLogger()
            : new DiagnosticsDebugLogger
            {
                LoggerName = loggerName,
                IsTraceEnabled = _traceEnabled,
                IsDebugEnabled = _debugEnabled,
                IsInfoEnabled = _infoEnabled,
                IsWarnEnabled = _warnEnabled,
                IsErrorEnabled = _errorEnabled,
                IsFatalEnabled = _fatalEnabled
            };
    }
}
