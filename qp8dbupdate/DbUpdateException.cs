using System;
using qp8dbupdate.Versioning;

namespace qp8dbupdate
{
    public class DbUpdateException : Exception
    {
        public DbUpdateException(string message, DBUpdateLogEntry entry)
            : base(message)
        {
            Entry = entry;
        }

        public DBUpdateLogEntry Entry { get; set; }
    }
}
