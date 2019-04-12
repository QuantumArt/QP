#if !NET_STANDARD
using System.Web;
#endif

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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
#if NET_STANDARD
                return _actions ?? (_actions = LoadActions());
#else
                if (HttpContext.Current == null || HttpContext.Current.Session == null)
                {
                    return _actions ?? (_actions = LoadActions());
                }

                if (HttpContext.Current.Session[HttpContextSession.BackendActionCache] == null)
                {
                    HttpContext.Current.Session[HttpContextSession.BackendActionCache] = LoadActions();
                }

                return HttpContext.Current.Session[HttpContextSession.BackendActionCache] as IEnumerable<BackendAction>;
#endif
            }
        }

        private static IEnumerable<BackendAction> LoadActions() => MapperFacade.BackendActionMapper.GetBizList(
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

        private static IEnumerable<CustomAction> LoadCustomActions() => MapperFacade.CustomActionMapper.GetBizList(
            QPContext.EFContext.CustomActionSet
                .Include(b => b.Action.EntityType.ContextMenu)
                .Include(b => b.Action.ToolbarButtons)
                .Include(b => b.Action.ContextMenuItems)
                .Include(b => b.Action.ActionType.PermissionLevel)
                .Include(b => b.Action.ExcludesBinds).ThenInclude(x => x.Excludes)
                .Include(b => b.ContentCustomActionBinds).ThenInclude(x => x.Content)
                .Include(b => b.SiteCustomActionBinds).ThenInclude(x => x.Site)
                .ToList()
        );

        public static IEnumerable<CustomAction> CustomActions
        {
            get
            {
#if NET_STANDARD
                return _customActions ?? (_customActions = LoadCustomActions());
#else
                if (HttpContext.Current == null || HttpContext.Current.Session == null)
                {
                    return _customActions ?? (_customActions = LoadCustomActions());
                }

                if (HttpContext.Current.Session[HttpContextSession.BackendCustomActionCache] == null)
                {
                    HttpContext.Current.Session[HttpContextSession.BackendCustomActionCache] = LoadCustomActions();
                }

                return HttpContext.Current.Session[HttpContextSession.BackendCustomActionCache] as IEnumerable<CustomAction>;
#endif
            }
        }

        public static void Reset()
        {
#if NET_STANDARD
            _actions = null;
            _customActions = null;
#else
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
#endif
        }
    }
}
