namespace Quantumart.QP8.BLL.Services.VisualEditor
{
    public class VisualEditorConfig
    {
        private readonly Field _field;
        private const int CkeditorEnterP = 1;
        private const int CkeditorEnterBr = 2;
        private const int ToolbarsHeight = 110;

        public string Language => QPContext.CurrentCultureName;

        public string DocType => !string.IsNullOrWhiteSpace(_field.DocType) ? _field.DocType : @"<!doctype html>";

        public bool FullPage => _field.FullPage;

        public int EnterMode => _field.PEnterMode ? CkeditorEnterP : CkeditorEnterBr;

        public int ShiftEnterMode => _field.PEnterMode ? CkeditorEnterBr : CkeditorEnterP;

        public bool UseEnglishQuotes => _field.UseEnglishQuotes;

        public bool DisableListAutoWrap => _field.DisableListAutoWrap;

        public int Height => _field.VisualEditorHeight - ToolbarsHeight;

        public string BodyClass => _field.RootElementClass;

        internal VisualEditorConfig(Field field)
        {
            _field = field;
        }
    }
}
