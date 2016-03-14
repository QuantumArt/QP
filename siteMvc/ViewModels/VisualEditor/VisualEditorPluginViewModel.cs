using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.VisualEditor
{
	public class VisualEditorPluginViewModel : EntityViewModel
	{
		public override string EntityTypeCode
		{
			get { return C.EntityTypeCode.VisualEditorPlugin; }
		}

		public override string ActionCode
		{
			get { return IsNew ? C.ActionCode.AddNewVisualEditorPlugin : C.ActionCode.VisualEditorPluginProperties; }
		}

		public new VisualEditorPlugin Data
		{
			get
			{
				return (VisualEditorPlugin)EntityData;
			}
			set
			{
				EntityData = value;
			}
		}

		public static VisualEditorPluginViewModel Create(VisualEditorPlugin plugin, string tabId, int parentId)
		{
			var model = EntityViewModel.Create<VisualEditorPluginViewModel>(plugin, tabId, parentId);			
			return model;
		}

		private List<VisualEditorCommand> _VeCommands;

		[LocalizedDisplayName("Commands", NameResourceType = typeof(VisualEditorStrings))]
		public IEnumerable<object> VeCommandsDisplay
		{  	
			get
			{
				if (_VeCommands == null)
					_VeCommands = Data.VeCommands != null? Data.VeCommands.ToList() : new List<VisualEditorCommand>();
				return _VeCommands.Select(x => new { x.Alias, x.Name, Invalid = x.IsInvalid, x.Id});
			}
		}

		public string AggregationListItems_VeCommandsDisplay { get; set; }

		private List<VisualEditorCommand> _JsonCommands;

		internal void DoCustomBinding()
        {
			_JsonCommands = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<List<VisualEditorCommand>>(AggregationListItems_VeCommandsDisplay);
			Data.DoCustomBinding(_JsonCommands);
        }

		public override void Validate(ModelStateDictionary modelState)
		{
			_JsonCommands.ForEach(x => x.IsInvalid = false);
			modelState.Clear();
			base.Validate(modelState);
		}
	}
}