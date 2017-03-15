using System;
using System.Collections.Generic;
using System.Linq;

namespace QP8.Infrastucture.Extensions
{
    public static class ExceptionHelpers
    {
        public static IEnumerable<Exception> GetExceptionsList(this Exception ex)
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

        public static string Dump(this Exception ex)
        {
            return string.Join(Environment.NewLine, ex.GetExceptionsList().Select(x => x.Message));
        }
    }
}
