using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL.ListItems;
using System.Data;
using System.Data.Objects;
using System.Web;
using Quantumart.QP8.BLL.Repository.Helpers;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Repository
{
	internal static class CustomActionRepository
	{
		#region List
		/// <summary>
		/// Возвращает список Custom Action по коду
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		internal static IEnumerable<CustomAction> GetListByCodes(IEnumerable<string> codes)
		{
			return BackendActionCache.CustomActions.Where(ca => codes.Contains(ca.Action.Code)).ToArray();
		}


		internal static IEnumerable<CustomActionListItem> List(ListCommand cmd, out int totalRecords)
		{
			using (var scope = new QPConnectionScope())
			{
                cmd.SortExpression = MappersRepository.CustomActionListItemRowMapper.TranslateSortExpression(cmd.SortExpression);
				IEnumerable<DataRow> rows = Common.GetCustomActionList(scope.DbConnection, cmd.SortExpression, cmd.StartRecord, cmd.PageSize, out totalRecords);
				var result = MappersRepository.CustomActionListItemRowMapper.GetBizList(rows.ToList());
				return result;
			}
		} 
		#endregion

		#region CRUD

		internal static CustomAction GetById(int id)
		{
			return BackendActionCache.CustomActions.SingleOrDefault(a => a.Id == id);
		}

		internal static CustomAction GetByCode(string code)
		{
			return BackendActionCache.CustomActions.SingleOrDefault(a => a.Action.Code == code);
		}

		internal static bool Exists(int id)
		{
			return QPContext.EFContext.CustomActionSet.Any(a => a.Id == id);
		}

		internal static CustomAction Update(CustomAction customAction)
		{
			var oldCustomAction = GetById(customAction.Id);

			QP8Entities entities = QPContext.EFContext;
			CustomActionDAL dal = MappersRepository.CustomActionMapper.GetDalObject(customAction);													
			dal.LastModifiedBy = QPContext.CurrentUserId;
			using (new QPConnectionScope())
			{
				dal.Modified = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
			}			

			entities.CustomActionSet.Attach(dal);
			entities.ObjectStateManager.ChangeObjectState(dal, EntityState.Modified);

			BackendActionDAL dal2 = MappersRepository.BackendActionMapper.GetDalObject(customAction.Action);
			entities.BackendActionSet.Attach(dal2);
			entities.ObjectStateManager.ChangeObjectState(dal2, EntityState.Modified);
						
			// Toolbar Buttons
			foreach (var t in entities.ToolbarButtonSet.Where(t => t.ActionId == customAction.Action.Id))
			{
				entities.ToolbarButtonSet.DeleteObject(t);
			}
			foreach (var t in MappersRepository.ToolbarButtonMapper.GetDalList(customAction.Action.ToolbarButtons.ToList()))
			{				
				entities.ToolbarButtonSet.AddObject(t);
			}


			ToolbarButtonDAL refreshBtnDal = CreateRefreshButton(dal.ActionId);
			if (!customAction.Action.IsInterface)
			{
				foreach (var t in entities.ToolbarButtonSet.Where(b => b.ParentActionId == dal.ActionId && b.ActionId == refreshBtnDal.ActionId))
				{
					entities.ToolbarButtonSet.DeleteObject(t);
				}
			}

			// Если действие интерфейсное, то создать для него кнопку Refresh
			if (customAction.Action.IsInterface && !entities.ToolbarButtonSet.Any(b => b.ParentActionId == dal.ActionId && b.ActionId == refreshBtnDal.ActionId))
			{
				entities.ToolbarButtonSet.AddObject(refreshBtnDal);
			}
			
			// Context Menu Items
			int? oldContextMenuId = null;
			foreach (var c in entities.ContextMenuItemSet.Where(c => c.ActionId == customAction.Action.Id))
			{
				oldContextMenuId = c.ContextMenuId;
				entities.ContextMenuItemSet.DeleteObject(c);
			}
			foreach (var c in MappersRepository.ContextMenuItemMapper.GetDalList(customAction.Action.ContextMenuItems.ToList()))
			{
				entities.ContextMenuItemSet.AddObject(c);
			}

			CustomActionDAL dalDB = entities.CustomActionSet
				.Include("Contents")
				.Include("Sites")
				.Single(a => a.Id == customAction.Id);						

			// Binded Sites
			HashSet<decimal> inmemorySiteIDs = new HashSet<decimal>(customAction.Sites.Select(bs => Converter.ToDecimal(bs.Id)));
			HashSet<decimal> indbSiteIDs = new HashSet<decimal>(dalDB.Sites.Select(bs => Converter.ToDecimal(bs.Id)));
			foreach (var s in dalDB.Sites.ToArray())
			{
				if (!inmemorySiteIDs.Contains(s.Id))
				{
					entities.SiteSet.Attach(s);
					dalDB.Sites.Remove(s);
				}
			}
			foreach (var s in MappersRepository.SiteMapper.GetDalList(customAction.Sites.ToList()))
			{
				if (!indbSiteIDs.Contains(s.Id))
				{
					entities.SiteSet.Attach(s);
					dal.Sites.Add(s);
				}
			}

			// Binded Contents
			HashSet<decimal> inmemoryContentIDs = new HashSet<decimal>(customAction.Contents.Select(bs => Converter.ToDecimal(bs.Id)));
			HashSet<decimal> indbContentIDs = new HashSet<decimal>(dalDB.Contents.Select(bs => Converter.ToDecimal(bs.Id)));
			foreach (var s in dalDB.Contents.ToArray())
			{
				if (!inmemoryContentIDs.Contains(s.Id))
				{
					entities.ContentSet.Attach(s);
					dalDB.Contents.Remove(s);
				}
			}
			foreach (var s in MappersRepository.ContentMapper.GetDalList(customAction.Contents.ToList()))
			{
				if (!indbContentIDs.Contains(s.Id))
				{
					entities.ContentSet.Attach(s);
					dal.Contents.Add(s);
				}
			}			

			entities.SaveChanges();

			if (oldContextMenuId != customAction.Action.EntityType.ContextMenu.Id)
				SetBottomSeparator(oldContextMenuId);
			SetBottomSeparator(customAction.Action.EntityType.ContextMenu.Id);

			CustomAction updated = MappersRepository.CustomActionMapper.GetBizObject(dal);
			BackendActionCache.Reset();
			return updated;				
		}

		internal static CustomAction Save(CustomAction customAction)
		{
			QP8Entities entities = QPContext.EFContext;

			BackendActionDAL actionDal = MappersRepository.BackendActionMapper.GetDalObject(customAction.Action);
			entities.BackendActionSet.AddObject(actionDal);

			EntityObject.VerifyIdentityInserting(EntityTypeCode.BackendAction, actionDal.Id, customAction.ForceActionId);

			if (customAction.ForceActionId != 0)
				actionDal.Id = customAction.ForceActionId;

			DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.BackendAction);
			entities.SaveChanges();
			DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.BackendAction);

			CustomActionDAL customActionDal = MappersRepository.CustomActionMapper.GetDalObject(customAction);
			customActionDal.LastModifiedBy = QPContext.CurrentUserId;
			customActionDal.Action = actionDal;
			using (new QPConnectionScope())
			{
				customActionDal.Created = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
				customActionDal.Modified = customActionDal.Created;				 
			}

			entities.CustomActionSet.AddObject(customActionDal);

			DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.CustomAction, customAction);
			entities.SaveChanges();
			DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.CustomAction);

			// Toolbar Button
			foreach (var t in MappersRepository.ToolbarButtonMapper.GetDalList(customAction.Action.ToolbarButtons.ToList()))
			{
				t.ActionId = customActionDal.Action.Id;
				entities.ToolbarButtonSet.AddObject(t);
			}
			// Context Menu Items			
			foreach (var c in MappersRepository.ContextMenuItemMapper.GetDalList(customAction.Action.ContextMenuItems.ToList()))
			{
				c.ActionId = customActionDal.Action.Id;
				entities.ContextMenuItemSet.AddObject(c);
			}
			// Binded Sites
			foreach (var s in MappersRepository.SiteMapper.GetDalList(customAction.Sites.ToList()))
			{				
				entities.SiteSet.Attach(s);
				customActionDal.Sites.Add(s);				
			}
			// Binded Contents
			foreach (var s in MappersRepository.ContentMapper.GetDalList(customAction.Contents.ToList()))
			{				
				entities.ContentSet.Attach(s);
				customActionDal.Contents.Add(s);				
			}	
		
			// Если действие интерфейсное, то создать для него кнопку Refresh
			if (customAction.Action.IsInterface)
			{
				ToolbarButtonDAL refreshBtnDal = CreateRefreshButton(customActionDal.ActionId);
				entities.ToolbarButtonSet.AddObject(refreshBtnDal);
			}


			entities.SaveChanges();

			int? contextMenuId = entities.EntityTypeSet.Single(t => t.Id == customAction.Action.EntityTypeId).ContextMenuId;
			SetBottomSeparator(contextMenuId);			

			CustomAction updated = MappersRepository.CustomActionMapper.GetBizObject(customActionDal);
			BackendActionCache.Reset();
			return updated;				
		}		

		internal static void Delete(int id)
		{			

			QP8Entities entities = QPContext.EFContext;

			CustomActionDAL dalDB = entities.CustomActionSet
				.Include("Action.ToolbarButtons")
				.Include("Action.ContextMenuItems")
				.Include("Action.EntityType")
				.Single(a => a.Id == id);

			int? contextMenuId = dalDB.Action.EntityType.ContextMenuId;

			// Toolbar buttons
			foreach (var t in dalDB.Action.ToolbarButtons.ToArray())
			{
				entities.ToolbarButtonSet.DeleteObject(t);
			}
			foreach (var t in entities.ToolbarButtonSet.Where(b => b.ParentActionId == dalDB.ActionId))
			{
				entities.ToolbarButtonSet.DeleteObject(t);
			}
			// Context Menu Items
			int? oldContextMenuId = null;
			foreach (var c in dalDB.Action.ContextMenuItems.ToArray())
			{
				oldContextMenuId = c.ContextMenuId;
				entities.ContextMenuItemSet.DeleteObject(c);
			}

			entities.BackendActionSet.DeleteObject(dalDB.Action);
			entities.CustomActionSet.DeleteObject(dalDB);						

			entities.SaveChanges();

			if (oldContextMenuId != contextMenuId)
				SetBottomSeparator(oldContextMenuId);
			SetBottomSeparator(contextMenuId);

			BackendActionCache.Reset();
		} 

		#endregion

		/// <summary>
		/// Получить все используемые значения Order для Custom Acction данного EntityType
		/// </summary>
		/// <param name="entityTypeId"></param>
		/// <returns></returns>
		internal static IEnumerable<int> GetActionOrdersForEntityType(int entityTypeId)
		{
			return QPContext.EFContext.CustomActionSet
				.Include("Action")
				.Where(c => c.Action.EntityTypeId == entityTypeId)
				.Select(c => c.Order)
				.Distinct()
				.OrderBy(o => o)
				.ToArray();
		}

		/// <summary>
		/// Получить все используемые значения Order для Custom Acction данного EntityType
		/// исключая указанный Custom Action
		/// </summary>
		/// <param name="entityTypeId"></param>
		/// <returns></returns>
		internal static bool IsOrderUniq(int exceptActionId, int order, int entityTypeId)
		{
			return !(QPContext.EFContext.CustomActionSet
						.Include("Action")
						.Where(c => c.Action.EntityTypeId == entityTypeId && c.Id != exceptActionId && c.Order == order)
						.Any()
					);
		}

		private static void SetBottomSeparator(int? contextMenuId)
		{
			if(contextMenuId.HasValue)
			{
				QP8Entities entities = QPContext.EFContext;

				ContextMenuItemDAL maxOrderNotCustomActionMenuItem = entities.ContextMenuItemSet
					.Where(i => i.ContextMenu.Id == contextMenuId.Value && !i.Action.IsCustom)
					.OrderByDescending(i => i.Order)
					.FirstOrDefault();
				if (maxOrderNotCustomActionMenuItem != null)
				{
					bool customActionMenuItemExist = entities.ContextMenuItemSet
							.Where(i => i.ContextMenu.Id == contextMenuId.Value && i.Action.IsCustom)
							.Any();

					if (maxOrderNotCustomActionMenuItem.HasBottomSeparator != customActionMenuItemExist)
					{
						maxOrderNotCustomActionMenuItem.HasBottomSeparator = customActionMenuItemExist;
						entities.SaveChanges();
					}
				}
			}
		}

		private static ToolbarButtonDAL CreateRefreshButton(int actionId)
		{
			BackendAction refreshAction = BackendActionRepository.GetByCode(ActionCode.RefreshCustomAction);
			if (refreshAction == null)
				throw new ApplicationException(String.Format("Action is not found: {0}", ActionCode.RefreshCustomAction));
			ToolbarButtonDAL refreshBtnDal = new ToolbarButtonDAL
			{
				ParentActionId = actionId,
				ActionId = refreshAction.Id,
				Name = "Refresh",
				Icon = "refresh.gif",
				IsCommand = true,
				Order = 1
			};
			return refreshBtnDal;
		}
	}
}
