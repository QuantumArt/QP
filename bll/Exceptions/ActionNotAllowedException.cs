using System;

namespace Quantumart.QP8.BLL.Exceptions
{
    public class ActionNotAllowedException : Exception
    {
        public ActionNotAllowedException()
        {
        }

        public ActionNotAllowedException(string message)
            : base(message)
        {
        }
    }
}
