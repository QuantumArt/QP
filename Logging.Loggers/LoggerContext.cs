using Quantumart.QP8.BLL;
using Quantumart.QP8.Security;

namespace Quantumart.QP8.Logging.Loggers
{
    public class LoggerContext
    {
        public QPIdentity UserIdentity { get; private set; }

        public string CustomerCode { get; private set; }

        public LoggerContext()
        {
            UserIdentity = QPContext.CurrentUserIdentity;
            CustomerCode = QPContext.CurrentCustomerCode;
        }
    }
}
