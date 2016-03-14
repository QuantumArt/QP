using System;
using System.Collections.Generic;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Factories;
using System.IO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Constants;


namespace Quantumart.QP8.BLL
{
	public class SiteFolder : Folder
    {

        #region overrides

        public override int ParentEntityId
        {
            get { return SiteId; }
            set { SiteId = value; }
        }		

		protected override EntityObject GetParent()
        {
            return SiteRepository.GetById(ParentEntityId);
        }

		protected override FolderFactory GetFactory()
		{
			return new SiteFolderFactory();
		}

		public override string EntityTypeCode
		{
			get
			{
				return Constants.EntityTypeCode.SiteFolder;
			}
		}

		protected override void ValidateSecurity(RulesException errors)
		{
			if (!IsNew)
				base.ValidateSecurity(errors);
			else if (RecurringId.HasValue)
			{
				if (!SecurityRepository.IsEntityAccessible(EntityTypeCode, RecurringId.Value, ActionTypeCode.Update))
					errors.CriticalErrorForModel(CannotAddBecauseOfSecurityMessage);
			}
			else
				base.ValidateSecurity(errors);

		}

		#endregion

        /// <summary>
		/// идентификатор сайта
		/// </summary>
		public int SiteId { get; set; }

		public Site Site
		{
            get
            {
                return (Site)Parent;
            }
		}

		public static PathInfo GetPathInfo(int id)
		{
			PathInfo info = Folder.GetPathInfo(new SiteFolderFactory(), id);
			if (info == null)
				throw new Exception(String.Format(LibraryStrings.SiteFolderNotExists, id));
			return info;
		}

    }
}
