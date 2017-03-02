using System;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.Infrastructure.Adapters
{
    public class ThreadStorageScopeContext : IDisposable
    {
        public ThreadStorageScopeContext()
        {
            QPContext.UseThreadStorage = true;
        }

        public void Dispose()
        {
            QPContext.UseThreadStorage = false;
        }
    }
}
