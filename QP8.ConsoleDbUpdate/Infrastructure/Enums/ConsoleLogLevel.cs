using System;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Enums
{
    [Flags]
    internal enum ConsoleLogLevel
    {
        Fatal = 1 << 0,
        Error = 1 << 1,
        Warn = 1 << 2,
        Info = 1 << 3
    }
}
