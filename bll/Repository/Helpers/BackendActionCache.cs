using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL.Mappers;

namespace Quantumart.QP8.BLL.Repository.Helpers
{
    internal static class BackendActionCache
    {
        private const string ActionCacheKey = "BackendActionCache.BackendActions";
        private const string CustomActionCacheKey = "BackendActionCache.CustomBackendActions";

        private static IEnumerable<BackendAction> _actions;
        private static IEnumerable<CustomAction> _customActions;

        public static IEnumerable<BackendAction> Actions
        {
            get
            {
                if (HttpContext.Current == null || HttpContext.Current.Session == null)
                {
                    return _actions ?? (_actions = LoadActions());
                }

                if (HttpContext.Current.Session[ActionCacheKey] == null)
                {
                    HttpContext.Current.Session[ActionCacheKey] = LoadActions();
                }

                return HttpContext.Current.Session[ActionCacheKey] as IEnumerable<BackendAction>;
            }
        }

        private static IEnumerable<BackendAction> LoadActions()
        {
            return MappersRepository.BackendActionMapper.GetBizList(
                QPContext.EFContext.BackendActionSet
                    .Include("EntityType")
                    .Include("EntityType.Parent")
                    .Include("EntityType.CancelAction")
                    .Include("ActionType.PermissionLevel")
                    .Include("DefaultViewType")
                    .Include("Views.ViewType")
                    .Include("NextSuccessfulAction")
                    .Include("NextFailedAction")
                    .Include("Excludes")
                    .ToList()
            );
        }

        private static IEnumerable<CustomAction> LoadCustomActions()
        {
            return MappersRepository.CustomActionMapper.GetBizList(
                QPContext.EFContext.CustomActionSet
                    .Include("Action.EntityType.ContextMenu")
                    .Include("Action.ToolbarButtons")
                    .Include("Action.ContextMenuItems")
                    .Include("Action.ActionType.PermissionLevel")
                    .Include("Action.Excludes")
                    .Include("Contents.Site")
                    .Include("Sites")
                    .ToList()
            );
        }

        public static IEnumerable<CustomAction> CustomActions
        {
            get
            {
                if (HttpContext.Current == null || HttpContext.Current.Session == null)
                {
                    return _customActions ?? (_customActions = LoadCustomActions());
                }

                if (HttpContext.Current.Session[CustomActionCacheKey] == null)
                {
                    HttpContext.Current.Session[CustomActionCacheKey] = LoadCustomActions();
                }

                return HttpContext.Current.Session[CustomActionCacheKey] as IEnumerable<CustomAction>;
            }
        }

        public static void Reset()
        {
            if (HttpContext.Current == null || HttpContext.Current.Session == null)
            {
                _actions = null;
                _customActions = null;
            }
            else
            {
                HttpContext.Current.Session.Remove(ActionCacheKey);
                HttpContext.Current.Session.Remove(CustomActionCacheKey);
            }
        }
    }
}
