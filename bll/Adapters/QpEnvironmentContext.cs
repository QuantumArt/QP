using System;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.BLL.Adapters
{
    public class QpEnvironmentContext : IDisposable
    {
        public QpEnvironmentContext(QpConnectionInfo info)
        {
            QPContext.CurrentDbConnectionInfo = info;
        }

        public void Dispose()
        {
            QPContext.CurrentDbConnectionInfo = null;
        }
    }
}
