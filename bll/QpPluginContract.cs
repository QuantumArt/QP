using System.Collections.Generic;
using Newtonsoft.Json;

namespace Quantumart.QP8.BLL
{
    public class QpPluginContract
    {
        public string Code { get; set; }

        public string Description { get; set; }

        public string Version { get; set; }

        public string InstanceKey { get; set; }

        public List<QpPluginField> Fields { get; set; }
    }
}
