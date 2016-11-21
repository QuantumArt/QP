using System;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.BLL.Adapters
{
    public class NonQpEnvironmentContext : IDisposable
    {
        public NonQpEnvironmentContext(string connectionString)
        {
            QPConfiguration._tempDirectory = @"c:\temp\";
            QPContext.CurrentDbConnectionString = connectionString;
        }

        public void Dispose()
        {
            QPConfiguration._tempDirectory = null;
            QPContext.CurrentDbConnectionString = null;
        }
    }
}
