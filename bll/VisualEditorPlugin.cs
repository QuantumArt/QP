using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Quantumart.QP8.Validators;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using System.Text.RegularExpressions;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.BLL
{
	public class VisualEditorPlugin : EntityObject
	{

		internal const int BASE_COMMAND_ORDER = 10;
		
		#region creation
		internal VisualEditorPlugin()
		{
			
		}

		internal static VisualEditorPlugin Create()
		{
			return new VisualEditorPlugin
			{
				Order = VisualEditorRepository.GetPluginMaxOrder() + 1
			};
		}
		#endregion

		#region properties
		#region simple read-write

		[MaxLengthValidator(512, MessageTemplateResourceName = "UrlPrefixMaxLengthExceeded", MessageTemplateResourceType = typeof(VisualEditorStrings))]
		[LocalizedDisplayName("Url", NameResourceType = typeof(VisualEditorStrings))]
		public string Url { get; set; }

		[LocalizedDisplayName("Order", NameResourceType = typeof(VisualEditorStrings))]
		public int Order { get; set; }

		public List<VisualEditorCommand> VeCommands { get; set; }

		public int[] ForceCommandIds { get; set; }

		#endregion
		#region simple read-only
		public override string EntityTypeCode
		{
			get
			{
				return Constants.EntityTypeCode.VisualEditorPlugin;
			}
		}

		public override int ParentEntityId
		{
			get
			{
				return 1;				
			}
		}
		#endregion
		#endregion

		public void DoCustomBinding(List<VisualEditorCommand> jsonCommands)
		{
			Dictionary<int, VisualEditorCommand> oldVeCommands = null;
			if (VeCommands != null)
				oldVeCommands = VeCommands.ToDictionary(n => n.Id, m => m);

			var rowOrder = VisualEditorRepository.GetCommandMaxRowOrder();
			var toolbarInRowOrder = BASE_COMMAND_ORDER + Order;
			var groupInToolbarOrder = 0;
			var commandInGroupOrder = 0;
			
			foreach (var command in jsonCommands)
			{

				command.RowOrder = rowOrder;
				command.ToolbarInRowOrder = toolbarInRowOrder;
				command.GroupInToolbarOrder = groupInToolbarOrder;
				command.CommandInGroupOrder = commandInGroupOrder;
				
				command.PluginId = Id;
				if (command.Id != 0 && oldVeCommands != null)
				{
					var oldCommand = oldVeCommands[command.Id];
					command.Created = oldCommand.Created;
					command.Modified = oldCommand.Modified;
				}
				commandInGroupOrder++;
			}
			VeCommands = jsonCommands;
		}

		public override void Validate()
		{
			RulesException<VisualEditorPlugin> errors = new RulesException<VisualEditorPlugin>();
			base.Validate(errors);

			if (!String.IsNullOrEmpty(Url) 
				&& !Regex.IsMatch(Url, Constants.RegularExpressions.AbsoluteWebFolderUrl) 
				&& !Regex.IsMatch(Url, Constants.RegularExpressions.RelativeWebFolderUrl)
			)
			{
				errors.ErrorFor(n => Url, VisualEditorStrings.UrlPrefixInvalidFormat);
			}

			var duplicateCurrentNames = VeCommands.GroupBy(c => c.Name).Where(g => g.Count() > 1).Select(x => x.Key).ToArray();
			var duplicateCurrentAliases = VeCommands.GroupBy(c => c.Alias).Where(g => g.Count() > 1).Select(x => x.Key).ToArray();
			
			if(VeCommands.Count == 0)
				errors.ErrorForModel(VisualEditorStrings.CommandsRequired);

			else
			{
				var VeCommandsArray = VeCommands.ToArray();
			
				for (int i = 0; i < VeCommandsArray.Length; i++)
					ValidateVeCommand(VeCommandsArray[i], errors, i + 1, duplicateCurrentNames, duplicateCurrentAliases);
			}
			if (!errors.IsEmpty)
				throw errors;
		}

	
		private void ValidateVeCommand(VisualEditorCommand command, RulesException<VisualEditorPlugin> errors, int index, string[]dupNames, string[]dupAliases)
		{
			if (!string.IsNullOrWhiteSpace(command.Name) && dupNames.Contains(command.Name))
			{
				errors.ErrorForModel(String.Format(VisualEditorStrings.CommandNameDuplicate, index));
				command.IsInvalid = true;
			}

			if (!string.IsNullOrWhiteSpace(command.Alias) && dupAliases.Contains(command.Alias))
			{
				errors.ErrorForModel(String.Format(VisualEditorStrings.CommandAliasDuplicate, index));
				command.IsInvalid = true;
			}

			if (string.IsNullOrWhiteSpace(command.Name))
			{
				errors.ErrorForModel(String.Format(VisualEditorStrings.CommandNameRequired, index));
				command.IsInvalid = true;
			}
			if (!string.IsNullOrWhiteSpace(command.Name) && Regex.IsMatch(command.Name, RegularExpressions.InvalidEntityName))
			{
				errors.ErrorForModel(String.Format(VisualEditorStrings.CommandNameInvalidFormat, index));
				command.IsInvalid = true;
			}
			if (string.IsNullOrWhiteSpace(command.Alias))
			{
				errors.ErrorForModel(String.Format(VisualEditorStrings.CommandAliasRequired, index));
				command.IsInvalid = true;
			}
			if (command.Name.Length > 255)
			{
				errors.ErrorForModel(String.Format(VisualEditorStrings.CommandNameMaxLengthExceeded, index));
				command.IsInvalid = true;
			}
			if (command.Alias.Length > 255)
			{
				errors.ErrorForModel(String.Format(VisualEditorStrings.CommandAliasMaxLengthExceeded, index));
				command.IsInvalid = true;
			}

			if (!VisualEditorRepository.IsCommandNameFree(command.Name, Id))
			{
			    errors.ErrorForModel(String.Format(VisualEditorStrings.CommandNameNonUnique, index));
			    command.IsInvalid = true;
			}

			if (!VisualEditorRepository.IsCommandAliasFree(command.Alias, Id))
			{
			    errors.ErrorForModel(String.Format(VisualEditorStrings.CommandAliasNonUnique, index));
			    command.IsInvalid = true;
			}
		}
	}
}