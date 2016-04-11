using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Quantumart.QP8.BLL.Services.VisualEditor
{
    public class VisualEditorPlugin : EntityObject
    {
        internal const int BaseCommandOrder = 10;

        internal VisualEditorPlugin() { }

        internal static VisualEditorPlugin Create()
        {
            return new VisualEditorPlugin
            {
                Order = VisualEditorRepository.GetPluginMaxOrder() + 1
            };
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.VisualEditorPlugin;

        public override int ParentEntityId => 1;

        [MaxLengthValidator(512, MessageTemplateResourceName = "UrlPrefixMaxLengthExceeded", MessageTemplateResourceType = typeof(VisualEditorStrings))]
        [LocalizedDisplayName("Url", NameResourceType = typeof(VisualEditorStrings))]
        public string Url { get; set; }

        [LocalizedDisplayName("Order", NameResourceType = typeof(VisualEditorStrings))]
        public int Order { get; set; }

        public List<VisualEditorCommand> VeCommands { get; set; }

        public int[] ForceCommandIds { get; set; }

        public void DoCustomBinding(List<VisualEditorCommand> jsonCommands)
        {
            Dictionary<int, VisualEditorCommand> oldVeCommands = null;
            if (VeCommands != null)
            {
                oldVeCommands = VeCommands.ToDictionary(n => n.Id, m => m);
            }

            var rowOrder = VisualEditorRepository.GetCommandMaxRowOrder();
            var toolbarInRowOrder = BaseCommandOrder + Order;
            var commandInGroupOrder = 0;

            foreach (var command in jsonCommands)
            {

                command.RowOrder = rowOrder;
                command.ToolbarInRowOrder = toolbarInRowOrder;
                command.GroupInToolbarOrder = 0;
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
            var errors = new RulesException<VisualEditorPlugin>();
            base.Validate(errors);

            if (!string.IsNullOrEmpty(Url) && !Regex.IsMatch(Url, RegularExpressions.AbsoluteWebFolderUrl) && !Regex.IsMatch(Url, RegularExpressions.RelativeWebFolderUrl))
            {
                errors.ErrorFor(n => Url, VisualEditorStrings.UrlPrefixInvalidFormat);
            }

            var duplicateCurrentNames = VeCommands.GroupBy(c => c.Name).Where(g => g.Count() > 1).Select(x => x.Key).ToArray();
            var duplicateCurrentAliases = VeCommands.GroupBy(c => c.Alias).Where(g => g.Count() > 1).Select(x => x.Key).ToArray();
            if (VeCommands.Count == 0)
            {
                errors.ErrorForModel(VisualEditorStrings.CommandsRequired);
            }
            else
            {
                var veCommandsArray = VeCommands.ToArray();
                for (var i = 0; i < veCommandsArray.Length; i++)
                {
                    ValidateVeCommand(veCommandsArray[i], errors, i + 1, duplicateCurrentNames, duplicateCurrentAliases);
                }
            }
            if (!errors.IsEmpty)
            {
                throw errors;
            }
        }

        private void ValidateVeCommand(VisualEditorCommand command, RulesException errors, int index, IEnumerable<string> dupNames, IEnumerable<string> dupAliases)
        {
            if (!string.IsNullOrWhiteSpace(command.Name) && dupNames.Contains(command.Name))
            {
                errors.ErrorForModel(string.Format(VisualEditorStrings.CommandNameDuplicate, index));
                command.IsInvalid = true;
            }

            if (!string.IsNullOrWhiteSpace(command.Alias) && dupAliases.Contains(command.Alias))
            {
                errors.ErrorForModel(string.Format(VisualEditorStrings.CommandAliasDuplicate, index));
                command.IsInvalid = true;
            }

            if (string.IsNullOrWhiteSpace(command.Name))
            {
                errors.ErrorForModel(string.Format(VisualEditorStrings.CommandNameRequired, index));
                command.IsInvalid = true;
            }

            if (!string.IsNullOrWhiteSpace(command.Name) && Regex.IsMatch(command.Name, RegularExpressions.InvalidEntityName))
            {
                errors.ErrorForModel(string.Format(VisualEditorStrings.CommandNameInvalidFormat, index));
                command.IsInvalid = true;
            }

            if (string.IsNullOrWhiteSpace(command.Alias))
            {
                errors.ErrorForModel(string.Format(VisualEditorStrings.CommandAliasRequired, index));
                command.IsInvalid = true;
            }

            if (command.Name.Length > 255)
            {
                errors.ErrorForModel(string.Format(VisualEditorStrings.CommandNameMaxLengthExceeded, index));
                command.IsInvalid = true;
            }

            if (command.Alias.Length > 255)
            {
                errors.ErrorForModel(string.Format(VisualEditorStrings.CommandAliasMaxLengthExceeded, index));
                command.IsInvalid = true;
            }

            if (!VisualEditorRepository.IsCommandNameFree(command.Name, Id))
            {
                errors.ErrorForModel(string.Format(VisualEditorStrings.CommandNameNonUnique, index));
                command.IsInvalid = true;
            }

            if (!VisualEditorRepository.IsCommandAliasFree(command.Alias, Id))
            {
                errors.ErrorForModel(string.Format(VisualEditorStrings.CommandAliasNonUnique, index));
                command.IsInvalid = true;
            }
        }
    }
}
