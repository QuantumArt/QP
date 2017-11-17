using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using Quantumart.QP8.BLL.Services.VisualEditor;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.VisualEditor
{
    public class VisualEditorPluginViewModel : EntityViewModel
    {
        private List<VisualEditorCommand> _jsonCommands;

        private List<VisualEditorCommand> _veCommands;

        public override string EntityTypeCode => Constants.EntityTypeCode.VisualEditorPlugin;

        public override string ActionCode => IsNew ? Constants.ActionCode.AddNewVisualEditorPlugin : Constants.ActionCode.VisualEditorPluginProperties;

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public string AggregationListItemsVeCommandsDisplay { get; set; }

        public new VisualEditorPlugin Data
        {
            get => (VisualEditorPlugin)EntityData;
            set => EntityData = value;
        }

        public static VisualEditorPluginViewModel Create(VisualEditorPlugin plugin, string tabId, int parentId) => Create<VisualEditorPluginViewModel>(plugin, tabId, parentId);

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

        internal void DoCustomBinding()
        {
            _jsonCommands = JsonConvert.DeserializeObject<List<VisualEditorCommand>>(AggregationListItemsVeCommandsDisplay);
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
