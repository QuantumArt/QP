using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.BLL.Facades;

namespace Quantumart.QP8.BLL.Repository.Helpers
{
    public static class BackendActionCache
    {
        private static readonly object Locker = new object();

        private static readonly Dictionary<string, List<BackendAction>> ActionCache
            = new Dictionary<string, List<BackendAction>>();
        private static readonly Dictionary<string, List<CustomAction>> CustomActionCache
            = new Dictionary<string, List<CustomAction>>();

        private static string CurrentKey => $"{QPContext.CurrentCustomerCode}__{QPContext.CurrentUserId}";

        private static string CurrentKeyPrefix => $"{QPContext.CurrentCustomerCode}__";


        public static List<BackendAction> Actions
        {
            get
            {
                if (!ActionCache.TryGetValue(CurrentKey, out var actions))
                {
                    lock (Locker)
                    {
                        if (!ActionCache.TryGetValue(CurrentKey, out actions))
                        {
                            actions = LoadActions();
                            ActionCache[CurrentKey] = actions;
                        }
                    }
                }
                return actions;
            }
        }

        public static List<CustomAction> CustomActions
        {
            get
            {
                if (!CustomActionCache.TryGetValue(CurrentKey, out var actions))
                {
                    var backendActions = Actions;
                    lock (Locker)
                    {
                        if (!CustomActionCache.TryGetValue(CurrentKey, out actions))
                        {
                            actions = LoadCustomActions(backendActions);
                            CustomActionCache[CurrentKey] = actions;
                        }
                    }
                }
                return actions;
            }
        }

        private static List<BackendAction> LoadActions() => MapperFacade.BackendActionMapper.GetBizList(
            QPContext.EFContext.BackendActionSet
                .Include(x => x.EntityType)
                .Include(x => x.EntityType.Parent)
                .Include(x => x.EntityType.CancelAction)
                .Include(x => x.ActionType.PermissionLevel)
                .Include(x => x.DefaultViewType)
                .Include(x => x.Views).ThenInclude( y => y.ViewType)
                .Include(x => x.NextSuccessfulAction)
                .Include(x => x.NextFailedAction)
                .Include(x => x.ExcludesBinds).ThenInclude(x => x.Excludes)
                .ToList()
        );

        private static List<CustomAction> LoadCustomActions(List<BackendAction> backendActions)
        {
            var customActions = MapperFacade.CustomActionMapper.GetBizList(
                QPContext.EFContext.CustomActionSet
                    .Include(b => b.ContentCustomActionBinds)
                    .Include(b => b.SiteCustomActionBinds)
                    .ToList()
            );

            foreach (var c in customActions)
            {
                c.Action = backendActions.FirstOrDefault(n => n.Id == c.ActionId);
            }

            return customActions;
        }

        public static void ResetForUser()
        {
            lock (Locker)
            {
                ActionCache.Remove(CurrentKey);
                CustomActionCache.Remove(CurrentKey);
            }
        }

        public static void ResetForCustomerCode()
        {
            lock (Locker)
            {
                var keysToRemove = ActionCache.Keys.Where(n => n.StartsWith(CurrentKeyPrefix)).ToArray();
                foreach (var key in keysToRemove)
                {
                    ActionCache.Remove(key);
                }
                var keysToRemove2 = CustomActionCache.Keys.Where(n => n.StartsWith(CurrentKeyPrefix)).ToArray();
                foreach (var key in keysToRemove2)
                {
                    CustomActionCache.Remove(key);
                }
            }
        }
    }
}
