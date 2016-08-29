using System;
using Quantumart.QP8.BLL.Factories;
using Quantumart.QP8.BLL.Interfaces.Logging;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Enums;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Extensions;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Adapters
{
    internal class QpUpdateLoggingWrapper//TODO: : ILog
    {
        private readonly ILog _logger;

        private ConsoleLogLevel _consoleLogLevel;

        public QpUpdateLoggingWrapper(ConsoleLogLevel consoleLogLevel = ConsoleLogLevel.Fatal)
        {
            LogManager.LogFactory = new NLogFactory();
            _logger = LogManager.GetLogger("QP8Update");
            _consoleLogLevel = consoleLogLevel;
        }

        public bool IsDebugEnabled => _logger.IsDebugEnabled;

        public bool IsInfoEnabled => _logger.IsInfoEnabled;

        public bool IsWarnEnabled => _logger.IsWarnEnabled;

        public bool IsErrorEnabled => _logger.IsErrorEnabled;

        public bool IsFatalEnabled => _logger.IsFatalEnabled;

        public void Debug(object message)
        {
            _logger.Debug(message);
        }

        public void Debug(object message, Exception exception)
        {
            _logger.Debug(message, exception);
        }

        public void DebugFormat(string format, params object[] args)
        {
            _logger.DebugFormat(format, args);
        }

        public void Error(object message)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Error))
            {
                Console.WriteLine($"Error: {message}");
            }

            _logger.Error(message);
        }

        public void Error(object message, Exception exception)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Error))
            {
                Console.WriteLine($"Error: {message}");
                Console.WriteLine($"Exceptions: {exception.Dump()}");
            }

            _logger.Error(message, exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Error))
            {
                Console.WriteLine($"Error: {format}", args);
            }

            _logger.ErrorFormat(format, args);
        }

        public void Fatal(object message)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Fatal))
            {
                Console.WriteLine($"Fatal: {message}");
            }

            _logger.Fatal(message);
        }

        public void Fatal(object message, Exception exception)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Fatal))
            {
                Console.WriteLine($"Fatal: {message}");
                Console.WriteLine($"Exceptions: {exception.Dump()}");
            }

            _logger.Fatal(message, exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Fatal))
            {
                Console.WriteLine($"Fatal: {format}", args);
            }

            _logger.FatalFormat(format, args);
        }

        public void Info(object message)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Info))
            {
                Console.WriteLine($"Info: {message}");
            }

            _logger.Info(message);
        }

        public void Info(object message, Exception exception)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Info))
            {
                Console.WriteLine($"Info: {message}");
                Console.WriteLine($"Exceptions: {exception.Dump()}");
            }

            _logger.Info(message, exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Info))
            {
                Console.WriteLine($"Info: {format}", args);
            }

            _logger.InfoFormat(format, args);
        }

        public void Warn(object message)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Warn))
            {
                Console.WriteLine($"Warning: {message}");
            }

            _logger.Warn(message);
        }

        public void Warn(object message, Exception exception)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Warn))
            {
                Console.WriteLine($"Warning: {message}");
                Console.WriteLine($"Exceptions: {exception.Dump()}");
            }

            _logger.Warn(message, exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Warn))
            {
                Console.WriteLine($"Warning: {format}", args);
            }

            _logger.WarnFormat(format, args);
        }

        public void Flush()
        {
            _logger.Flush();
        }

        public void SetLogLevel(int verboseLevel)
        {
            switch (verboseLevel)
            {
                case 3:
                    _consoleLogLevel = ConsoleLogLevel.Fatal | ConsoleLogLevel.Error | ConsoleLogLevel.Warn | ConsoleLogLevel.Info;
                    break;
                case 2:
                    _consoleLogLevel = ConsoleLogLevel.Fatal | ConsoleLogLevel.Error | ConsoleLogLevel.Warn;
                    break;
                case 1:
                    _consoleLogLevel = ConsoleLogLevel.Fatal | ConsoleLogLevel.Error;
                    break;
                case 0:
                    _consoleLogLevel = ConsoleLogLevel.Fatal;
                    break;
            }
        }
    }
}
