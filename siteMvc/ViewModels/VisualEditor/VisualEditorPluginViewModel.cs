using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Quantumart.QP8.BLL.Services.VisualEditor;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.VisualEditor
{
    public class VisualEditorPluginViewModel : EntityViewModel
    {
        private List<VisualEditorCommand> _jsonCommands;

        private List<VisualEditorCommand> _veCommands;

        public override string EntityTypeCode => Constants.EntityTypeCode.VisualEditorPlugin;

        public override string ActionCode => IsNew ? Constants.ActionCode.AddNewVisualEditorPlugin : Constants.ActionCode.VisualEditorPluginProperties;

        public string AggregationListItemsVeCommandsDisplay { get; set; }

        public new VisualEditorPlugin Data
        {
            get => (VisualEditorPlugin)EntityData;
            set => EntityData = value;
        }

        public static VisualEditorPluginViewModel Create(VisualEditorPlugin plugin, string tabId, int parentId) => Create<VisualEditorPluginViewModel>(plugin, tabId, parentId);

        [Display(Name = "Commands", ResourceType = typeof(VisualEditorStrings))]
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

        public override void DoCustomBinding()
        {
            _jsonCommands = JsonConvert.DeserializeObject<List<VisualEditorCommand>>(AggregationListItemsVeCommandsDisplay);
            Data.DoCustomBinding(_jsonCommands);
        }

        public override void Validate()
        {
            _jsonCommands.ForEach(x => x.IsInvalid = false);
            base.Validate();
        }
    }
}
