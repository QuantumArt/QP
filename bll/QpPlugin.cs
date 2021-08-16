using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using QP8.Infrastructure.Web.Helpers;
using QP8.Plugins.Contract;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL
{
    public class QpPlugin : EntityObject
    {
        internal const int BaseCommandOrder = 10;
        private const int MaxVersionLength = 10;
        private const int MaxFieldNameLength = 255;

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

        public string OldContract { get; set; }

        public DateTime OldModified { get; set; }

        public int OldLastModifiedBy { get; set; }

        public bool ContractLoaded { get; set; }

        [Display(Name = "Code", ResourceType = typeof(QpPluginStrings))]
        public string Code { get; set; }

        [Display(Name = "Version", ResourceType = typeof(QpPluginStrings))]
        public string Version { get; set; }

        public string OldVersion { get; set; }

        [Display(Name = "InstanceKey", ResourceType = typeof(QpPluginStrings))]
        public string InstanceKey { get; set; }

        public bool AllowMultipleInstances { get; set; }

        public List<QpPluginField> Fields { get; set; }

        public Dictionary<string, QpPluginField> OldFields { get; set; }

        public int[] ForceFieldIds { get; set; }

        public QpPluginContract ParsedContract { get; set; }

        public bool ContractParsed => ParsedContract != null;

        public string ParsedContractInvalidMessage { get; set; }

        public string LoadedContractInvalidMessage { get; set; }

        public override void DoCustomBinding()
        {
            if (Contract != OldContract || ContractLoaded)
            {
                if (!String.IsNullOrEmpty(Contract) && Contract != "{}")
                {
                    try
                    {
                        ParsedContract = JsonConvert.DeserializeObject<QpPluginContract>(Contract);
                    }
                    catch (Exception ex)
                    {
                        ParsedContractInvalidMessage = ex.Message;
                    }
                }
            }

            if (ContractParsed)
            {
                Code = ParsedContract.Code;
                OldVersion = Version;
                Version = ParsedContract.Version;
                InstanceKey = ParsedContract.InstanceKey;
                AllowMultipleInstances = ParsedContract.AllowMultipleInstances;
                Description = ParsedContract.Description;
                OldFields = Fields.ToDictionary(n => n.Name.ToLower(), m => m);
                Fields.AddRange(ParsedContract.Fields.Where(n => !OldFields.ContainsKey(n.Name.ToLower())));
            }
        }

        public override void Validate()
        {
            var errors = new RulesException<QpPlugin>();
            base.Validate(errors);

            if (!string.IsNullOrEmpty(LoadedContractInvalidMessage))
            {
                errors.ErrorFor(n => ServiceUrl, $"{QpPluginStrings.LoadedContractInvalidMessage}: {LoadedContractInvalidMessage}");
            }

            if (!string.IsNullOrEmpty(ParsedContractInvalidMessage))
            {
                errors.ErrorFor(n => Contract, $"{QpPluginStrings.ParsedContractInvalidMessage}: {ParsedContractInvalidMessage}");
            }

            if (!string.IsNullOrEmpty(ServiceUrl) && !UrlHelpers.IsValidWebFolderUrl(ServiceUrl))
            {
                errors.ErrorFor(n => ServiceUrl, QpPluginStrings.ServiceUrlInvalidFormat);
            }

            if (ContractParsed)
            {
                if (string.IsNullOrEmpty(Contract))
                {
                    errors.ErrorFor(n => ServiceUrl, QpPluginStrings.ContractNotEntered);
                }
                else
                {
                    if (string.IsNullOrEmpty(Code))
                    {
                        errors.ErrorFor(n => ServiceUrl, QpPluginStrings.CodeNotEntered);
                    }
                    else
                    {
                        if (QpPluginRepository.CodeExists(this))
                        {
                            errors.ErrorFor(n => Code, QpPluginStrings.CodeExists);
                        }
                    }
                    if (string.IsNullOrEmpty(Version))
                    {
                        errors.ErrorFor(n => ServiceUrl, QpPluginStrings.VersionNotEntered);
                    }
                    else
                    {
                        if (Version.Length > MaxVersionLength)
                        {
                            errors.ErrorFor(n => Version, String.Format(QpPluginStrings.VersionMaxLengthExceeded, null, MaxVersionLength));
                        }
                        else if (OldVersion == Version)
                        {
                            errors.ErrorFor(n => Version, QpPluginStrings.VersionEqual);
                        }
                    }

                    if (Fields.Any(n => String.IsNullOrEmpty(n.Name)))
                    {
                        errors.ErrorForModel(QpPluginStrings.FieldNameNotEntered);
                    }

                    if (Fields.Any(n => n.Name.Length > MaxFieldNameLength))
                    {
                        errors.ErrorForModel(String.Format(QpPluginStrings.FieldNameMaxLengthExceeded, null, MaxFieldNameLength));
                    }

                    if (Fields.GroupBy(n => n.Name.ToLower() + n.RelationType).Any(g => g.Count() > 1))
                    {
                        errors.ErrorForModel(QpPluginStrings.FieldNameDuplicate);
                    }
                }
            }

            if (!errors.IsEmpty)
            {
                throw errors;
            }
        }
    }
}
