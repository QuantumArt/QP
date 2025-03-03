using System;
using QP8.Infrastructure.Extensions;
using NLog;
using NLog.Fluent;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Enums;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Adapters
{
    internal class QpUpdateLoggingWrapper : IDisposable
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();


        private ConsoleLogLevel _consoleLogLevel;

        public QpUpdateLoggingWrapper(ConsoleLogLevel consoleLogLevel = ConsoleLogLevel.Fatal)
        {
            _consoleLogLevel = consoleLogLevel;
        }

        public bool IsDebugEnabled => Logger.IsDebugEnabled;

        public bool IsInfoEnabled => Logger.IsInfoEnabled;

        public bool IsWarnEnabled => Logger.IsWarnEnabled;

        public bool IsErrorEnabled => Logger.IsErrorEnabled;

        public bool IsFatalEnabled => Logger.IsFatalEnabled;

        public void Debug(object message)
        {
            Logger.Debug(message);
        }

        public void Debug(object message, Exception exception)
        {
            Logger.ForDebugEvent()
                .Exception(exception)
                .Message(message.ToString())
                .Log();
        }

        public void DebugFormat(string format, params object[] args)
        {
            Logger.Debug(format, args);
        }

        public void Info(object message)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Info))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(message);
                Console.ResetColor();
            }

            Logger.Info(message);
        }

        public void Info(object message, Exception exception)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Info))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(message);
                Console.WriteLine($@"Exceptions:{Environment.NewLine}{exception.Dump()}");
                Console.ResetColor();
            }

            Logger.ForInfoEvent()
                .Exception(exception)
                .Message(message.ToString())
                .Log();
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Info))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(format, args);
                Console.ResetColor();
            }

            Logger.Info(format, args);
        }

        public void Warn(object message)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Warn))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Error.WriteLine(message);
                Console.ResetColor();
            }

            Logger.Warn(message);
        }

        public void Warn(object message, Exception exception)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Warn))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Error.WriteLine(message);
                Console.Error.WriteLine($@"Exceptions:{Environment.NewLine}{exception.Dump()}");
                Console.ResetColor();
            }

            Logger.ForWarnEvent()
                .Exception(exception)
                .Message(message.ToString())
                .Log();
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Warn))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Error.WriteLine(format, args);
                Console.ResetColor();
            }

            Logger.Warn(format, args);
        }

        public void Error(object message)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Error))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(message);
                Console.ResetColor();
            }

            Logger.Error(message);
        }

        public void Error(object message, Exception exception)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Error))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(message);
                Console.Error.WriteLine($@"Exceptions:{Environment.NewLine}{exception.Dump()}");
                Console.ResetColor();
            }
            Logger.ForErrorEvent()
                .Exception(exception)
                .Message(message.ToString())
                .Log();
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Error))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(format, args);
                Console.ResetColor();
            }

            Logger.Error(format, args);
        }

        public void Fatal(object message)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Fatal))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Error.WriteLine(message);
                Console.ResetColor();
            }

            Logger.Fatal(message);
        }

        public void Fatal(object message, Exception exception)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Fatal))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Error.WriteLine(message);
                Console.Error.WriteLine($@"Exceptions:{Environment.NewLine}{exception.Dump()}");
                Console.ResetColor();
            }

            Logger.ForFatalEvent()
                .Exception(exception)
                .Message(message.ToString())
                .Log();
        }

        public void FatalFormat(string format, params object[] args)
        {
            if (_consoleLogLevel.HasFlag(ConsoleLogLevel.Fatal))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Error.WriteLine(format, args);
                Console.ResetColor();
            }

            Logger.Fatal(format, args);
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

        public void Dispose()
        {
        }
    }
}
