using System.Collections.Generic;

namespace QP8.Plugins.Contract
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
