using System;
using System.Collections.Generic;
using System.Linq;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Extensions
{
    internal static class ExceptionHelpers
    {
        internal static IEnumerable<Exception> GetExceptionsList(this Exception ex)
        {
            while (true)
            {
                yield return ex;
                if (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    continue;
                }

                break;
            }
        }

        internal static string Dump(this Exception ex)
        {
            return string.Join(Environment.NewLine, ex.GetExceptionsList().Select(x => x.Message));
        }
    }
}
