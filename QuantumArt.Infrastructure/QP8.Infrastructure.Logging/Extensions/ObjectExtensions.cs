using QP8.Infrastructure.Extensions;
using QP8.Infrastructure.Logging.Interfaces;

namespace QP8.Infrastructure.Logging.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Shortcut for logger command TraceFormat(message, args), where args is current object
        /// </summary>
        /// <typeparam name="T">Current object type</typeparam>
        /// <param name="obj">Current object</param>
        /// <param name="message">Message with {0} placeholder for current object</param>
        /// <param name="logger">ILog instance to use, or default Logger.Log, if logger - null</param>
        /// <returns>Current object</returns>
        public static T LogTraceFormat<T>(this T obj, string message, ILog logger = null)
        {
            (logger ?? Logger.Log).TraceFormat(message, obj.ToJsonLog());
            return obj;
        }

        /// <summary>
        /// Shortcut for logger command DebugFormat(message, args), where args is current object
        /// </summary>
        /// <typeparam name="T">Current object type</typeparam>
        /// <param name="obj">Current object</param>
        /// <param name="message">Message with {0} placeholder for current object</param>
        /// <param name="logger">ILog instance to use, or default Logger.Log, if logger - null</param>
        /// <returns>Current object</returns>
        public static T LogDebugFormat<T>(this T obj, string message, ILog logger = null)
        {
            (logger ?? Logger.Log).DebugFormat(message, obj.ToJsonLog());
            return obj;
        }

        /// <summary>
        /// Shortcut for logger command InfoFormat(message, args), where args is current object
        /// </summary>
        /// <typeparam name="T">Current object type</typeparam>
        /// <param name="obj">Current object</param>
        /// <param name="message">Message with {0} placeholder for current object</param>
        /// <param name="logger">ILog instance to use, or default Logger.Log, if logger - null</param>
        /// <returns>Current object</returns>
        public static T LogInfoFormat<T>(this T obj, string message, ILog logger = null)
        {
            (logger ?? Logger.Log).InfoFormat(message, obj.ToJsonLog());
            return obj;
        }

        /// <summary>
        /// Shortcut for logger command WarnFormat(message, args), where args is current object
        /// </summary>
        /// <typeparam name="T">Current object type</typeparam>
        /// <param name="obj">Current object</param>
        /// <param name="message">Message with {0} placeholder for current object</param>
        /// <param name="logger">ILog instance to use, or default Logger.Log, if logger - null</param>
        /// <returns>Current object</returns>
        public static T LogWarnFormat<T>(this T obj, string message, ILog logger = null)
        {
            (logger ?? Logger.Log).WarnFormat(message, obj.ToJsonLog());
            return obj;
        }

        /// <summary>
        /// Shortcut for logger command ErrorFormat(message, args), where args is current object
        /// </summary>
        /// <typeparam name="T">Current object type</typeparam>
        /// <param name="obj">Current object</param>
        /// <param name="message">Message with {0} placeholder for current object</param>
        /// <param name="logger">ILog instance to use, or default Logger.Log, if logger - null</param>
        /// <returns>Current object</returns>
        public static T LogErrorFormat<T>(this T obj, string message, ILog logger = null)
        {
            (logger ?? Logger.Log).ErrorFormat(message, obj.ToJsonLog());
            return obj;
        }

        /// <summary>
        /// Shortcut for logger command FatalFormat(message, args), where args is current object
        /// </summary>
        /// <typeparam name="T">Current object type</typeparam>
        /// <param name="obj">Current object</param>
        /// <param name="message">Message with {0} placeholder for current object</param>
        /// <param name="logger">ILog instance to use, or default Logger.Log, if logger - null</param>
        /// <returns>Current object</returns>
        public static T LogFatalFormat<T>(this T obj, string message, ILog logger = null)
        {
            (logger ?? Logger.Log).FatalFormat(message, obj.ToJsonLog());
            return obj;
        }
    }
}
