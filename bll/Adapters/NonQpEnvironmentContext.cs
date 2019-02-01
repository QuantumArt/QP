using System;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.BLL.Adapters
{
    public class NonQpEnvironmentContext : IDisposable
    {
        public NonQpEnvironmentContext(string connectionString)
        {
            QPConfiguration.TempDirectory = @"c:\temp\";
            QPContext.CurrentDbConnectionString = connectionString;
        }

        public void Dispose()
        {
            QPConfiguration.TempDirectory = null;
            QPContext.CurrentDbConnectionString = null;
        }
    }
}
