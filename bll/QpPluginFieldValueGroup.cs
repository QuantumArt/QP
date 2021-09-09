using System.Collections.Generic;

namespace Quantumart.QP8.BLL
{
    public class QpPluginFieldValueGroup
    {
        public IEnumerable<QpPluginFieldValue> Fields { get; set; }
        public string Name { get; set; }
    }
}
