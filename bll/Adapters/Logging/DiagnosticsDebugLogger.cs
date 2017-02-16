using System;
using System.Linq;
using Quantumart.QP8.BLL.Interfaces.Logging;

namespace Quantumart.QP8.BLL.Adapters.Logging
{
    public class DiagnosticsDebugLogger : ILog
    {
        public bool IsTraceEnabled { get; set; }

        public bool IsDebugEnabled { get; set; }

        public bool IsInfoEnabled { get; set; }

        public bool IsWarnEnabled { get; set; }

        public bool IsErrorEnabled { get; set; }

        public bool IsFatalEnabled { get; set; }

        public void Trace(object message)
        {
            if (IsTraceEnabled)
            {
                Log("Trace", message?.ToString());
            }
        }

        public void Trace(object message, Exception exception)
        {
            if (IsTraceEnabled)
            {
                Log("Trace", message?.ToString(), exception);
            }
        }

        public void TraceFormat(string format, params object[] args)
        {
            if (IsTraceEnabled)
            {
                Log("Trace", format, args);
            }
        }

        public void Debug(object message)
        {
            if (IsDebugEnabled)
            {
                Log("Debug", message?.ToString());
            }
        }

        public void Debug(object message, Exception exception)
        {
            if (IsDebugEnabled)
            {
                Log("Debug", message?.ToString(), exception);
            }
        }

        public void DebugFormat(string format, params object[] args)
        {
            if (IsDebugEnabled)
            {
                Log("Debug", format, args);
            }
        }

        public void Info(object message)
        {
            if (IsInfoEnabled)
            {
                Log("Info", message?.ToString());
            }
        }

        public void Info(object message, Exception exception)
        {
            if (IsInfoEnabled)
            {
                Log("Info", message?.ToString(), exception);
            }
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (IsInfoEnabled)
            {
                Log("Info", format, args);
            }
        }

        public void Warn(object message)
        {
            if (IsWarnEnabled)
            {
                Log("Warn", message?.ToString());
            }
        }

        public void Warn(object message, Exception exception)
        {
            if (IsWarnEnabled)
            {
                Log("Warn", message?.ToString(), exception);
            }
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (IsWarnEnabled)
            {
                Log("Warn", format, args);
            }
        }

        public void Error(object message)
        {
            if (IsErrorEnabled)
            {
                Log("Error", message?.ToString());
            }
        }

        public void Error(object message, Exception exception)
        {
            if (IsErrorEnabled)
            {
                Log("Error", message?.ToString(), exception);
            }
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (IsErrorEnabled)
            {
                Log("Error", format, args);
            }
        }

        public void Fatal(object message)
        {
            if (IsFatalEnabled)
            {
                Log("Fatal", message?.ToString());
            }
        }

        public void Fatal(object message, Exception exception)
        {
            if (IsFatalEnabled)
            {
                Log("Fatal", message?.ToString(), exception);
            }
        }

        public void FatalFormat(string format, params object[] args)
        {
            if (IsFatalEnabled)
            {
                Log("Fatal", format, args);
            }
        }

        public void Log(string logLevel, string message, Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(message, logLevel);
        }

        public void Log(string logLevel, string format, params object[] args)
        {
            string message;
            if (args == null || !args.Any())
            {
                message = format;
            }
            else
            {
                message = string.Format(format, args);
            }

            System.Diagnostics.Debug.WriteLine(message, logLevel);
        }

        public void Log(string logLevel, string format, object[] args, Exception ex)
        {
            string message;
            if (args == null || !args.Any())
            {
                message = format;
            }
            else
            {
                message = string.Format(format, args);
            }

            System.Diagnostics.Debug.WriteLine($"{message}. Exception: {ex.Message}", logLevel);
        }

        public void SetContext(string item, string value)
        {
            System.Diagnostics.Debug.WriteLine($"Set param for thread context [item: {item}, value: {value}]");
        }

        public void SetContext(string item, object value)
        {
            SetGlobalContext(item, value?.ToString());
        }

        public void SetAsyncContext(string item, string value)
        {
            System.Diagnostics.Debug.WriteLine($"Set param for async context [item: {item}, value: {value}]");
        }

        public void SetAsyncContext(string item, object value)
        {
            SetGlobalContext(item, value?.ToString());
        }

        public void SetGlobalContext(string item, string value)
        {
            System.Diagnostics.Debug.WriteLine($"Set param for global context [item: {item}, value: {value}]");
        }

        public void SetGlobalContext(string item, object value)
        {
            SetGlobalContext(item, value?.ToString());
        }

        public void Flush()
        {
            System.Diagnostics.Debug.Flush();
        }

        public void Shutdown()
        {
            System.Diagnostics.Debug.Close();
        }

        public void Dispose()
        {
            Flush();
            Shutdown();
        }
    }
}
