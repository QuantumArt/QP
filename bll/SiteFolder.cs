using System;
using Quantumart.QP8.BLL.Factories.FolderFactory;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL
{
    public class SiteFolder : Folder
    {
        public override int ParentEntityId
        {
            get => SiteId;
            set => SiteId = value;
        }

        protected override EntityObject GetParent() => SiteRepository.GetById(ParentEntityId);

        protected override FolderFactory GetFactory() => new SiteFolderFactory();

        public override string EntityTypeCode => Constants.EntityTypeCode.SiteFolder;

        protected override RulesException ValidateSecurity(RulesException errors)
        {
            if (!IsNew)
            {
                base.ValidateSecurity(errors);
            }
            else if (RecurringId.HasValue)
            {
                if (!SecurityRepository.IsEntityAccessible(EntityTypeCode, RecurringId.Value, ActionTypeCode.Update))
                {
                    errors.CriticalErrorForModel(CannotAddBecauseOfSecurityMessage);
                }
            }
            else
            {
                base.ValidateSecurity(errors);
            }

            return errors;
        }

        public int SiteId { get; set; }

        public Site Site => (Site)Parent;

        public static PathInfo GetPathInfo(int id)
        {
            var info = GetPathInfo(new SiteFolderFactory(), id);
            if (info == null)
            {
                throw new Exception(string.Format(LibraryStrings.SiteFolderNotExists, id));
            }

            return info;
        }
    }
}
