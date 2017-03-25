using System;
using QP8.Infrastructure.Extensions;
using QP8.Infrastructure.Logging.Factories;
using QP8.Infrastructure.Logging.Interfaces;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Enums;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Adapters
{
    internal class QpUpdateLoggingWrapper : IDisposable
    {
        private readonly ILog _logger;

        private ConsoleLogLevel _consoleLogLevel;

        public QpUpdateLoggingWrapper(ConsoleLogLevel consoleLogLevel = ConsoleLogLevel.Fatal)
        {
            LogProvider.LogFactory = new NLogFactory();
            _logger = LogProvider.GetLogger();
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

        public void Info(object message)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Info))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(message);
                Console.ResetColor();
            }

            _logger.Info(message);
        }

        public void Info(object message, Exception exception)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Info))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(message);
                Console.WriteLine($"Exceptions:{Environment.NewLine}{exception.Dump()}");
                Console.ResetColor();
            }

            _logger.Info(message, exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Info))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(format, args);
                Console.ResetColor();
            }

            _logger.InfoFormat(format, args);
        }

        public void Warn(object message)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Warn))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(message);
                Console.ResetColor();
            }

            _logger.Warn(message);
        }

        public void Warn(object message, Exception exception)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Warn))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(message);
                Console.WriteLine($"Exceptions:{Environment.NewLine}{exception.Dump()}");
                Console.ResetColor();
            }

            _logger.Warn(message, exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Warn))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(format, args);
                Console.ResetColor();
            }

            _logger.WarnFormat(format, args);
        }

        public void Error(object message)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Error))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.ResetColor();
            }

            _logger.Error(message);
        }

        public void Error(object message, Exception exception)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Error))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.WriteLine($"Exceptions:{Environment.NewLine}{exception.Dump()}");
                Console.ResetColor();
            }

            _logger.Error(message, exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Error))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(format, args);
                Console.ResetColor();
            }

            _logger.ErrorFormat(format, args);
        }

        public void Fatal(object message)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Fatal))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(message);
                Console.ResetColor();
            }

            _logger.Fatal(message);
        }

        public void Fatal(object message, Exception exception)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Fatal))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(message);
                Console.WriteLine($"Exceptions:{Environment.NewLine}{exception.Dump()}");
                Console.ResetColor();
            }

            _logger.Fatal(message, exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Fatal))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(format, args);
                Console.ResetColor();
            }

            _logger.FatalFormat(format, args);
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

        public void Flush()
        {
            _logger.Flush();
        }

        public void Shutdown()
        {
            _logger.Shutdown();
        }

        public void Dispose()
        {
            Flush();
            Shutdown();
        }
    }
}
