using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
    public abstract class LockableEntityObject : EntityObject
	{
		/// <summary>
        /// информация о пользователе, который заблокировал сущность
        /// </summary>
		public virtual User LockedByUser
		{
			get;
			set;
		}

        /// <summary>
        /// дата блокировки сущности пользователем
        /// </summary>

        public DateTime Locked
        {
            get;
            set;
        }
        [LocalizedDisplayName("Locked", NameResourceType = typeof(GlobalStrings))]
        public string LockedToDisplay
        {
            get
            {
                return Locked.ValueToDisplay();
            }
        }


        /// <summary>
        /// идентификатор пользователя, который заблокировал сущность
        /// </summary>
        public int LockedBy
        {
            get;
            set;
        }

        [LocalizedDisplayName("LockedByUser", NameResourceType = typeof(GlobalStrings))]
        public string LockedByDisplayName
        {
            get
            {
                return (LockedByUser != null) ? LockedByUser.DisplayName : String.Empty;
            }
        }

        public bool LockedByYou
        {
            get
            {
                return IsLockedByYou(LockedBy);
            }
        }

        public bool LockedByAnyone
        {
            get
            {
                return IsLockedByAnyone(LockedBy);
            }
        }

        public bool LockedByAnyoneElse
        {
            get
            {
                return LockedByAnyone && !LockedByYou;
            }
        }

        /// <summary>
        /// является ли блокировка постоянной
        /// </summary>
        [LocalizedDisplayName("PermanentLock", NameResourceType = typeof(GlobalStrings))]
        public bool PermanentLock
        {
            get;
            set;
        }
        /// <summary>
        /// Пытается заблокировать сущность от имени текущего пользователя
        /// </summary>
        /// <returns>false, если сущность заблокирована кем-то еще</returns>
        public void AutoLock()
        {
            if (!LockedByAnyone)
            {
				LockedBy = QPContext.CurrentUserId;
                Locked = (DateTime)EntityObjectRepository.Lock(this);
            }
        }

        public void AutoUnlock()
        {
            if (LockedByYou && !PermanentLock)
            {
                EntityObjectRepository.UnLock(this);
            }
        }

        public string LockedByIcon
        {
            get
            {
                return (LockedByAnyone) ? ((LockedByYou) ? "locked.gif" : "locked_by_user.gif") : "0.gif";
            }
        }

        public static string GetLockedByIcon(int lockedBy)
        {
            return (IsLockedByAnyone(lockedBy)) ? ((IsLockedByYou(lockedBy)) ? "locked.gif" : "locked_by_user.gif") : "0.gif";
        }

        public static bool IsLockedByAnyone(int lockedBy)
        {
            return lockedBy != 0;
        }

        public static bool IsLockedByYou(int lockedBy)
        {
            return lockedBy == QPContext.CurrentUserId;
        }

        public static string GetLockedByToolTip(int lockedBy, string displayName)
        {
                if (IsLockedByAnyone(lockedBy))
                    if (IsLockedByYou(lockedBy))
                        return SiteStrings.Tooltip_LockedByYou;
                    else
                        return String.Format(SiteStrings.Tooltip_LockedByUser, displayName);
                else
                    return String.Empty;
        }
        public string LockedByToolTip
        {
            get
            {
                return GetLockedByToolTip(LockedBy, LockedByDisplayName);
            }   
        }

        public bool CanBeUnlocked
        {
            get
            {
				return LockedByAnyoneElse && QPContext.CanUnlockItems;
            }
        }

        public abstract string LockedByAnyoneElseMessage { get; }

        protected override void Validate(RulesException errors)
        {
            base.Validate(errors);

            if (LockedByAnyoneElse)
                errors.CriticalErrorForModel(String.Format(LockedByAnyoneElseMessage, LockedByDisplayName));
        }

		public void LoadLockedByUser()
		{
			LockedByUser = UserRepository.GetById(LockedBy, true);
		}

    }
}
