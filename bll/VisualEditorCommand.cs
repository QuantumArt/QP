namespace Quantumart.QP8.BLL
{
    public class VisualEditorCommand : EntityObject
    {
        public string Alias { get; set; }

        public int RowOrder { get; set; }

        public int ToolbarInRowOrder { get; set; }

        public int GroupInToolbarOrder { get; set; }

        public int CommandInGroupOrder { get; set; }

        public bool On { get; set; }

        public bool IsInvalid { get; set; }

        public VisualEditorPlugin VePlugin { get; set; }

        public int? PluginId { get; set; }
    }
}
