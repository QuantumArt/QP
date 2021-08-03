using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services
{
    public class CustomActionService : ICustomActionService
    {
        private const string ControllerActionUrl = @"~/CustomAction/Execute/";
        private const string ToolbarButtonDefaultIconUrl = "custom_action.gif";
        private readonly ISessionLogRepository _repository;

        public CustomActionService(ISessionLogRepository repository)
        {
            _repository = repository;
        }

        public CustomActionPrepareResult PrepareForExecuting(string hostId, int parentId, CustomActionQuery query)
        {
            var action = CustomActionRepository.GetByCode(query.ActionCode);
            var session = _repository.GetCurrent();
            if (session == null)
            {
                throw new SecurityException("Session log record doesn't exist");
            }

            session.Sid = Guid.NewGuid().ToString();
            _repository.Update(session);

            action.SessionId = session.Sid;
            action.Ids = query.Ids;
            action.ParentId = parentId;

            return SecurityCheck(new CustomActionPrepareResult { CustomAction = action }, action, query.Ids);
        }

        public ListResult<CustomActionListItem> List(ListCommand cmd)
        {
            var list = CustomActionRepository.List(cmd, out var totalRecords);
            return new ListResult<CustomActionListItem>
            {
                Data = list.ToList(),
                TotalRecords = totalRecords
            };
        }

        public CustomAction Read(int id)
        {
            var action = CustomActionRepository.GetById(id);

            if (action == null)
            {
                throw new ApplicationException(string.Format(CustomActionStrings.ActionNotFound, id));
            }

            return action;
        }

        public CustomAction ReadForModify(int id)
        {
            return Read(id).Clone();
        }

        public CustomAction Update(CustomAction customAction, int[] selectedActionsIds)
        {
            if (customAction == null)
            {
                throw new ArgumentNullException(nameof(customAction));
            }

            if (!CustomActionRepository.Exists(customAction.Id))
            {
                throw new ApplicationException(string.Format(CustomActionStrings.ActionNotFound, customAction.Id));
            }

            customAction = Normalize(customAction, selectedActionsIds);
            customAction = CustomActionRepository.Update(customAction);
            return customAction;
        }

        public CopyResult Copy(int id, int[] selectedActionsIds)
        {
            var result = new CopyResult();
            var action = ReadForModify(id);
            action.Id = 0;
            if (action.Action != null)
            {
                action.Action.Id = 0;

            }

            if (action == null)
            {
                throw new Exception(string.Format(CustomActionStrings.ActionNotFoundByCode, id));
            }
            if (!action.IsUpdatable || !action.IsAccessible(ActionTypeCode.Read))
            {
                result.Message = MessageResult.Error(CustomActionStrings.CannotCopyBecauseOfSecurity);
            }

            if (result.Message == null)
            {
                action = Normalize(action, selectedActionsIds);
                action = CustomActionRepository.Copy(action);
            }

            return result;
        }

        private static CustomAction Normalize(CustomAction customAction, int[] selectedActionsIds)
        {
            if (customAction.IsNew)
            {
                customAction.Action.ControllerActionUrl = ControllerActionUrl;
                customAction.Action.Code = !string.IsNullOrEmpty(customAction.ForceActionCode) ? customAction.ForceActionCode : $"custom_{DateTime.Now.Ticks}";
            }

            var entityType = EntityTypeRepository.GetById(customAction.Action.EntityTypeId);
            if (entityType.ContextMenu == null)
            {
                entityType.ContextMenu = ContextMenuRepository.GetByCode(entityType.Code);
            }

            customAction.Action.EntityType = entityType;
            customAction.Action.Name = customAction.Name;
            customAction.Action.TabId = entityType.TabId;
            customAction.CalculateOrder(entityType.Id);

            if (!customAction.ShowInToolbar)
            {
                customAction.ParentActions = null;
                customAction.Action.ToolbarButtons = Enumerable.Empty<ToolbarButton>();
            }
            else if (selectedActionsIds != null)
            {
                var result = new List<ToolbarButton>();
                foreach (var id in selectedActionsIds)
                {
                    var button = customAction.Action.ToolbarButtons?.FirstOrDefault(n => n.ParentActionId == id) ?? new ToolbarButton();
                    button.ActionId = customAction.Action.Id;
                    button.ParentActionId = id;
                    button.Name = customAction.Name;
                    button.Icon = !string.IsNullOrWhiteSpace(customAction.IconUrl) ? customAction.IconUrl : ToolbarButtonDefaultIconUrl;
                    button.Order = customAction.Order + 1000;
                    result.Add(button);
                }

                customAction.Action.ToolbarButtons = result;
            }

            BackendActionTypeRepository.GetById(customAction.Action.TypeId);
            if (customAction.ShowInMenu)
            {
                if (entityType.ContextMenu == null)
                {
                    throw new ApplicationException(string.Format(CustomActionStrings.ThereIsNoContextMenuForEntityType, entityType.Name));
                }

                ContextMenuItem contentMenuItem;
                if (customAction.Action.ContextMenuItems != null && customAction.Action.ContextMenuItems.Any())
                {
                    contentMenuItem = customAction.Action.ContextMenuItems.First();
                }
                else
                {
                    contentMenuItem = new ContextMenuItem();
                }

                contentMenuItem.ContextMenuId = entityType.ContextMenu.Id;
                contentMenuItem.ActionId = customAction.Action.Id;
                contentMenuItem.Name = customAction.Name;
                contentMenuItem.Icon = !string.IsNullOrWhiteSpace(customAction.IconUrl) ? customAction.IconUrl : ToolbarButtonDefaultIconUrl;
                contentMenuItem.Order = customAction.Order + 1000;
                customAction.Action.ContextMenuItems = new[] { contentMenuItem };
            }
            else
            {
                customAction.Action.ContextMenuItems = Enumerable.Empty<ContextMenuItem>();
            }

            if (!customAction.Action.IsInterface)
            {
                customAction.Action.IsWindow = false;
            }
            else
            {
                customAction.Action.ConfirmPhrase = null;
                customAction.Action.HasPreAction = false;
            }

            if (!customAction.Action.IsWindow)
            {
                customAction.Action.WindowHeight = null;
                customAction.Action.WindowWidth = null;
            }

            if (!IsEntityTypeSiteDescendants(entityType.Id))
            {
                customAction.SiteIds = new int[] {};
                customAction.SiteExcluded = false;
            }

            if (!IsEntityTypeContentDescendants(entityType.Id))
            {
                customAction.ContentIds = new int[] {};
                customAction.ContentExcluded = false;
            }

            return customAction;
        }

        public CustomAction New() => CustomAction.CreateNew();

        public CustomAction NewForSave() => CustomAction.CreateNew();

        public CustomAction Save(CustomAction customAction, int[] selectedActionsIds)
        {
            if (customAction == null)
            {
                throw new ArgumentNullException(nameof(customAction));
            }

            customAction = Normalize(customAction, selectedActionsIds);
            customAction = CustomActionRepository.Save(customAction);
            return customAction;
        }

        public MessageResult Remove(int id)
        {
            CustomActionRepository.Delete(id);
            return null;
        }

        public IEnumerable<ListItem> GetActionTypeList()
        {
            return LoadActionTypeList()
                .Select(i => new ListItem(i.Value, Translator.Translate(i.Text), i.DependentItemIDs))
                .OrderBy(n => n.Text)
                .ToArray();
        }

        public IEnumerable<ListItem> GetEntityTypeList()
        {
            return LoadEntityTypeList()
                .Select(i => new ListItem(i.Value, Translator.Translate(i.Text), i.DependentItemIDs))
                .OrderBy(n => n.Text)
                .ToArray();
        }

        public IEnumerable<Site> GetSites(CustomAction action)
        {
            return SiteRepository.GetList(action.SiteIds);
        }

        public IEnumerable<Content> GetContents(CustomAction action)
        {
            return ContentRepository.GetList(action.ContentIds, true);
        }

        public InitListResult InitList(int parentId) => new InitListResult
        {
            IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewCustomAction)
        };

        private static bool IsEntityTypeSiteDescendants(int entityTypeId)
        {
            return LoadEntityTypeList().Any(l => l.Value == entityTypeId.ToString() && l.DependentItemIDs != null && l.DependentItemIDs.Contains(SiteSelectionModePanelName));
        }

        private static bool IsEntityTypeContentDescendants(int entityTypeId)
        {
            return LoadEntityTypeList().Any(l => l.Value == entityTypeId.ToString() && l.DependentItemIDs != null && l.DependentItemIDs.Contains(ContentSelectionModePanelName));
        }

        private const string SiteSelectionModePanelName = "siteSelectionModePanel";
        private const string ContentSelectionModePanelName = "contentSelectionModePanel";

        private static IEnumerable<ListItem> LoadEntityTypeList()
        {
            IEnumerable<EntityType> entityTypes = EntityTypeRepository.GetList()
                .Where(t => !t.Code.Equals(EntityTypeCode.CustomAction, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(n => n.Id)
                .ToArray();

            var siteDescendants = new HashSet<int>(GetDescendantEntityTypes(EntityTypeCode.Site, entityTypes).Select(t => t.Id));
            var contentDescendants = new HashSet<int>(
                GetDescendantEntityTypes(EntityTypeCode.Content, entityTypes).Select(t => t.Id).Union(
                    GetDescendantEntityTypes(EntityTypeCode.VirtualContent, entityTypes).Select(t => t.Id)
                )
            );

            Func<int, string[]> getListItemDependentIds = etId =>
            {
                if (siteDescendants.Contains(etId))
                {
                    return contentDescendants.Contains(etId)
                        ? new[] { SiteSelectionModePanelName, ContentSelectionModePanelName }
                        : new[] { SiteSelectionModePanelName };
                }

                return null;
            };

            return entityTypes.Select(n => new ListItem(n.Id.ToString(), n.Name, getListItemDependentIds(n.Id))).ToArray();
        }

        private static IEnumerable<EntityType> GetDescendantEntityTypes(string rootCode, IEnumerable<EntityType> allTypes)
        {
            var result = new List<EntityType>();
            Action<EntityType> traverse = null;
            traverse = parent =>
            {
                result.Add(parent);
                var children = allTypes.Where(t => t.ParentCode == parent.Code).ToArray();
                foreach (var c in children)
                {
                    traverse(c);
                }
            };

            traverse(allTypes.Single(t => t.Code == rootCode));
            return result;
        }

        private static IEnumerable<ListItem> LoadActionTypeList()
        {
            return BackendActionTypeRepository.GetList()
                .Where(n => n.Code != ActionTypeCode.Refresh)
                .OrderBy(n => n.Id)
                .Select(n => new ListItem(n.Id.ToString(), n.Name))
                .ToArray();
        }

        private static CustomActionPrepareResult SecurityCheck(CustomActionPrepareResult result, CustomAction action, IEnumerable<int> ids)
        {
            result.IsActionAccessable = true;
            result.SecurityErrorMesage = null;

            if (!SecurityRepository.IsActionAccessible(action.Action.Code))
            {
                result.IsActionAccessable = false;
                result.SecurityErrorMesage = string.Format(GlobalStrings.ActionIsNotAccessible, action.Name);
            }
            else
            {
                var notAccessedIDs = EntityPermissionCheck(action, ids).ToList();
                if (notAccessedIDs.Any())
                {
                    result.IsActionAccessable = false;
                    result.SecurityErrorMesage = string.Format(GlobalStrings.EntityIsNotAccessible, action.Action.ActionType.Name, action.Action.EntityType.Name, string.Join(",", notAccessedIDs));
                }
            }

            return result;
        }

        private static IEnumerable<int> EntityPermissionCheck(CustomAction action, IEnumerable<int> ids)
        {
            return ids.Where(id => !SecurityRepository.IsEntityAccessible(action.Action.EntityType.Code, id, action.Action.ActionType.Code)).ToArray();
        }
    }
}
