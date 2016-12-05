using System;
using NLog;
using Quantumart.QP8.BLL.Interfaces.Logging;
using LogManager = NLog.LogManager;

namespace Quantumart.QP8.BLL.Adapters.Logging
{
    /// <summary>
    /// Wrapper over the NLog 2.0 beta and above logger
    /// </summary>
    public class NLogLogger : ILog
    {
        private readonly Logger _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogLogger"/> class.
        /// </summary>
        /// <param name="typeName">The type name.</param>
        public NLogLogger(string typeName)
        {
            _log = LogManager.GetLogger(typeName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogLogger"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public NLogLogger(Type type)
        {
            _log = LogManager.GetLogger(UseFullTypeNames ? type.FullName : type.Name);
        }

        public static bool UseFullTypeNames { get; set; }

        public bool IsDebugEnabled => _log.IsDebugEnabled;

        public bool IsInfoEnabled => _log.IsInfoEnabled;

        public bool IsWarnEnabled => _log.IsWarnEnabled;

        public bool IsErrorEnabled => _log.IsErrorEnabled;

        public bool IsFatalEnabled => _log.IsFatalEnabled;

        public void Debug(object message)
        {
            if (IsDebugEnabled)
            {
                Log(LogLevel.Debug, message?.ToString());
            }
        }

        public void Debug(object message, Exception exception)
        {
            if (IsDebugEnabled)
            {
                Log(LogLevel.Debug, message?.ToString(), exception);
            }
        }

        public void DebugFormat(string format, params object[] args)
        {
            if (IsDebugEnabled)
            {
                Log(LogLevel.Debug, format, args);
            }
        }

        public void Info(object message)
        {
            if (IsInfoEnabled)
            {
                Log(LogLevel.Info, message?.ToString());
            }
        }

        public void Info(object message, Exception exception)
        {
            if (IsInfoEnabled)
            {
                Log(LogLevel.Info, message?.ToString(), exception);
            }
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (IsInfoEnabled)
            {
                Log(LogLevel.Info, format, args);
            }
        }

        public void Warn(object message)
        {
            if (IsWarnEnabled)
            {
                Log(LogLevel.Warn, message?.ToString());
            }
        }

        public void Warn(object message, Exception exception)
        {
            if (IsWarnEnabled)
            {
                Log(LogLevel.Warn, message?.ToString(), exception);
            }
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (IsWarnEnabled)
            {
                Log(LogLevel.Warn, format, args);
            }
        }

        public void Error(object message)
        {
            if (IsErrorEnabled)
            {
                Log(LogLevel.Error, message?.ToString());
            }
        }

        public void Error(object message, Exception exception)
        {
            if (IsErrorEnabled)
            {
                Log(LogLevel.Error, message?.ToString(), exception);
            }
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (IsErrorEnabled)
            {
                Log(LogLevel.Error, format, args);
            }
        }

        public void Fatal(object message)
        {
            if (IsFatalEnabled)
            {
                Log(LogLevel.Fatal, message?.ToString());
            }
        }

        public void Fatal(object message, Exception exception)
        {
            if (IsFatalEnabled)
            {
                Log(LogLevel.Fatal, message?.ToString(), exception);
            }
        }

        public void FatalFormat(string format, params object[] args)
        {
            if (IsFatalEnabled)
            {
                Log(LogLevel.Fatal, format, args);
            }
        }

        public void Log(LogLevel logLevel, string message, Exception ex)
        {
            _log.Log(typeof(NLogLogger), new LogEventInfo(logLevel, _log.Name, null, message, null, ex));
        }

        public void Log(LogLevel logLevel, string format, params object[] args)
        {
            _log.Log(typeof(NLogLogger), new LogEventInfo(logLevel, _log.Name, null, format, args));
        }

        public void Log(LogLevel logLevel, string format, object[] args, Exception ex)
        {
            _log.Log(typeof(NLogLogger), new LogEventInfo(logLevel, _log.Name, null, format, args, ex));
        }

        public void Flush()
        {
            LogManager.Flush();
        }

        public void Shutdown()
        {
            LogManager.Shutdown();
        }

        public void Dispose()
        {
            Flush();
            Shutdown();
        }
    }
}
