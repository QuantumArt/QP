using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{
    public class ContentGroup : EntityObject
    {
        #region private fields

        private Site _Site;

        #endregion

        #region constants

        public static readonly int MaxNameLength = 255;

        public static readonly string DefaultName = "Default Group";

        private static string _TranslatedDefaultName;

        public static string TranslatedDefaultName
        {
            get
            {
                if (_TranslatedDefaultName == null)
                    _TranslatedDefaultName = Translator.Translate(DefaultName);
                return _TranslatedDefaultName;
            }
        }



        #endregion

        #region creation

        public ContentGroup()
        {

        }

        public ContentGroup(int siteId)
        {
            SiteId = siteId;
        }

        public static ContentGroup GetDefaultGroup(int siteId)
        {
            return ContentRepository.GetGroupById(ContentRepository.GetDefaultGroupId(siteId));
        }

        #endregion

        #region properties

        #region simple read-write

        [RequiredValidator(MessageTemplateResourceName = "GroupNameNotEntered", MessageTemplateResourceType = typeof(ContentStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "GroupNameMaxLengthExceeded", MessageTemplateResourceType = typeof(ContentStrings))]
        [FormatValidator(Constants.RegularExpressions.InvalidEntityName, Negated = true, MessageTemplateResourceName = "GroupNameInvalidFormat", MessageTemplateResourceType = typeof(ContentStrings))]
        [LocalizedDisplayName("GroupName", NameResourceType = typeof(ContentStrings))]
        public override string Name { get; set; }

        public int SiteId { get; set; }

        #endregion

        #region simple read-only

        public override int ParentEntityId => SiteId;

        public override string EntityTypeCode => Constants.EntityTypeCode.ContentGroup;

        public override string CannotAddBecauseOfSecurityMessage => ContentStrings.CannotAddGroupBecauseOfSecurity;

        public override string CannotUpdateBecauseOfSecurityMessage => ContentStrings.CannotUpdateGroupBecauseOfSecurity;

        public override string PropertyIsNotUniqueMessage => ContentStrings.GroupNameNonUnique;

        public bool IsDefault => Id == ContentRepository.GetDefaultGroupId(SiteId);

        public string OutputName => (!IsDefault) ? Name : Translator.Translate(Name);
        #endregion

        #region references

        public Site Site
        {
            get
            {
                if (_Site == null)
                {
                    _Site = SiteRepository.GetById(SiteId);
                }
                return _Site;
            }
        }

        public override EntityObject Parent => Site;
        #endregion

        #endregion

        #region methods

        public override void Validate()
        {
            RulesException<ContentGroup> errors = new RulesException<ContentGroup>();

            if (IsDefault)
            {
                errors.ErrorForModel(ContentStrings.CannotUpdateDefaultGroup);
            }
            else
            {
                base.Validate(errors);
            }
            if (!errors.IsEmpty)
                throw errors;
        }

        #endregion
    }
}
