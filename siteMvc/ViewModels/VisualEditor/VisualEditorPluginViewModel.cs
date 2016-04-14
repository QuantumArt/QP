using Quantumart.QP8.BLL.Services.VisualEditor;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Quantumart.QP8.WebMvc.ViewModels.VisualEditor
{
    public class VisualEditorPluginViewModel : EntityViewModel
    {
        public override string EntityTypeCode => Constants.EntityTypeCode.VisualEditorPlugin;

        public override string ActionCode => IsNew ? Constants.ActionCode.AddNewVisualEditorPlugin : Constants.ActionCode.VisualEditorPluginProperties;

        public new VisualEditorPlugin Data
        {
            get { return (VisualEditorPlugin)EntityData; }
            set { EntityData = value; }
        }

        public static VisualEditorPluginViewModel Create(VisualEditorPlugin plugin, string tabId, int parentId)
        {
            return Create<VisualEditorPluginViewModel>(plugin, tabId, parentId);
        }

        private List<VisualEditorCommand> _veCommands;

        [LocalizedDisplayName("Commands", NameResourceType = typeof(VisualEditorStrings))]
        public IEnumerable<object> VeCommandsDisplay
        {
            get
            {
                if (_veCommands == null)
                {
                    _veCommands = Data.VeCommands?.ToList() ?? new List<VisualEditorCommand>();
                }

                return _veCommands.Select(x => new { x.Alias, x.Name, Invalid = x.IsInvalid, x.Id });
            }
        }

        public string AggregationListItemsVeCommandsDisplay { get; set; }

        private List<VisualEditorCommand> _jsonCommands;

        internal void DoCustomBinding()
        {
            _jsonCommands = new JavaScriptSerializer().Deserialize<List<VisualEditorCommand>>(AggregationListItemsVeCommandsDisplay);
            Data.DoCustomBinding(_jsonCommands);
        }

        public override void Validate(ModelStateDictionary modelState)
        {
            _jsonCommands.ForEach(x => x.IsInvalid = false);
            modelState.Clear();
            base.Validate(modelState);
        }
    }
}
