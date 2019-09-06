using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL
{
    public class Page : LockableEntityObject
    {
        [Required(ErrorMessageResourceName = "FileNameRequired", ErrorMessageResourceType = typeof(TemplateStrings))]
        [StringLength(255, ErrorMessageResourceName = "FileNameMaxLengthExceeded", ErrorMessageResourceType = typeof(TemplateStrings))]
        [RegularExpression(RegularExpressions.FileName, ErrorMessageResourceName = "FileNameInvalidFormat", ErrorMessageResourceType = typeof(TemplateStrings))]
        [Display(Name = "FileName", ResourceType = typeof(TemplateStrings))]
        public string FileName { get; set; }

        [StringLength(255, ErrorMessageResourceName = "CustomClassMaxLengthExceeded", ErrorMessageResourceType = typeof(TemplateStrings))]
        [Display(Name = "CustomClass", ResourceType = typeof(TemplateStrings))]
        public string CustomClass { get; set; }

        [Display(Name = "FolderName", ResourceType = typeof(TemplateStrings))]
        public string Folder { get; set; }

        [Display(Name = "GenerateTrace", ResourceType = typeof(TemplateStrings))]
        public bool GenerateTrace { get; set; }

        [Display(Name = "EnableViewState", ResourceType = typeof(TemplateStrings))]
        public bool EnableViewState { get; set; }

        [Display(Name = "SendNoCacheHeader", ResourceType = typeof(TemplateStrings))]
        public bool SendNocacheHeaders { get; set; }

        [Display(Name = "SendLastModifiedHeader", ResourceType = typeof(TemplateStrings))]
        public bool SendLastModifiedHeader { get; set; }

        [Display(Name = "ProxyCaching", ResourceType = typeof(TemplateStrings))]
        public bool ProxyCache { get; set; }

        [Display(Name = "Charset", ResourceType = typeof(TemplateStrings))]
        public string Charset { get; set; }

        [Display(Name = "Locale", ResourceType = typeof(TemplateStrings))]
        public int Locale { get; set; }

        public int LastAssembledBy { get; set; }

        public int TemplateId { get; set; }

        public DateTime Assembled { get; set; }

        [Display(Name = "ExpiresIn", ResourceType = typeof(TemplateStrings))]
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

        [Display(Name = "ApplyToExistingObjects", ResourceType = typeof(TemplateStrings))]
        public bool ApplyToExistingObjects { get; set; }

        public PageTemplate PageTemplate { get; set; }

        [Display(Name = "BrowserCaching", ResourceType = typeof(TemplateStrings))]
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
                    if (CustomClass.Length > 255)
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
            } while (PageRepository.NameExists(this));

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
