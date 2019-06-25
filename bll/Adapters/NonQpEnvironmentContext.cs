using System;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.BLL.Adapters
{
    public class NonQpEnvironmentContext : IDisposable
    {
        public NonQpEnvironmentContext(QpConnectionInfo info)
        {
            QPConfiguration.TempDirectory = @"c:\temp\";
            QPContext.CurrentDbConnectionInfo = info;
        }

        public void Dispose()
        {
            QPConfiguration.TempDirectory = null;
            QPContext.CurrentDbConnectionInfo = null;
        }
    }
}
