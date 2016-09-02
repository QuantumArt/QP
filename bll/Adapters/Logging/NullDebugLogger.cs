using System;
using System.Diagnostics.CodeAnalysis;
using Quantumart.QP8.BLL.Interfaces.Logging;

namespace Quantumart.QP8.BLL.Adapters.Logging
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    public class NullDebugLogger : ILog
    {
        public NullDebugLogger(string type) { }

        public NullDebugLogger(Type type) { }

        private static void Log(object message, Exception exception) { }

        private static void LogFormat(object message, params object[] args) { }

        private static void Log(object message) { }

        public void Debug(object message, Exception exception) { }

        public bool IsDebugEnabled { get; set; }

        public bool IsInfoEnabled { get; set; }

        public bool IsWarnEnabled { get; set; }

        public bool IsErrorEnabled { get; set; }

        public bool IsFatalEnabled { get; set; }

        public void Debug(object message) { }

        public void DebugFormat(string format, params object[] args) { }

        public void Error(object message, Exception exception) { }

        public void Error(object message) { }

        public void ErrorFormat(string format, params object[] args) { }

        public void Fatal(object message, Exception exception) { }

        public void Fatal(object message) { }

        public void FatalFormat(string format, params object[] args) { }

        public void Info(object message, Exception exception) { }

        public void Info(object message) { }

        public void InfoFormat(string format, params object[] args) { }

        public void Warn(object message, Exception exception) { }

        public void Warn(object message) { }

        public void WarnFormat(string format, params object[] args) { }

        public void Flush() { }
    }
}
