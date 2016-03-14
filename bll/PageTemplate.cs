using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Quantumart.QP8.BLL
{
    public class PageTemplate : LockableEntityObject
    {
		public PageTemplate()
		{
			_AdditionalNamespaceItems = new InitPropertyValue<IEnumerable<AdditionalNamespace>>(() => this.ParseUsings());
		}

		private const string _regexString = @"[,\s\n]+";

		private const string _delimeter = ",";

        [LocalizedDisplayName("NetLanguage", NameResourceType = typeof(TemplateStrings))]
        public int? NetLanguageId { get; set; }		
		
		[LocalizedDisplayName("NetClassName", NameResourceType = typeof(TemplateStrings))]
		[MaxLengthValidator(255, MessageTemplateResourceName = "NetTemplateNameMaxLengthExceeded", MessageTemplateResourceType = typeof(TemplateStrings))]
		public string NetTemplateName { get; set; }

        [LocalizedDisplayName("FolderName", NameResourceType = typeof(TemplateStrings))]
		[MaxLengthValidator(255, MessageTemplateResourceName = "TemplateFolderNameMaxLengthExceeded", MessageTemplateResourceType = typeof(TemplateStrings))]
        public string TemplateFolder { get; set; }

        [LocalizedDisplayName("MaxNumberOfFormatStoredVersions", NameResourceType = typeof(TemplateStrings))]
        public int MaxNumOfFormatStoredVersions { get; set; }

        [LocalizedDisplayName("SendNoCacheHeader", NameResourceType = typeof(TemplateStrings))]
        public bool SendNocacheHeaders { get; set; }

        [LocalizedDisplayName("CodeBehind", NameResourceType = typeof(TemplateStrings))]
        public string CodeBehind { get; set; }

        [LocalizedDisplayName("Presentation", NameResourceType = typeof(TemplateStrings))]
        public string TemplateBody { get; set; }

        [LocalizedDisplayName("PresentationForPreview", NameResourceType = typeof(TemplateStrings))]
        public string PreviewTemplateBody { get; set; }

        [LocalizedDisplayName("CodeBehindForPreview", NameResourceType = typeof(TemplateStrings))]
        public string PreviewCodeBehind { get; set; }

        [LocalizedDisplayName("EnableViewState", NameResourceType = typeof(TemplateStrings))]
        public bool EnableViewstate { get; set; }

        [LocalizedDisplayName("DisableAutoDataBinding", NameResourceType = typeof(TemplateStrings))]
        public bool DisableDatabind { get; set; }

		[MaxLengthValidator(255, MessageTemplateResourceName = "CustomClassForGenericsMaxLengthExceeded", MessageTemplateResourceType = typeof(TemplateStrings))]
        [LocalizedDisplayName("CustomClassForGenerics", NameResourceType = typeof(TemplateStrings))]
        public string CustomClassForGenerics { get; set; }

		[MaxLengthValidator(255, MessageTemplateResourceName = "CustomClassForPublishingContainersMaxLengthExceeded", MessageTemplateResourceType = typeof(TemplateStrings))]
        [LocalizedDisplayName("CustomClassForPublishingContainers", NameResourceType = typeof(TemplateStrings))]
        public string CustomClassForContainers { get; set; }

        [LocalizedDisplayName("CustomClassForPublishingForms", NameResourceType = typeof(TemplateStrings))]
		[MaxLengthValidator(255, MessageTemplateResourceName = "CustomClassForFormsMaxLengthExceeded", MessageTemplateResourceType = typeof(TemplateStrings))]
        public string CustomClassForForms { get; set; }

        [LocalizedDisplayName("Charset", NameResourceType = typeof(TemplateStrings))]
        public string Charset { get; set; }

        [LocalizedDisplayName("Locale", NameResourceType = typeof(TemplateStrings))]
        public int Locale { get; set; }

        [LocalizedDisplayName("CustomClassForPages", NameResourceType = typeof(TemplateStrings))]
		[MaxLengthValidator(255, MessageTemplateResourceName = "CustomClassForPagesMaxLengthExceeded", MessageTemplateResourceType = typeof(TemplateStrings))]
        public string CustomClassForPages { get; set; }

        [LocalizedDisplayName("CustomClassForTemplate", NameResourceType = typeof(TemplateStrings))]
		[MaxLengthValidator(255, MessageTemplateResourceName = "CustomClassForTemplatesMaxLengthExceeded", MessageTemplateResourceType = typeof(TemplateStrings))]
        public string TemplateCustomClass { get; set; }

        public int SiteId { get; set; }

        public bool IsSystem { get; set; }

		public string Using { get; set; }

        internal static PageTemplate Create(int parentId, Site site)
        {
            return new PageTemplate() 
			{ 
				SiteId = parentId,
				Site = site,
				MaxNumOfFormatStoredVersions = 100,
				SendNocacheHeaders = true,
				EnableViewstate = true,
				Charset = PageTemplateRepository.GetCharsetByName(Constants.Charset.Utf8).Subj,
				Locale = PageTemplateRepository.GetLocaleByName(Constants.Locale.russian).Id
			};
        }

		[LocalizedDisplayName("ApplyToExistingPagesAndObjects", NameResourceType = typeof(TemplateStrings))]
		public bool ApplyToExistingPagesAndObjects { get; set; }

		[LocalizedDisplayName("ApplyToExistingObjects", NameResourceType = typeof(TemplateStrings))]
		public bool ApplyToExistingObjects { get; set; }

		[LocalizedDisplayName("OverridePageSettings", NameResourceType = typeof(TemplateStrings))]
		public bool OverridePageSettings { get; set; }

		[LocalizedDisplayName("OverrideObjectSettings", NameResourceType = typeof(TemplateStrings))]
		public bool OverrideObjectSettings { get; set; }

        public override void Validate()
        {
            RulesException<PageTemplate> errors = new RulesException<PageTemplate>();
            base.Validate(errors);

			if (SiteIsDotNet)
			{
				if (!Regex.IsMatch(NetTemplateName, RegularExpressions.NetName))
					errors.ErrorFor(x => x.NetTemplateName, TemplateStrings.NetNameInvalidFormat);
				if (!PageTemplateRepository.PageTemplateNetNameUnique(NetTemplateName, SiteId, Id))
					errors.ErrorFor(x => x.NetTemplateName, TemplateStrings.NetNameNotUnique);
			}

			if (!string.IsNullOrWhiteSpace(TemplateFolder))
			{
				if (!Regex.IsMatch(TemplateFolder, RegularExpressions.RelativeWindowsFolderPath))
					errors.ErrorFor(x => x.TemplateFolder, TemplateStrings.FolderNameInvalidFormat);
			}

			if (!string.IsNullOrWhiteSpace(CustomClassForPages))
			{
				if (!Regex.IsMatch(CustomClassForPages, RegularExpressions.NetName))
					errors.ErrorFor(x => x.CustomClassForPages, TemplateStrings.CustomClassForPagesInvalidFormat);
			}

			if (!string.IsNullOrWhiteSpace(TemplateCustomClass))
			{
				if (!Regex.IsMatch(TemplateCustomClass, RegularExpressions.NetName))
					errors.ErrorFor(x => x.CustomClassForPages, TemplateStrings.CustomClassForTemplateInvalidFormat);
			}

			if (!string.IsNullOrWhiteSpace(CustomClassForGenerics))
			{
				if (!Regex.IsMatch(CustomClassForGenerics, RegularExpressions.NetName))
					errors.ErrorFor(x => x.CustomClassForGenerics, TemplateStrings.CustomClassForGenericsInvalidFormat);
			}

			if (!string.IsNullOrWhiteSpace(CustomClassForContainers))
			{
				if (!Regex.IsMatch(CustomClassForContainers, RegularExpressions.NetName))
					errors.ErrorFor(x => x.CustomClassForContainers, TemplateStrings.CustomClassForContainersInvalidFormat);
			}

			if (!string.IsNullOrWhiteSpace(CustomClassForForms))
			{
				if (!Regex.IsMatch(CustomClassForForms, RegularExpressions.NetName))
					errors.ErrorFor(x => x.CustomClassForForms, TemplateStrings.CustomClassForFormsInvalidFormat);
			}

			if (Using.Length > 512)
			{
				errors.ErrorFor(x => x.Using, TemplateStrings.UsingTotalLengthExceeded);
			}

			var usingsArray = AdditionalNamespaceItems.ToArray();
			for (int i = 0; i < usingsArray.Length; i++)
				ValidateUsing(usingsArray[i], errors, i + 1);

			if (!errors.IsEmpty)
				throw errors;
        }

		private void ValidateUsing(AdditionalNamespace additionalNamespace, RulesException<PageTemplate> errors, int index)
		{
			if (string.IsNullOrWhiteSpace(additionalNamespace.Name))
			{
				errors.ErrorForModel(String.Format(TemplateStrings.AdditionalNamespaceRequired, index));
				additionalNamespace.Invalid = true;
				return;
			}
			
			if (additionalNamespace.Name.Length > 255)
			{
				errors.ErrorForModel(String.Format(TemplateStrings.AdditionalNamespaceLengthExceeded, index));
				additionalNamespace.Invalid = true;
			}

			if (!Regex.IsMatch(additionalNamespace.Name, RegularExpressions.FullQualifiedNetName))
			{
				errors.ErrorForModel(String.Format(TemplateStrings.AdditionalNamespaceInvalidFormat, index));
				additionalNamespace.Invalid = true;
			}
		}

        public override string EntityTypeCode
        {
            get
            {
                return Constants.EntityTypeCode.PageTemplate;
            }
        }

        public override int ParentEntityId
        {
            get
            {
                return SiteId;
            }
        }

		public Site Site { get; set; }

        public override string LockedByAnyoneElseMessage
        {
            get { return TemplateStrings.LockedByAnyoneElse; }
        }

		private IEnumerable<AdditionalNamespace> ParseUsings()
		{
			Regex regex = new Regex(_regexString);
			return string.IsNullOrEmpty(this.Using) ? new List<AdditionalNamespace>() : regex.Split(Using).Select(x => new AdditionalNamespace { Name = x, Invalid = false }).ToList();
		}		

		private InitPropertyValue<IEnumerable<AdditionalNamespace>> _AdditionalNamespaceItems;		

		[LocalizedDisplayName("AdditionalNamespases", NameResourceType = typeof(TemplateStrings))]
		public IEnumerable<AdditionalNamespace> AdditionalNamespaceItems
		{
			get { return _AdditionalNamespaceItems.Value; }
			set { _AdditionalNamespaceItems.Value = value; }
		}

		public void SetUsings()
		{
			Using = string.Join(_delimeter, AdditionalNamespaceItems.Select(x => x.Name).ToArray());
		}

		public bool SiteIsDotNet
		{
			get
			{
				return Site.AssemblingType == Constants.AssemblingType.AspDotNet;
			}
		}

		public void GenerateNetName()
		{
			this.NetTemplateName = this.Name.Replace(" ", "_");
		}

		private string UploadUrlPlaceHolder
		{
			get { return "<%=upload_url%>"; }
		}

		private string SiteUrlPlaceHolder
		{
			get { return "<%=site_url%>"; }
		}

		public void ReplacePlaceHoldersToUrls()
		{
			if (!String.IsNullOrEmpty(TemplateBody))
			{
				TemplateBody
					.Replace(UploadUrlPlaceHolder, siteUrls["ImagesLongUploadUrl"])
					.Replace(SiteUrlPlaceHolder, siteUrls["CurrentUrl"]);
			}
		}

		public void ReplaceUrlsToPlaceHolders()
		{
			if (!String.IsNullOrEmpty(TemplateBody))
			{
				TemplateBody
					.Replace(siteUrls["ImagesLongUploadUrl"], UploadUrlPlaceHolder)
					.Replace(siteUrls["StageUrl"], SiteUrlPlaceHolder)
					.Replace(siteUrls["LiveUrl"], SiteUrlPlaceHolder);
			}
		}

		private Dictionary<string, string> _siteUrls = null;

		private Dictionary<string, string> siteUrls
		{
			get
			{
				if (_siteUrls == null)
					_siteUrls = SiteUrlHelper.GetSiteUrlsBySiteId(SiteId);
				return _siteUrls;
			}
		}
	}
}