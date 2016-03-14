using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.DAL;
using System.Web;
using Quantumart.QP8.BLL.Mappers;

namespace Quantumart.QP8.BLL.Repository.Helpers
{
	internal static class BackendActionCache
	{
		private static readonly string ACTION_CACHE_KEY = "BackendActionCache.BackendActions";
		private static readonly string CUSTOM_ACTION_CACHE_KEY = "BackendActionCache.CustomBackendActions";

		private static IEnumerable<BackendAction> _Actions;
		private static IEnumerable<CustomAction> _CustomActions;


		public static IEnumerable<BackendAction> Actions 
		{ 
			get
			{
				if (HttpContext.Current == null || HttpContext.Current.Session == null)
				{
					if (_Actions == null)
					{
						_Actions = LoadActions();
					}
					return _Actions;
				}
				else
				{
					if (HttpContext.Current.Session[ACTION_CACHE_KEY] == null)
					{
						HttpContext.Current.Session[ACTION_CACHE_KEY] = LoadActions();
					}
					return HttpContext.Current.Session[ACTION_CACHE_KEY] as IEnumerable<BackendAction>;
				}
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
					if (_CustomActions == null)
					{
						_CustomActions = LoadCustomActions();
					}
					return _CustomActions;
				}
				else 
				{

					if (HttpContext.Current.Session[CUSTOM_ACTION_CACHE_KEY] == null)
					{
						HttpContext.Current.Session[CUSTOM_ACTION_CACHE_KEY] = LoadCustomActions();
					}
					return HttpContext.Current.Session[CUSTOM_ACTION_CACHE_KEY] as IEnumerable<CustomAction>;
				}
			}
		}

		public static void Reset()
		{
			if (HttpContext.Current == null || HttpContext.Current.Session == null)
			{
				_Actions = null;
				_CustomActions = null;
			}
			else
			{
				HttpContext.Current.Session.Remove(ACTION_CACHE_KEY);
				HttpContext.Current.Session.Remove(CUSTOM_ACTION_CACHE_KEY);
			}

		}
	}
}
