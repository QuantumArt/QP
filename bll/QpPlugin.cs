using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using QP8.Infrastructure.Web.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.VisualEditor;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL
{
    public class QpPlugin : EntityObject
    {
        internal const int BaseCommandOrder = 10;

        internal QpPlugin()
        {
            Fields = new List<QpPluginField>();
        }

        internal static QpPlugin Create() => new QpPlugin
        {
            Order = QpPluginRepository.GetPluginMaxOrder() + 1
        };

        public override string EntityTypeCode => Constants.EntityTypeCode.QpPlugin;

        public override int ParentEntityId => 1;

        [StringLength(512, ErrorMessageResourceName = "ServiceUrlMaxLengthExceeded", ErrorMessageResourceType = typeof(QpPluginStrings))]
        [Display(Name = "ServiceUrl", ResourceType = typeof(QpPluginStrings))]
        public string ServiceUrl { get; set; }

        [Display(Name = "Order", ResourceType = typeof(QpPluginStrings))]
        public int Order { get; set; }

        [Display(Name = "Contract", ResourceType = typeof(QpPluginStrings))]
        public string Contract { get; set; }

        [Display(Name = "Code", ResourceType = typeof(QpPluginStrings))]
        public string Code { get; set; }

        [Display(Name = "Version", ResourceType = typeof(QpPluginStrings))]
        public string Version { get; set; }

        [Display(Name = "InstanceKey", ResourceType = typeof(QpPluginStrings))]
        public string InstanceKey { get; set; }

        public List<QpPluginField> Fields { get; set; }

        public int[] ForceFieldIds { get; set; }

        public QpPluginContract ParsedContract { get; set; }

        public string ParsedContractInvalidMessage { get; set; }

        public override void DoCustomBinding()
        {
            try
            {
                ParsedContract = JsonConvert.DeserializeObject<QpPluginContract>(Contract);
            }
            catch (Exception ex)
            {
                ParsedContractInvalidMessage = ex.Message;
            }

            if (ParsedContract != null)
            {
                Code = ParsedContract.Code;
                Version = ParsedContract.Version;
                InstanceKey = ParsedContract.InstanceKey;
                Description = ParsedContract.Description;
                var fieldNames = new HashSet<string>(Fields.Select(n => n.Name.ToLower()));
                Fields.AddRange(ParsedContract.Fields.Where(n => !fieldNames.Contains(n.Name.ToLower())));
            }
        }

        public override void Validate()
        {
            var errors = new RulesException<QpPlugin>();
            base.Validate(errors);

            if (!string.IsNullOrEmpty(ParsedContractInvalidMessage))
            {
                errors.ErrorFor(n => Contract, ParsedContractInvalidMessage);
            }

            if (!string.IsNullOrEmpty(ServiceUrl) && !UrlHelpers.IsValidWebFolderUrl(ServiceUrl))
            {
                errors.ErrorFor(n => ServiceUrl, QpPluginStrings.ServiceUrlInvalidFormat);
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

            if (!string.IsNullOrWhiteSpace(command.Name) && !Regex.IsMatch(command.Name, RegularExpressions.EntityName))
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
