using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;


namespace Quantumart.QP8.WebMvc.ViewModels
{
    public abstract class LockableEntityViewModel : EntityViewModel
    {
        public new LockableEntityObject Data
        {
            get { return (LockableEntityObject)EntityData;  }
			set { EntityData = value; }
        }

		#region read-only members
		public string UnlockId
		{
			get
			{
				return UniqueId("unlock");
			}
		}

		public string UnlockText
		{
			get
			{
				return GlobalStrings.Unlock;
			}
		}

		public virtual string CaptureLockActionCode
		{
			get
			{
				return String.Empty;
			}
		}
		#endregion
		
	}
}