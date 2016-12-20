using System;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{
    public abstract class LockableEntityObject : EntityObject
    {
        public virtual User LockedByUser
        {
            get;
            set;
        }

        public DateTime Locked
        {
            get;
            set;
        }

        [LocalizedDisplayName("Locked", NameResourceType = typeof(GlobalStrings))]
        public string LockedToDisplay => Locked.ValueToDisplay();

        public int LockedBy
        {
            get;
            set;
        }

        [LocalizedDisplayName("LockedByUser", NameResourceType = typeof(GlobalStrings))]
        public string LockedByDisplayName => LockedByUser != null ? LockedByUser.DisplayName : string.Empty;

        public bool LockedByYou => IsLockedByYou(LockedBy);

        public bool LockedByAnyone => IsLockedByAnyone(LockedBy);

        public bool LockedByAnyoneElse => LockedByAnyone && !LockedByYou;

        [LocalizedDisplayName("PermanentLock", NameResourceType = typeof(GlobalStrings))]
        public bool PermanentLock
        {
            get;
            set;
        }

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

        public string LockedByIcon => LockedByAnyone ? (LockedByYou ? "locked.gif" : "locked_by_user.gif") : "0.gif";

        public static string GetLockedByIcon(int lockedBy)
        {
            return IsLockedByAnyone(lockedBy) ? (IsLockedByYou(lockedBy) ? "locked.gif" : "locked_by_user.gif") : "0.gif";
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
            {
                if (IsLockedByYou(lockedBy))
                {
                    return SiteStrings.Tooltip_LockedByYou;
                }

                return string.Format(SiteStrings.Tooltip_LockedByUser, displayName);
            }

            return string.Empty;
        }

        public string LockedByToolTip => GetLockedByToolTip(LockedBy, LockedByDisplayName);

        public bool CanBeUnlocked => LockedByAnyoneElse && QPContext.CanUnlockItems;

        public abstract string LockedByAnyoneElseMessage { get; }

        protected override RulesException Validate(RulesException errors)
        {
            base.Validate(errors);
            if (LockedByAnyoneElse)
            {
                errors.CriticalErrorForModel(string.Format(LockedByAnyoneElseMessage, LockedByDisplayName));
            }

            return errors;
        }

        public void LoadLockedByUser()
        {
            LockedByUser = UserRepository.GetById(LockedBy, true);
        }
    }
}
