using Quantumart.QP8.BLL.Enums;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL
{
    public class QpPluginField
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public QpPluginValueType ValueType { get; set; }
        public QpPluginRelationType RelationType { get; set; }
        public int SortOrder { get; set; }
    }
}
