using System;
using System.Text.RegularExpressions;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{
    public class Page : LockableEntityObject
    {
		[RequiredValidator(MessageTemplateResourceName = "FileNameRequired", MessageTemplateResourceType = typeof(TemplateStrings))]		
		[MaxLengthValidator(255, MessageTemplateResourceName = "FileNameMaxLengthExceeded", MessageTemplateResourceType = typeof(TemplateStrings))]
		[FormatValidator(RegularExpressions.FileName, Negated = false, MessageTemplateResourceName = "FileNameInvalidFormat", MessageTemplateResourceType = typeof(TemplateStrings))]
        [LocalizedDisplayName("FileName", NameResourceType = typeof(TemplateStrings))]
        public string FileName { get; set; }

		[MaxLengthValidator(255, MessageTemplateResourceName = "CustomClassMaxLengthExceeded", MessageTemplateResourceType = typeof(TemplateStrings))]
        [LocalizedDisplayName("CustomClass", NameResourceType = typeof(TemplateStrings))]
        public string CustomClass { get; set; }

        [LocalizedDisplayName("FolderName", NameResourceType = typeof(TemplateStrings))]
        public string Folder { get; set; }

        [LocalizedDisplayName("GenerateTrace", NameResourceType = typeof(TemplateStrings))]
        public bool GenerateTrace { get; set; }

        [LocalizedDisplayName("EnableViewState", NameResourceType = typeof(TemplateStrings))]
        public bool EnableViewState { get; set; }

        [LocalizedDisplayName("SendNoCacheHeader", NameResourceType = typeof(TemplateStrings))]
        public bool SendNocacheHeaders { get; set; }

        [LocalizedDisplayName("SendLastModifiedHeader", NameResourceType = typeof(TemplateStrings))]
        public bool SendLastModifiedHeader { get; set; }

        [LocalizedDisplayName("ProxyCaching", NameResourceType = typeof(TemplateStrings))]
        public bool ProxyCache { get; set; }

        [LocalizedDisplayName("Charset", NameResourceType = typeof(TemplateStrings))]
        public string Charset { get; set; }

        [LocalizedDisplayName("Locale", NameResourceType = typeof(TemplateStrings))]
        public int Locale { get; set; }

        public int LastAssembledBy { get; set; }

        public int TemplateId { get; set; }

        public DateTime Assembled { get; set; }

		[LocalizedDisplayName("ExpiresIn", NameResourceType = typeof(TemplateStrings))]
		public int CacheHours { get; set; }

        public override string LockedByAnyoneElseMessage => string.Format(TemplateStrings.PageLockedByAnyoneElse, LockedBy);

        internal static Page Create(int parentId)
        {
			var parentTemplate = PageTemplateRepository.GetPageTemplatePropertiesById(parentId);
			return new Page 
			{ 
				TemplateId = parentId,
				PageTemplate = parentTemplate,
				CacheHours = 1,
				EnableViewState = true,
				Charset = parentTemplate.Charset,
				Locale = parentTemplate.Locale,
				SendNocacheHeaders = true				
			};
        }

		public override string EntityTypeCode => Constants.EntityTypeCode.Page;

        public override int ParentEntityId => TemplateId;

        [LocalizedDisplayName("ApplyToExistingObjects", NameResourceType = typeof(TemplateStrings))]
		public bool ApplyToExistingObjects { get; set; }

		public PageTemplate PageTemplate { get; set; }

		[LocalizedDisplayName("BrowserCaching", NameResourceType = typeof(TemplateStrings))]
		public bool BrowserCaching { get; set; }

		public override void Validate()
		{
			var errors = new RulesException<Page>();
			base.Validate(errors);

			if (!PageTemplateRepository.PageFileNameUnique(FileName, PageTemplate.Id, Id))
			{
			    errors.ErrorFor(x => x.FileName, TemplateStrings.NetNameNotUnique);
			}

		    if (!string.IsNullOrWhiteSpace(CustomClass))
			{
				if (!Regex.IsMatch(CustomClass, RegularExpressions.NetName))
				{
				    errors.ErrorFor(x => x.CustomClass, TemplateStrings.CustomClassInvalidFormat);
				}
			}

			if (!string.IsNullOrWhiteSpace(Folder))
			{
				if (!Regex.IsMatch(Folder, RegularExpressions.RelativeWindowsFolderPath))
				{
				    errors.ErrorFor(x => x.Folder, TemplateStrings.FolderNameInvalidFormat);
				}
			}

			if (PageTemplate.SiteIsDotNet)
			{
				if (!string.IsNullOrWhiteSpace(CustomClass))
				{
					if (!Regex.IsMatch(CustomClass, RegularExpressions.NetName))
					{
					    errors.ErrorFor(x => x.CustomClass, TemplateStrings.CustomClassInvalidFormat);
					}
				    if(CustomClass.Length > 255)
				    {
				        errors.ErrorFor(x => x.CustomClass, TemplateStrings.CustomClassMaxLengthExceeded);
				    }
				}

				if (!string.IsNullOrWhiteSpace(Folder))
				{
					if (!Regex.IsMatch(Folder, RegularExpressions.RelativeWindowsFolderPath))
					{
					    errors.ErrorFor(x => x.Folder, TemplateStrings.FolderNameInvalidFormat);
					}
				    if (Folder.Length > 255)
				    {
				        errors.ErrorFor(x => x.Folder, TemplateStrings.FolderNameMaxLengthExceeded);
				    }
				}
			}
			if (!errors.IsEmpty)
			{
			    throw errors;
			}
		}

		internal void MutatePage()
		{
			var name = Name;
			var index = 0;
			do
			{
				index++;
				Name = MutateHelper.MutateTitle(name, index);
			}
			while (PageRepository.NameExists(this));
					
			var fileName = FileName;

			index = 0;

			while (PageRepository.FileNameExists(this))
			{
				index++;
				FileName = MutateHelper.MutatePageFileName(FileName, index);
			}						
		}
	}
}
