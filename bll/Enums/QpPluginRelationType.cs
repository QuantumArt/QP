using System.Runtime.Serialization;

namespace Quantumart.QP8.BLL.Enums
{
    public enum QpPluginRelationType
    {
        Site,
        Content,
        [EnumMember(Value = "CONTENT_ATTRIBUTE")]
        ContentAttribute
    }
}
