using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
    public class PageTemplate : LockableEntityObject
    {
        public PageTemplate()
        {
            _additionalNamespaceItems = new InitPropertyValue<IEnumerable<AdditionalNamespace>>(ParseUsings);
        }

        private const string RegexString = @"[,\s\n]+";

        private const string Delimeter = ",";

        [Display(Name = "NetLanguage", ResourceType = typeof(TemplateStrings))]
        public int? NetLanguageId { get; set; }

        [Display(Name = "NetClassName", ResourceType = typeof(TemplateStrings))]
        [StringLength(255, ErrorMessageResourceName = "NetNameMaxLengthExceeded", ErrorMessageResourceType = typeof(TemplateStrings))]
        public string NetTemplateName { get; set; }

        [Display(Name = "FolderName", ResourceType = typeof(TemplateStrings))]
        [StringLength(255, ErrorMessageResourceName = "FolderNameMaxLengthExceeded", ErrorMessageResourceType = typeof(TemplateStrings))]
        public string TemplateFolder { get; set; }

        [Display(Name = "MaxNumberOfFormatStoredVersions", ResourceType = typeof(TemplateStrings))]
        public int MaxNumOfFormatStoredVersions { get; set; }

        [Display(Name = "SendNoCacheHeader", ResourceType = typeof(TemplateStrings))]
        public bool SendNocacheHeaders { get; set; }

        [Display(Name = "CodeBehind", ResourceType = typeof(TemplateStrings))]
        public string CodeBehind { get; set; }

        [Display(Name = "Presentation", ResourceType = typeof(TemplateStrings))]
        public string TemplateBody { get; set; }

        [Display(Name = "PresentationForPreview", ResourceType = typeof(TemplateStrings))]
        public string PreviewTemplateBody { get; set; }

        [Display(Name = "CodeBehindForPreview", ResourceType = typeof(TemplateStrings))]
        public string PreviewCodeBehind { get; set; }

        [Display(Name = "EnableViewState", ResourceType = typeof(TemplateStrings))]
        public bool EnableViewstate { get; set; }

        [Display(Name = "DisableAutoDataBinding", ResourceType = typeof(TemplateStrings))]
        public bool DisableDatabind { get; set; }

        [StringLength(255, ErrorMessageResourceName = "CustomClassForGenericsMaxLengthExceeded", ErrorMessageResourceType = typeof(TemplateStrings))]
        [Display(Name = "CustomClassForGenerics", ResourceType = typeof(TemplateStrings))]
        public string CustomClassForGenerics { get; set; }

        [StringLength(255, ErrorMessageResourceName = "CustomClassForPublishingContainersMaxLengthExceeded", ErrorMessageResourceType = typeof(TemplateStrings))]
        [Display(Name = "CustomClassForPublishingContainers", ResourceType = typeof(TemplateStrings))]
        public string CustomClassForContainers { get; set; }

        [Display(Name = "CustomClassForPublishingForms", ResourceType = typeof(TemplateStrings))]
        [StringLength(255, ErrorMessageResourceName = "CustomClassForFormsMaxLengthExceeded", ErrorMessageResourceType = typeof(TemplateStrings))]
        public string CustomClassForForms { get; set; }

        [Display(Name = "Charset", ResourceType = typeof(TemplateStrings))]
        public string Charset { get; set; }

        [Display(Name = "Locale", ResourceType = typeof(TemplateStrings))]
        public int Locale { get; set; }

        [Display(Name = "CustomClassForPages", ResourceType = typeof(TemplateStrings))]
        [StringLength(255, ErrorMessageResourceName = "CustomClassForPagesMaxLengthExceeded", ErrorMessageResourceType = typeof(TemplateStrings))]
        public string CustomClassForPages { get; set; }

        [Display(Name = "CustomClassForTemplate", ResourceType = typeof(TemplateStrings))]
        [StringLength(255, ErrorMessageResourceName = "CustomClassForTemplatesMaxLengthExceeded", ErrorMessageResourceType = typeof(TemplateStrings))]
        public string TemplateCustomClass { get; set; }

        public int SiteId { get; set; }

        public bool IsSystem { get; set; }

        public string Using { get; set; }

        internal static PageTemplate Create(int parentId, Site site) => new PageTemplate
        {
            SiteId = parentId,
            Site = site,
            MaxNumOfFormatStoredVersions = 100,
            SendNocacheHeaders = true,
            EnableViewstate = true,
            Charset = PageTemplateRepository.GetCharsetByName("utf-8").Subj,
            Locale = PageTemplateRepository.GetLocaleByName("Russian").Id
        };

        [Display(Name = "ApplyToExistingPagesAndObjects", ResourceType = typeof(TemplateStrings))]
        public bool ApplyToExistingPagesAndObjects { get; set; }

        [Display(Name = "ApplyToExistingObjects", ResourceType = typeof(TemplateStrings))]
        public bool ApplyToExistingObjects { get; set; }

        [Display(Name = "OverridePageSettings", ResourceType = typeof(TemplateStrings))]
        public bool OverridePageSettings { get; set; }

        [Display(Name = "OverrideObjectSettings", ResourceType = typeof(TemplateStrings))]
        public bool OverrideObjectSettings { get; set; }

        public override void Validate()
        {
            var errors = new RulesException<PageTemplate>();
            base.Validate(errors);

            if (SiteIsDotNet)
            {
                if (!Regex.IsMatch(NetTemplateName, RegularExpressions.NetName))
                {
                    errors.ErrorFor(x => x.NetTemplateName, TemplateStrings.NetNameInvalidFormat);
                }

                if (!PageTemplateRepository.PageTemplateNetNameUnique(NetTemplateName, SiteId, Id))
                {
                    errors.ErrorFor(x => x.NetTemplateName, TemplateStrings.NetNameNotUnique);
                }
            }

            if (!string.IsNullOrWhiteSpace(TemplateFolder))
            {
                if (!Regex.IsMatch(TemplateFolder, RegularExpressions.RelativeWindowsFolderPath))
                {
                    errors.ErrorFor(x => x.TemplateFolder, TemplateStrings.FolderNameInvalidFormat);
                }
            }

            if (!string.IsNullOrWhiteSpace(CustomClassForPages))
            {
                if (!Regex.IsMatch(CustomClassForPages, RegularExpressions.NetName))
                {
                    errors.ErrorFor(x => x.CustomClassForPages, TemplateStrings.CustomClassForPagesInvalidFormat);
                }
            }

            if (!string.IsNullOrWhiteSpace(TemplateCustomClass))
            {
                if (!Regex.IsMatch(TemplateCustomClass, RegularExpressions.NetName))
                {
                    errors.ErrorFor(x => x.CustomClassForPages, TemplateStrings.CustomClassForTemplateInvalidFormat);
                }
            }

            if (!string.IsNullOrWhiteSpace(CustomClassForGenerics))
            {
                if (!Regex.IsMatch(CustomClassForGenerics, RegularExpressions.NetName))
                {
                    errors.ErrorFor(x => x.CustomClassForGenerics, TemplateStrings.CustomClassForGenericsInvalidFormat);
                }
            }

            if (!string.IsNullOrWhiteSpace(CustomClassForContainers))
            {
                if (!Regex.IsMatch(CustomClassForContainers, RegularExpressions.NetName))
                {
                    errors.ErrorFor(x => x.CustomClassForContainers, TemplateStrings.CustomClassForContainersInvalidFormat);
                }
            }

            if (!string.IsNullOrWhiteSpace(CustomClassForForms))
            {
                if (!Regex.IsMatch(CustomClassForForms, RegularExpressions.NetName))
                {
                    errors.ErrorFor(x => x.CustomClassForForms, TemplateStrings.CustomClassForFormsInvalidFormat);
                }
            }

            if (Using.Length > 512)
            {
                errors.ErrorFor(x => x.Using, TemplateStrings.UsingTotalLengthExceeded);
            }

            var usingsArray = AdditionalNamespaceItems.ToArray();
            for (var i = 0; i < usingsArray.Length; i++)
            {
                ValidateUsing(usingsArray[i], errors, i + 1);
            }

            if (!errors.IsEmpty)
            {
                throw errors;
            }
        }

        private static void ValidateUsing(AdditionalNamespace additionalNamespace, RulesException<PageTemplate> errors, int index)
        {
            if (string.IsNullOrWhiteSpace(additionalNamespace.Name))
            {
                errors.ErrorForModel(string.Format(TemplateStrings.AdditionalNamespaceRequired, index));
                additionalNamespace.Invalid = true;
                return;
            }

            if (additionalNamespace.Name.Length > 255)
            {
                errors.ErrorForModel(string.Format(TemplateStrings.AdditionalNamespaceLengthExceeded, index));
                additionalNamespace.Invalid = true;
            }

            if (!Regex.IsMatch(additionalNamespace.Name, RegularExpressions.FullQualifiedNetName))
            {
                errors.ErrorForModel(string.Format(TemplateStrings.AdditionalNamespaceInvalidFormat, index));
                additionalNamespace.Invalid = true;
            }
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.PageTemplate;

        public override int ParentEntityId => SiteId;

        [ValidateNever]
        [BindNever]
        public Site Site { get; set; }

        public override string LockedByAnyoneElseMessage => TemplateStrings.LockedByAnyoneElse;

        private IEnumerable<AdditionalNamespace> ParseUsings()
        {
            var regex = new Regex(RegexString);
            return string.IsNullOrEmpty(Using) ? new List<AdditionalNamespace>() : regex.Split(Using).Select(x => new AdditionalNamespace { Name = x, Invalid = false }).ToList();
        }

        private readonly InitPropertyValue<IEnumerable<AdditionalNamespace>> _additionalNamespaceItems;

        [Display(Name = "AdditionalNamespases", ResourceType = typeof(TemplateStrings))]
        public IEnumerable<AdditionalNamespace> AdditionalNamespaceItems
        {
            get => _additionalNamespaceItems.Value;
            set => _additionalNamespaceItems.Value = value;
        }

        public void SetUsings()
        {
            Using = string.Join(Delimeter, AdditionalNamespaceItems.Select(x => x.Name).ToArray());
        }

        public bool SiteIsDotNet => Site.AssemblingType == AssemblingType.AspDotNet;

        public void GenerateNetName()
        {
            NetTemplateName = Name.Replace(" ", "_");
        }
    }
}
