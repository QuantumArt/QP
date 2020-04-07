using System.ComponentModel.DataAnnotations;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL
{
    public class ContentGroup : EntityObject
    {
        private Site _site;

        public static readonly int MaxNameLength = 255;

        public static readonly string DefaultName = "Default Group";

        private static string _translatedDefaultName;

        public static string TranslatedDefaultName => _translatedDefaultName ?? (_translatedDefaultName = Translator.Translate(DefaultName));

        public ContentGroup()
        {
        }

        public ContentGroup(int siteId)
        {
            SiteId = siteId;
        }

        public static ContentGroup GetDefaultGroup(int siteId) => ContentRepository.GetGroupById(ContentRepository.GetDefaultGroupId(siteId));

        [Required(ErrorMessageResourceName = "GroupNameNotEntered", ErrorMessageResourceType = typeof(ContentStrings))]
        [StringLength(255, ErrorMessageResourceName = "GroupNameMaxLengthExceeded", ErrorMessageResourceType = typeof(ContentStrings))]
        [RegularExpression(RegularExpressions.EntityName, ErrorMessageResourceName = "GroupNameInvalidFormat", ErrorMessageResourceType = typeof(ContentStrings))]
        [Display(Name = "GroupName", ResourceType = typeof(ContentStrings))]
        public override string Name { get; set; }

        public int SiteId { get; set; }

        public override int ParentEntityId => SiteId;

        public override string EntityTypeCode => Constants.EntityTypeCode.ContentGroup;

        public override string CannotAddBecauseOfSecurityMessage => ContentStrings.CannotAddGroupBecauseOfSecurity;

        public override string CannotUpdateBecauseOfSecurityMessage => ContentStrings.CannotUpdateGroupBecauseOfSecurity;

        public override string PropertyIsNotUniqueMessage => ContentStrings.GroupNameNonUnique;

        public bool IsDefault => Id == ContentRepository.GetDefaultGroupId(SiteId);

        public string OutputName => !IsDefault ? Name : Translator.Translate(Name);

        public Site Site => _site ?? (_site = SiteRepository.GetById(SiteId));

        public override EntityObject Parent => Site;

        public override void Validate()
        {
            var errors = new RulesException<ContentGroup>();
            if (IsDefault)
            {
                errors.ErrorForModel(ContentStrings.CannotUpdateDefaultGroup);
            }
            else
            {
                base.Validate(errors);
            }

            if (!errors.IsEmpty)
            {
                throw errors;
            }
        }
    }
}
