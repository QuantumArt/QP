using QP8.Plugins.Contract;

namespace Quantumart.QP8.BLL
{
    public class QpPluginFieldValue
    {
        public QpPluginField Field { get; set; }
        public string Value { get; set; }
        public QpPlugin Plugin { get; set; }

        public string FormName => "plugin_field_" + Field.Id;
    }
}

