using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.Constants.Mvc;

namespace Quantumart.QP8.BLL.Repository.Helpers
{
    internal static class BackendActionCache
    {
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

                if (HttpContext.Current.Session[HttpContextSession.BackendActionCache] == null)
                {
                    HttpContext.Current.Session[HttpContextSession.BackendActionCache] = LoadActions();
                }

                return HttpContext.Current.Session[HttpContextSession.BackendActionCache] as IEnumerable<BackendAction>;
            }
        }

        private static IEnumerable<BackendAction> LoadActions()
        {
            return MapperFacade.BackendActionMapper.GetBizList(
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
            return MapperFacade.CustomActionMapper.GetBizList(
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

                if (HttpContext.Current.Session[HttpContextSession.BackendCustomActionCache] == null)
                {
                    HttpContext.Current.Session[HttpContextSession.BackendCustomActionCache] = LoadCustomActions();
                }

                return HttpContext.Current.Session[HttpContextSession.BackendCustomActionCache] as IEnumerable<CustomAction>;
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
                HttpContext.Current.Session.Remove(HttpContextSession.BackendActionCache);
                HttpContext.Current.Session.Remove(HttpContextSession.BackendCustomActionCache);
            }
        }
    }
}
