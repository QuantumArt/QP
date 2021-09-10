namespace QP8.Plugins.Contract
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
