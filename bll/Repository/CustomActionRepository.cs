using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository.Helpers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Repository
{
    internal static class CustomActionRepository
    {
        internal static IEnumerable<CustomAction> GetListByCodes(IEnumerable<string> codes)
        {
            return BackendActionCache.CustomActions.Where(ca => codes.Contains(ca.Action.Code)).ToArray();
        }

        internal static IEnumerable<CustomActionListItem> List(ListCommand cmd, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                cmd.SortExpression = MapperFacade.CustomActionListItemRowMapper.TranslateSortExpression(cmd.SortExpression);
                var rows = Common.GetCustomActionList(scope.DbConnection, cmd.SortExpression, cmd.StartRecord, cmd.PageSize, out totalRecords);
                var result = MapperFacade.CustomActionListItemRowMapper.GetBizList(rows.ToList());
                return result;
            }
        }

        internal static CustomAction GetById(int id)
        {
            var result = BackendActionCache.CustomActions.SingleOrDefault(a => a.Id == id);
            result.LastModifiedByUser = UserRepository.GetById(result.LastModifiedBy, true);
            return result;
        }

        internal static CustomAction GetByCode(string code)
        {
            var result = BackendActionCache.CustomActions.SingleOrDefault(a => a.Action.Code == code);
            result.LastModifiedByUser = UserRepository.GetById(result.LastModifiedBy, true);
            return result;
        }

        internal static bool Exists(int id)
        {
            return QPContext.EFContext.CustomActionSet.Any(a => a.Id == id);
        }

        internal static CustomAction Update(CustomAction customAction)
        {
            GetById(customAction.Id);
            var entities = QPContext.EFContext;
            var dal = MapperFacade.CustomActionMapper.GetDalObject(customAction);
            dal.LastModifiedBy = QPContext.CurrentUserId;
            using (new QPConnectionScope())
            {
                dal.Modified = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
            }

            entities.CustomActionSet.Attach(dal);
            entities.ObjectStateManager.ChangeObjectState(dal, EntityState.Modified);

            var dal2 = MapperFacade.BackendActionMapper.GetDalObject(customAction.Action);
            entities.BackendActionSet.Attach(dal2);
            entities.ObjectStateManager.ChangeObjectState(dal2, EntityState.Modified);

            // Toolbar Buttons
            foreach (var t in entities.ToolbarButtonSet.Where(t => t.ActionId == customAction.Action.Id))
            {
                entities.ToolbarButtonSet.DeleteObject(t);
            }

            foreach (var t in MapperFacade.ToolbarButtonMapper.GetDalList(customAction.Action.ToolbarButtons.ToList()))
            {
                entities.ToolbarButtonSet.AddObject(t);
            }

            var refreshBtnDal = CreateRefreshButton(dal.ActionId);
            if (!customAction.Action.IsInterface)
            {
                foreach (var t in entities.ToolbarButtonSet.Where(b => b.ParentActionId == dal.ActionId && b.ActionId == refreshBtnDal.ActionId))
                {
                    entities.ToolbarButtonSet.DeleteObject(t);
                }
            }

            if (customAction.Action.IsInterface && !entities.ToolbarButtonSet.Any(b => b.ParentActionId == dal.ActionId && b.ActionId == refreshBtnDal.ActionId))
            {
                entities.ToolbarButtonSet.AddObject(refreshBtnDal);
            }

            int? oldContextMenuId = null;
            foreach (var c in entities.ContextMenuItemSet.Where(c => c.ActionId == customAction.Action.Id))
            {
                oldContextMenuId = c.ContextMenuId;
                entities.ContextMenuItemSet.DeleteObject(c);
            }
            foreach (var c in MapperFacade.ContextMenuItemMapper.GetDalList(customAction.Action.ContextMenuItems.ToList()))
            {
                entities.ContextMenuItemSet.AddObject(c);
            }

            var dalDb = entities.CustomActionSet
                .Include("Contents")
                .Include("Sites")
                .Single(a => a.Id == customAction.Id);

            var inmemorySiteIDs = new HashSet<decimal>(customAction.Sites.Select(bs => Converter.ToDecimal(bs.Id)));
            var indbSiteIDs = new HashSet<decimal>(dalDb.Sites.Select(bs => Converter.ToDecimal(bs.Id)));
            foreach (var s in dalDb.Sites.ToArray())
            {
                if (!inmemorySiteIDs.Contains(s.Id))
                {
                    entities.SiteSet.Attach(s);
                    dalDb.Sites.Remove(s);
                }
            }
            foreach (var s in MapperFacade.SiteMapper.GetDalList(customAction.Sites.ToList()))
            {
                if (!indbSiteIDs.Contains(s.Id))
                {
                    entities.SiteSet.Attach(s);
                    dal.Sites.Add(s);
                }
            }

            // Binded Contents
            var inmemoryContentIDs = new HashSet<decimal>(customAction.Contents.Select(bs => Converter.ToDecimal(bs.Id)));
            var indbContentIDs = new HashSet<decimal>(dalDb.Contents.Select(bs => Converter.ToDecimal(bs.Id)));
            foreach (var s in dalDb.Contents.ToArray())
            {
                if (!inmemoryContentIDs.Contains(s.Id))
                {
                    entities.ContentSet.Attach(s);
                    dalDb.Contents.Remove(s);
                }
            }
            foreach (var s in MapperFacade.ContentMapper.GetDalList(customAction.Contents.ToList()))
            {
                if (!indbContentIDs.Contains(s.Id))
                {
                    entities.ContentSet.Attach(s);
                    dal.Contents.Add(s);
                }
            }

            entities.SaveChanges();
            if (oldContextMenuId != customAction.Action.EntityType.ContextMenu.Id)
            {
                SetBottomSeparator(oldContextMenuId);
            }

            SetBottomSeparator(customAction.Action.EntityType.ContextMenu.Id);
            var updated = MapperFacade.CustomActionMapper.GetBizObject(dal);
            BackendActionCache.Reset();

            return updated;
        }

        internal static CustomAction Save(CustomAction customAction)
        {
            var entities = QPContext.EFContext;
            var actionDal = MapperFacade.BackendActionMapper.GetDalObject(customAction.Action);
            entities.BackendActionSet.AddObject(actionDal);

            EntityObject.VerifyIdentityInserting(EntityTypeCode.BackendAction, actionDal.Id, customAction.ForceActionId);
            if (customAction.ForceActionId != 0)
            {
                actionDal.Id = customAction.ForceActionId;
            }

            DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.BackendAction);
            entities.SaveChanges();
            DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.BackendAction);

            var customActionDal = MapperFacade.CustomActionMapper.GetDalObject(customAction);
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

            foreach (var t in MapperFacade.ToolbarButtonMapper.GetDalList(customAction.Action.ToolbarButtons.ToList()))
            {
                t.ActionId = customActionDal.Action.Id;
                entities.ToolbarButtonSet.AddObject(t);
            }

            foreach (var c in MapperFacade.ContextMenuItemMapper.GetDalList(customAction.Action.ContextMenuItems.ToList()))
            {
                c.ActionId = customActionDal.Action.Id;
                entities.ContextMenuItemSet.AddObject(c);
            }

            foreach (var s in MapperFacade.SiteMapper.GetDalList(customAction.Sites.ToList()))
            {
                entities.SiteSet.Attach(s);
                customActionDal.Sites.Add(s);
            }

            foreach (var s in MapperFacade.ContentMapper.GetDalList(customAction.Contents.ToList()))
            {
                entities.ContentSet.Attach(s);
                customActionDal.Contents.Add(s);
            }

            if (customAction.Action.IsInterface)
            {
                var refreshBtnDal = CreateRefreshButton(customActionDal.ActionId);
                entities.ToolbarButtonSet.AddObject(refreshBtnDal);
            }

            entities.SaveChanges();
            var contextMenuId = entities.EntityTypeSet.Single(t => t.Id == customAction.Action.EntityTypeId).ContextMenuId;
            SetBottomSeparator(contextMenuId);

            var updated = MapperFacade.CustomActionMapper.GetBizObject(customActionDal);
            BackendActionCache.Reset();
            return updated;
        }

        internal static void Delete(int id)
        {
            var entities = QPContext.EFContext;
            var dalDb = entities.CustomActionSet
                .Include("Action.ToolbarButtons")
                .Include("Action.ContextMenuItems")
                .Include("Action.EntityType")
                .Single(a => a.Id == id);

            var contextMenuId = dalDb.Action.EntityType.ContextMenuId;
            foreach (var t in dalDb.Action.ToolbarButtons.ToArray())
            {
                entities.ToolbarButtonSet.DeleteObject(t);
            }

            foreach (var t in entities.ToolbarButtonSet.Where(b => b.ParentActionId == dalDb.ActionId))
            {
                entities.ToolbarButtonSet.DeleteObject(t);
            }

            int? oldContextMenuId = null;
            foreach (var c in dalDb.Action.ContextMenuItems.ToArray())
            {
                oldContextMenuId = c.ContextMenuId;
                entities.ContextMenuItemSet.DeleteObject(c);
            }

            entities.BackendActionSet.DeleteObject(dalDb.Action);
            entities.CustomActionSet.DeleteObject(dalDb);
            entities.SaveChanges();
            if (oldContextMenuId != contextMenuId)
            {
                SetBottomSeparator(oldContextMenuId);
            }

            SetBottomSeparator(contextMenuId);
            BackendActionCache.Reset();
        }

        internal static CustomAction Copy(CustomAction action)
        {
            var oldId = action.Id;
            var oldName = action.Name;
            action.Name = MutateName(action.Name);
            if (action.Alias != null)
            {
                action.Alias = MutateAlias(action.Alias);
            }
            action.CalculateOrder(action.Action.EntityTypeId, true, action.Order);

            var newAction = Save(action);

            return GetById(newAction.Id);
        }

        private static string MutateAlias(string alias)
        {
            string newAlias;
            var index = 0;
            do
            {
                index++;
                newAlias = MutateHelper.MutateNetName(alias, index);
            }
            while (ExistAlias(newAlias));
            return newAlias;
        }

        private static string MutateName(string name)
        {
            string newName;
            var index = 0;
            do
            {
                index++;
                newName = MutateHelper.MutateString(name, index);
            }
            while (ExistName(newName));
            return newName;
        }

        private static bool ExistName(string name)
        {
            return QPContext.EFContext.CustomActionSet.Any(a => a.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        private static bool ExistAlias(string alias)
        {
            return QPContext.EFContext.CustomActionSet.Any(a => a.Alias.Equals(alias, StringComparison.InvariantCultureIgnoreCase));
        }

        private static IEnumerable<int> ExistOrders()
        {
            return QPContext.EFContext.CustomActionSet.Select(s => s.Order);
        }

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

        internal static bool IsOrderUnique(int exceptActionId, int order, int entityTypeId)
        {
            return !QPContext.EFContext.CustomActionSet.Include("Action").Any(c => c.Action.EntityTypeId == entityTypeId && c.Id != exceptActionId && c.Order == order);
        }

        private static void SetBottomSeparator(int? contextMenuId)
        {
            if (contextMenuId.HasValue)
            {
                var entities = QPContext.EFContext;
                var maxOrderNotCustomActionMenuItem = entities.ContextMenuItemSet
                    .Where(i => i.ContextMenu.Id == contextMenuId.Value && !i.Action.IsCustom)
                    .OrderByDescending(i => i.Order)
                    .FirstOrDefault();

                if (maxOrderNotCustomActionMenuItem != null)
                {
                    var customActionMenuItemExist = entities.ContextMenuItemSet.Any(i => i.ContextMenu.Id == contextMenuId.Value && i.Action.IsCustom);
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
            var refreshAction = BackendActionRepository.GetByCode(ActionCode.RefreshCustomAction);
            if (refreshAction == null)
            {
                throw new ApplicationException($"Action is not found: {ActionCode.RefreshCustomAction}");
            }

            var refreshBtnDal = new ToolbarButtonDAL
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
