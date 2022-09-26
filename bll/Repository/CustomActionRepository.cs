using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository.Helpers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;
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
                var dbType = QPContext.DatabaseType;
                cmd.SortExpression = MapperFacade.CustomActionListItemRowMapper.TranslateSortExpression(cmd.SortExpression, dbType);
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

        // internal static CustomAction GetRealById(int id)
        // {
        //     var customAction = MapperFacade.CustomActionMapper.GetBizObject(
        //             QPContext.EFContext.CustomActionSet
        //                 .Include(b => b.ContentCustomActionBinds)
        //                 .Include(b => b.SiteCustomActionBinds)
        //                 .Include(b => b.Action)
        //                 .SingleOrDefault()
        //         );
        //
        //         foreach (var c in customActions)
        //         {
        //             c.Action = backendActions.FirstOrDefault(n => n.Id == c.ActionId);
        //         }
        //
        //         return customActions;
        //     }
        // }

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

            entities.Entry(dal).State = EntityState.Modified;

            var dal2 = MapperFacade.BackendActionMapper.GetDalObject(customAction.Action);
            entities.Entry(dal2).State = EntityState.Modified;

            // Toolbar Buttons
            foreach (var t in entities.ToolbarButtonSet.Where(t => t.ActionId == customAction.Action.Id))
            {
                entities.Entry(t).State = EntityState.Deleted;
            }

            foreach (var t in MapperFacade.ToolbarButtonMapper.GetDalList(customAction.Action.ToolbarButtons.ToList()))
            {
                entities.Entry(t).State = EntityState.Added;
            }

            var refreshBtnDal = CreateRefreshButton(dal.ActionId);
            if (!customAction.Action.IsInterface)
            {
                foreach (var t in entities.ToolbarButtonSet.Where(b => b.ParentActionId == dal.ActionId && b.ActionId == refreshBtnDal.ActionId))
                {
                    entities.Entry(t).State = EntityState.Deleted;
                }
            }

            if (customAction.Action.IsInterface && !entities.ToolbarButtonSet.Any(b => b.ParentActionId == dal.ActionId && b.ActionId == refreshBtnDal.ActionId))
            {
                entities.Entry(refreshBtnDal).State = EntityState.Added;
            }

            int? oldContextMenuId = null;
            foreach (var c in entities.ContextMenuItemSet.Where(c => c.ActionId == customAction.Action.Id))
            {
                oldContextMenuId = c.ContextMenuId;
                entities.Entry(c).State = EntityState.Deleted;
            }
            foreach (var c in MapperFacade.ContextMenuItemMapper.GetDalList(customAction.Action.ContextMenuItems.ToList()))
            {
                entities.Entry(c).State = EntityState.Added;
            }

            var dalDb = entities.CustomActionSet
                .Include(x => x.ContentCustomActionBinds)
                .Include(x => x.SiteCustomActionBinds)
                .Single(a => a.Id == customAction.Id);

            var inmemorySiteIDs = new HashSet<decimal>(customAction.SiteIds.Select(bs => Converter.ToDecimal(bs)));
            var dalSiteBinds = dalDb.SiteCustomActionBinds.ToArray();
            var indbSiteIDs = new HashSet<decimal>(dalSiteBinds.Select(bs => bs.SiteId));
            var dalSiteBindsToRemove = dalSiteBinds.Where(n => !inmemorySiteIDs.Contains(n.SiteId)).ToArray();
            var dalSiteBindsToCreate = customAction.SiteIds.Where(n => !indbSiteIDs.Contains(n)).Select(
                n => new SiteCustomActionBindDAL { CustomAction = dal, SiteId = n }
            );
            foreach (var s in dalSiteBindsToRemove)
            {
                dal.SiteCustomActionBinds.Remove(s);
            }

            foreach (var c in dalSiteBindsToCreate)
            {
                dal.SiteCustomActionBinds.Add(c);
            }

            // Binded Contents
            var inmemoryContentIDs = new HashSet<decimal>(customAction.ContentIds.Select(bs => Converter.ToDecimal(bs)));
            var dalBinds = dalDb.ContentCustomActionBinds.ToArray();
            var indbContentIDs = new HashSet<decimal>(dalBinds.Select(bs => bs.ContentId));
            var dalBindsToRemove = dalBinds.Where(n => !inmemoryContentIDs.Contains(n.ContentId)).ToArray();
            var dalbindsToCreate = customAction.ContentIds.Where(n => !indbContentIDs.Contains(n)).Select(
                n => new ContentCustomActionBindDAL { CustomAction = dal, ContentId = n }
            );

            foreach (var r in dalBindsToRemove)
            {
                dal.ContentCustomActionBinds.Remove(r);
            }

            foreach (var c in dalbindsToCreate)
            {
                dal.ContentCustomActionBinds.Add(c);
            }

            entities.SaveChanges();
            if (oldContextMenuId != customAction.Action.EntityType.ContextMenu.Id)
            {
                SetBottomSeparator(oldContextMenuId);
            }

            SetBottomSeparator(customAction.Action.EntityType.ContextMenu.Id);
            var updated = MapperFacade.CustomActionMapper.GetBizObject(dal);
            BackendActionCache.ResetForCustomerCode();

            return updated;
        }

        internal static CustomAction Save(CustomAction customAction)
        {
            var entities = QPContext.EFContext;
            var actionDal = MapperFacade.BackendActionMapper.GetDalObject(customAction.Action);
            entities.Entry(actionDal).State = EntityState.Added;

            EntityObject.VerifyIdentityInserting(EntityTypeCode.BackendAction, actionDal.Id, customAction.ForceActionId);
            if (customAction.ForceActionId != 0)
            {
                actionDal.Id = customAction.ForceActionId;
            }

            if (!string.IsNullOrEmpty(customAction.ForceActionCode))
            {
                actionDal.Code = customAction.ForceActionCode;
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

            if (customAction.ForceId != 0)
            {
                customActionDal.Id = customAction.ForceId;
            }
            entities.Entry(customActionDal).State = EntityState.Added;

            DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.CustomAction, customAction);
            entities.SaveChanges();
            DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.CustomAction);

            var buttonsToInsert = MapperFacade.ToolbarButtonMapper.GetDalList(customAction.Action.ToolbarButtons.ToList());
            foreach (var item in buttonsToInsert)
            {
                item.ActionId = customActionDal.ActionId;
                entities.Entry(item).State = EntityState.Added;
            }

            var cmiToInsert = MapperFacade.ContextMenuItemMapper.GetDalList(customAction.Action.ContextMenuItems.ToList());
            foreach (var item in cmiToInsert)
            {
                item.ActionId = customActionDal.ActionId;
                entities.Entry(item).State = EntityState.Added;
            }

            customActionDal.SiteCustomActionBinds = new List<SiteCustomActionBindDAL>();
            foreach (var item in customAction.SiteIds)
            {
                var bind = new SiteCustomActionBindDAL { SiteId = item, CustomAction = customActionDal };
                customActionDal.SiteCustomActionBinds.Add(bind);
            }

            customActionDal.ContentCustomActionBinds = new List<ContentCustomActionBindDAL>();
            foreach (var item in customAction.ContentIds)
            {
                var bind = new ContentCustomActionBindDAL { ContentId = item, CustomAction = customActionDal };
                customActionDal.ContentCustomActionBinds.Add(bind);
            }

            if (customAction.Action.IsInterface)
            {
                var refreshBtnDal = CreateRefreshButton(customActionDal.ActionId);
                entities.Entry(refreshBtnDal).State = EntityState.Added;
            }

            entities.SaveChanges();
            var contextMenuId = entities.EntityTypeSet.Single(t => t.Id == customAction.Action.EntityTypeId).ContextMenuId;
            SetBottomSeparator(contextMenuId);

            var updated = MapperFacade.CustomActionMapper.GetBizObject(customActionDal);
            updated.Action = MapperFacade.BackendActionMapper.GetBizObject(actionDal);
            BackendActionCache.ResetForCustomerCode();
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
                entities.Entry(t).State = EntityState.Deleted;
            }

            foreach (var t in entities.ToolbarButtonSet.Where(b => b.ParentActionId == dalDb.ActionId))
            {
                entities.Entry(t).State = EntityState.Deleted;
            }

            int? oldContextMenuId = null;
            foreach (var c in dalDb.Action.ContextMenuItems.ToArray())
            {
                oldContextMenuId = c.ContextMenuId;
                entities.Entry(c).State = EntityState.Deleted;
            }
            entities.Entry(dalDb.Action).State = EntityState.Deleted;
            entities.Entry(dalDb).State = EntityState.Deleted;
            entities.SaveChanges();

            if (oldContextMenuId != contextMenuId)
            {
                SetBottomSeparator(oldContextMenuId);
            }

            SetBottomSeparator(contextMenuId);
            BackendActionCache.ResetForCustomerCode();
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
            while (DoesAliasExist(newAlias));
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
            while (DoesNameExist(newName));
            return newName;
        }

        private static bool DoesNameExist(string name)
        {
            return QPContext.EFContext.CustomActionSet.Any(a => a.Name.Equals(name));
        }

        private static bool DoesAliasExist(string alias)
        {
            return QPContext.EFContext.CustomActionSet.Any(
                a => a.Alias != null && a.Alias.Equals(alias)
            );
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
