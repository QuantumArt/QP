using System;

namespace QP8.Infrastructure.Logging.Interfaces
{
    /// <summary>
    /// Logs a message in a running application
    /// </summary>
    public interface ILog : IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is trace enabled
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is trace enabled; otherwise, <c>false</c>
        /// </value>
        bool IsTraceEnabled { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is debug enabled
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is debug enabled; otherwise, <c>false</c>
        /// </value>
        bool IsDebugEnabled { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is info enabled
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is info enabled; otherwise, <c>false</c>
        /// </value>
        bool IsInfoEnabled { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is warn enabled
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is warn enabled; otherwise, <c>false</c>
        /// </value>
        bool IsWarnEnabled { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is error enabled
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is error enabled; otherwise, <c>false</c>
        /// </value>
        bool IsErrorEnabled { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is fatal enabled
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is fatal enabled; otherwise, <c>false</c>
        /// </value>
        bool IsFatalEnabled { get; }

        /// <summary>
        /// Logs a Trace message
        /// </summary>
        /// <param name="message">The message</param>
        void Trace(object message);

        /// <summary>
        /// Logs a Trace message and exception
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="exception">The exception</param>
        void Trace(object message, Exception exception);

        /// <summary>
        /// Logs a Trace format message.
        /// </summary>
        /// <param name="format">The format</param>
        /// <param name="args">The args</param>
        void TraceFormat(string format, params object[] args);

        /// <summary>
        /// Logs a Debug message
        /// </summary>
        /// <param name="message">The message</param>
        void Debug(object message);

        /// <summary>
        /// Logs a Debug message and exception
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="exception">The exception</param>
        void Debug(object message, Exception exception);

        /// <summary>
        /// Logs a Debug format message.
        /// </summary>
        /// <param name="format">The format</param>
        /// <param name="args">The args</param>
        void DebugFormat(string format, params object[] args);

        /// <summary>
        /// Logs an Info message and exception
        /// </summary>
        /// <param name="message">The message</param>
        void Info(object message);

        /// <summary>
        /// Logs an Info message and exception
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="exception">The exception</param>
        void Info(object message, Exception exception);

        /// <summary>
        /// Logs an Info format message
        /// </summary>
        /// <param name="format">The format</param>
        /// <param name="args">The args</param>
        void InfoFormat(string format, params object[] args);

        /// <summary>
        /// Logs a Warning message
        /// </summary>
        /// <param name="message">The message</param>
        void Warn(object message);

        /// <summary>
        /// Logs a Warning message and exception
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="exception">The exception</param>
        void Warn(object message, Exception exception);

        /// <summary>
        /// Logs a Warning format message
        /// </summary>
        /// <param name="format">The format</param>
        /// <param name="args">The args</param>
        void WarnFormat(string format, params object[] args);

        /// <summary>
        /// Logs a Error message
        /// </summary>
        /// <param name="message">The message</param>
        void Error(object message);

        /// <summary>
        /// Logs a Error message and exception
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="exception">The exception</param>
        void Error(object message, Exception exception);

        /// <summary>
        /// Logs a Error format message
        /// </summary>
        /// <param name="format">The format</param>
        /// <param name="args">The args</param>
        void ErrorFormat(string format, params object[] args);

        /// <summary>
        /// Logs a Fatal message
        /// </summary>
        /// <param name="message">The message</param>
        void Fatal(object message);

        /// <summary>
        /// Logs a Fatal message and exception
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="exception">The exception</param>
        void Fatal(object message, Exception exception);

        /// <summary>
        /// Logs a Error format message
        /// </summary>
        /// <param name="format">The format</param>
        /// <param name="args">The args</param>
        void FatalFormat(string format, params object[] args);

        /// <summary>
        /// Set param for current thread context
        /// </summary>
        /// <param name="item">Param name</param>
        /// <param name="value">Param Value</param>
        void SetContext(string item, string value);

        /// <summary>
        /// Set param for current thread context
        /// </summary>
        /// <param name="item">Param name</param>
        /// <param name="value">Param Value</param>
        void SetContext(string item, object value);

        /// <summary>
        /// Set param for current logical context
        /// </summary>
        /// <param name="item">Param name</param>
        /// <param name="value">Param Value</param>
        void SetAsyncContext(string item, string value);

        /// <summary>
        /// Set param for current logical context
        /// </summary>
        /// <param name="item">Param name</param>
        /// <param name="value">Param Value</param>
        void SetAsyncContext(string item, object value);

        /// <summary>
        /// Set param for global context
        /// </summary>
        /// <param name="item">Param name</param>
        /// <param name="value">Param Value</param>
        void SetGlobalContext(string item, string value);

        /// <summary>
        /// Set param for global context
        /// </summary>
        /// <param name="item">Param name</param>
        /// <param name="value">Param Value</param>
        void SetGlobalContext(string item, object value);

        /// <summary>
        /// Flush log data
        /// </summary>
        void Flush();

        /// <summary>
        /// Close logger and dispose all targets
        /// </summary>
        void Shutdown();
    }
}
