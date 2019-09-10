using System;
using System.ComponentModel.DataAnnotations;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
    public abstract class LockableEntityObject : EntityObject
    {
        public virtual User LockedByUser { get; set; }

        public DateTime Locked { get; set; }

        [Display(Name = "Locked", ResourceType = typeof(GlobalStrings))]
        public string LockedToDisplay => Locked.ValueToDisplay();

        public int LockedBy { get; set; }

        [Display(Name = "LockedByUser", ResourceType = typeof(GlobalStrings))]
        public string LockedByDisplayName => LockedByUser != null ? LockedByUser.DisplayName : string.Empty;

        public bool LockedByYou => IsLockedByYou(LockedBy);

        public bool LockedByAnyone => IsLockedByAnyone(LockedBy);

        public bool LockedByAnyoneElse => LockedByAnyone && !LockedByYou;

        [Display(Name = "PermanentLock", ResourceType = typeof(GlobalStrings))]
        public bool PermanentLock { get; set; }

        public void AutoLock()
        {
            if (!LockedByAnyone)
            {
                LockedBy = QPContext.CurrentUserId;

                // ReSharper disable once PossibleInvalidOperationException
                Locked = EntityObjectRepository.Lock(this).Value;
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

        public static string GetLockedByIcon(int lockedBy) => IsLockedByAnyone(lockedBy) ? (IsLockedByYou(lockedBy) ? "locked.gif" : "locked_by_user.gif") : "0.gif";

        public static bool IsLockedByAnyone(int lockedBy) => lockedBy != 0;

        public static bool IsLockedByYou(int lockedBy) => lockedBy == QPContext.CurrentUserId;

        public static string GetLockedByToolTip(int lockedBy, string displayName)
        {
            if (IsLockedByAnyone(lockedBy))
            {
                return IsLockedByYou(lockedBy) ? SiteStrings.Tooltip_LockedByYou : string.Format(SiteStrings.Tooltip_LockedByUser, displayName);
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
