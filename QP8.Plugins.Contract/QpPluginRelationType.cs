using System.Runtime.Serialization;

namespace QP8.Plugins.Contract
{
    public enum QpPluginRelationType
    {
        Site,
        Content,
        [EnumMember(Value = "CONTENT_ATTRIBUTE")]
        ContentAttribute
    }
}
