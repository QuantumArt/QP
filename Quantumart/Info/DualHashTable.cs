using System.Collections;

namespace Quantumart.QPublishing.Info
{
    public class DualHashTable
    {
        public DualHashTable()
        {
            Ids = new Hashtable();
            Items = new Hashtable();
        }

        public Hashtable Ids { get; private set; }

        public Hashtable Items { get; private set; }
    }
}
