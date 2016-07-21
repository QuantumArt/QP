using System;
using qp8dbupdate.Infrastructure.Versioning;

namespace qp8dbupdate.Infrastructure.Exceptions
{
    public class DbUpdateException : Exception
    {
        public DbUpdateException(string message, DbUpdateLogEntry entry)
            : base(message)
        {
            Entry = entry;
        }

        public DbUpdateLogEntry Entry { get; set; }
    }
}
