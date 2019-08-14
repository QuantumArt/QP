using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QP8.Infrastructure.Web.Extensions;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.Constants.Mvc;

namespace Quantumart.QP8.BLL.Repository.Helpers
{
    internal static class BackendActionCache
    {
        private static HttpContext HttpContext => new HttpContextAccessor().HttpContext;

        private static IEnumerable<BackendAction> _actions;
        private static IEnumerable<CustomAction> _customActions;

        public static IEnumerable<BackendAction> Actions
        {
            get
            {
                if (HttpContext == null || HttpContext.Session == null)
                {
                    return _actions ?? (_actions = LoadActions());
                }

                if (!HttpContext.Session.HasKey(HttpContextSession.BackendActionCache))
                {
                    HttpContext.Session.SetValue(HttpContextSession.BackendActionCache, LoadActions());
                }

                return HttpContext.Session.GetValue<IEnumerable<BackendAction>>(HttpContextSession.BackendActionCache);
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
                if (HttpContext == null || HttpContext.Session == null)
                {
                    return _customActions ?? (_customActions = LoadCustomActions());
                }

                if (!HttpContext.Session.HasKey(HttpContextSession.BackendCustomActionCache))
                {
                    HttpContext.Session.SetValue(HttpContextSession.BackendCustomActionCache, LoadCustomActions());
                }

                return HttpContext.Session.GetValue<IEnumerable<CustomAction>>(HttpContextSession.BackendCustomActionCache);
            }
        }

        public static void Reset()
        {
            if (HttpContext == null || HttpContext.Session == null)
            {
                _actions = null;
                _customActions = null;
            }
            else
            {
                HttpContext.Session.Remove(HttpContextSession.BackendActionCache);
                HttpContext.Session.Remove(HttpContextSession.BackendCustomActionCache);
            }
        }
    }
}
