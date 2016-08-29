using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository;
using System.Security;
using Quantumart.QP8.BLL.SharedLogic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Services
{
	public interface ICustomActionService
	{
		CustomActionPrepareResult PrepareForExecuting(string code, string tabId, IEnumerable<int> IDs, int parentId);

		ListResult<CustomActionListItem> List(ListCommand listCommand);

		CustomAction Read(int id);

		CustomAction ReadForUpdate(int id);

		CustomAction Update(CustomAction customAction, int[] selectedActionsIds);

		CustomAction New();

		CustomAction NewForSave();

		CustomAction Save(CustomAction customAction, int[] selectedActionsIds);

		MessageResult Remove(int id);

		IEnumerable<ListItem> GetActionTypeList();

		IEnumerable<ListItem> GetEntityTypeList();

		IEnumerable<Site> GetSites(IEnumerable<int> siteIDs);

		IEnumerable<Content> GetContents(IEnumerable<int> contentIDs);

		CustomActionInitListResult InitList(int parentId);
	}

	public class CustomActionService : ICustomActionService
	{
		private readonly string CONTROLLER_ACTION_URL = @"~/CustomAction/Execute/";
		private readonly string TOOLBAR_BUTTON_DEFAULT_ICON_URL = "custom_action.gif";
		
		private readonly ISessionLogRepository repository;		

		public CustomActionService(ISessionLogRepository repository)
		{
			this.repository = repository;
		}

		static CustomActionService()
		{
			entityTypeList = new Lazy<IEnumerable<ListItem>>(LoadEntityTypeList, true);
			actionTypeList = new Lazy<IEnumerable<ListItem>>(LoadActionTypeList, true);
		}

		#region ICustomActionService Members

		public CustomActionPrepareResult PrepareForExecuting(string code, string hostId, IEnumerable<int> IDs, int parentId)
		{
			CustomAction action = CustomActionRepository.GetByCode(code);			

			SessionsLog session = repository.GetCurrent();
			if (session == null)
				throw new SecurityException("Session log record doesn't exist");
			session.Sid = Guid.NewGuid().ToString();
			repository.Update(session);

			action.SessionId = session.Sid;
			action.Ids = IDs;
			action.ParentId = parentId;

			return SecurityCheck(
				new CustomActionPrepareResult
				{
					CustomAction = action				
				}, 
				action, IDs
			);			
		}		

		public ListResult<CustomActionListItem> List(ListCommand cmd)
		{
			int totalRecords;
			IEnumerable<CustomActionListItem> list = CustomActionRepository.List(cmd, out totalRecords);
			return new ListResult<CustomActionListItem>
			{
				Data = list.ToList(),
				TotalRecords = totalRecords
			};
		}

		public IEnumerable<Site> GetSites(IEnumerable<int> siteIDs)
		{
			return SiteRepository.GetList(siteIDs);			
		}

		public IEnumerable<Content> GetContents(IEnumerable<int> contentIDs)
		{
			return ContentRepository.GetList(contentIDs);						
		}
		

		#region CRUID
		public CustomAction Read(int id)
		{
			CustomAction action = CustomActionRepository.GetById(id);
			if (action == null)
				throw new ApplicationException(String.Format(CustomActionStrings.ActionNotFound, id));
			return action;
		}

		public CustomAction ReadForUpdate(int id)
		{
			return Read(id);
		}

		public CustomAction Update(CustomAction customAction, int[] selectedActionsIds)
		{
			if (customAction == null)
				throw new ArgumentNullException("customAction");
			if (!CustomActionRepository.Exists(customAction.Id))
				throw new ApplicationException(String.Format(CustomActionStrings.ActionNotFound, customAction.Id));
			customAction = Normalize(customAction, selectedActionsIds);
			customAction = CustomActionRepository.Update(customAction);
			return customAction;
		}

		/// <summary>
		/// Нормализация состояния Custom Action перед сохранением
		/// </summary>
		/// <param name="customAction"></param>
		/// <returns></returns>		
		private CustomAction Normalize(CustomAction customAction, int[] selectedActionsIds)
		{			
			if (customAction.IsNew)
			{
				customAction.Action.ControllerActionUrl = CONTROLLER_ACTION_URL;
				customAction.Action.Code = 
					(!String.IsNullOrEmpty(customAction.ForceActionCode)) ? 
						customAction.ForceActionCode : 
						String.Format("custom_{0}", DateTime.Now.Ticks)
				;
			}

			EntityType etype = EntityTypeRepository.GetById(customAction.Action.EntityTypeId);
			if (etype.ContextMenu == null)
			{
				etype.ContextMenu = ContextMenuRepository.GetByCode(etype.Code);
			}
			customAction.Action.EntityType = etype;

			customAction.Action.Name = customAction.Name;
			customAction.Action.TabId = etype.TabId;

			customAction.CalcOrder(etype.Id);

			if (!customAction.ShowInToolbar)
			{
				customAction.ParentActions = null;
				customAction.Action.ToolbarButtons = Enumerable.Empty<ToolbarButton>();
			}
			else if (selectedActionsIds != null)
			{
				List<ToolbarButton> result = new List<ToolbarButton>();
				foreach (int id in selectedActionsIds)
				{
					ToolbarButton button = (customAction.Action.ToolbarButtons == null) ? null : customAction.Action.ToolbarButtons.FirstOrDefault(n => n.ParentActionId == id);
					if (button == null)
						button = new ToolbarButton();
					button.ActionId = customAction.Action.Id;
					button.ParentActionId = id;
					button.Name = customAction.Name;
					button.Icon = !String.IsNullOrWhiteSpace(customAction.IconUrl) ? customAction.IconUrl : TOOLBAR_BUTTON_DEFAULT_ICON_URL;
					button.Order = customAction.Order + 1000;
					result.Add(button);
				}
				customAction.Action.ToolbarButtons = result;
			}


			BackendActionType actionType = BackendActionTypeRepository.GetById(customAction.Action.TypeId);
			if (customAction.ShowInMenu)
			{
				if (etype.ContextMenu == null)
					throw new ApplicationException(String.Format(CustomActionStrings.ThereIsNoContextMenuForEntityType, etype.Name));
				
				ContextMenuItem contentMenuItem = null;
				if (customAction.Action.ContextMenuItems != null && customAction.Action.ContextMenuItems.Any())
					contentMenuItem = customAction.Action.ContextMenuItems.First();
				else
					contentMenuItem = new ContextMenuItem();

				contentMenuItem.ContextMenuId = etype.ContextMenu.Id;
				contentMenuItem.ActionId = customAction.Action.Id;
				contentMenuItem.Name = customAction.Name;
				contentMenuItem.Icon = !String.IsNullOrWhiteSpace(customAction.IconUrl) ? customAction.IconUrl : TOOLBAR_BUTTON_DEFAULT_ICON_URL;
				contentMenuItem.Order = customAction.Order + 1000;
				customAction.Action.ContextMenuItems = new[] { contentMenuItem };
			}
			else
				customAction.Action.ContextMenuItems = Enumerable.Empty<ContextMenuItem>();

			if (!customAction.Action.IsInterface)
				customAction.Action.IsWindow = false;
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

			if (!IsEntityTypeSiteDescendants(etype.Id))
			{
				customAction.Sites = Enumerable.Empty<Site>();
				customAction.SiteExcluded = false;
			}

			if (!IsEntityTypeContentDescendants(etype.Id))
			{
				customAction.Contents = Enumerable.Empty<Content>();
				customAction.ContentExcluded = false;
			}


			

			return customAction;
		}
		

		public CustomAction New()
		{
			return CustomAction.CreateNew();
		}

		public CustomAction NewForSave()
		{
			return CustomAction.CreateNew();
		}

		public CustomAction Save(CustomAction customAction, int[] selectedActionsIds)
		{
			if (customAction == null)
				throw new ArgumentNullException("customAction");
			customAction = Normalize(customAction, selectedActionsIds);
			customAction = CustomActionRepository.Save(customAction);
			return customAction;
		}

		public MessageResult Remove(int id)
		{
			CustomAction action = Read(id);
			CustomActionRepository.Delete(id);
			return null;
		}	 
		#endregion		

		#region View Model Data

		public IEnumerable<ListItem> GetActionTypeList()
		{
			return actionTypeList.Value
				.Select(i => new ListItem(i.Value, Translator.Translate(i.Text), i.DependentItemIDs))
				.OrderBy(n => n.Text)
				.ToArray();
		}

		public IEnumerable<ListItem> GetEntityTypeList()
		{
			return entityTypeList.Value
				.Select(i => new ListItem(i.Value, Translator.Translate(i.Text), i.DependentItemIDs))
				.OrderBy(n => n.Text)
				.ToArray();
		}

		public CustomActionInitListResult InitList(int parentId)
		{
			return new CustomActionInitListResult
			{
				IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewCustomAction)
			};
		}
		#endregion

		private static bool IsEntityTypeSiteDescendants(int entityTypeId)
		{
			return LoadEntityTypeList()
				.Where(l => l.Value == entityTypeId.ToString() &&
							l.DependentItemIDs != null && l.DependentItemIDs.Contains(SITE_SELECTION_MODE_PANEL_NAME)
					  )
				.Any();
		}

		private static bool IsEntityTypeContentDescendants(int entityTypeId)
		{
			return LoadEntityTypeList()
				.Where(l => l.Value == entityTypeId.ToString() &&
							l.DependentItemIDs != null && l.DependentItemIDs.Contains(CONTENT_SELECTION_MODE_PANEL_NAME)
					  )
				.Any();			
		}


		#endregion

		#region Load Data
		private static readonly Lazy<IEnumerable<ListItem>> entityTypeList;
		private static readonly Lazy<IEnumerable<ListItem>> actionTypeList;

		private static readonly string SITE_SELECTION_MODE_PANEL_NAME = "siteSelectionModePanel";
		private static readonly string CONTENT_SELECTION_MODE_PANEL_NAME = "contentSelectionModePanel";

		private static IEnumerable<ListItem> LoadEntityTypeList()
		{
			IEnumerable<EntityType> entityTypes = EntityTypeRepository.GetList()
				.Where(t => !t.Code.Equals(EntityTypeCode.CustomAction, StringComparison.InvariantCultureIgnoreCase))
				.OrderBy(n => n.Id)
				.ToArray();

			HashSet<int> siteDescendants = new HashSet<int>(GetDescendantEntityTypes(EntityTypeCode.Site, entityTypes).Select(t => t.Id));
			HashSet<int> contentDescendants = new HashSet<int>(
				GetDescendantEntityTypes(EntityTypeCode.Content, entityTypes).Select(t => t.Id).Union(
					GetDescendantEntityTypes(EntityTypeCode.VirtualContent, entityTypes).Select(t => t.Id)
				)
			);

			Func<int, string[]> getListItemDependentIds = (etID) =>
			{
				if (siteDescendants.Contains(etID))
				{
					if (contentDescendants.Contains(etID))
						return new[] { SITE_SELECTION_MODE_PANEL_NAME, CONTENT_SELECTION_MODE_PANEL_NAME };
					else
						return new[] { SITE_SELECTION_MODE_PANEL_NAME };
				}
				else
					return null;
			};


			return entityTypes.Select(n =>
				new ListItem(n.Id.ToString(), n.Name, getListItemDependentIds(n.Id))
			)
			.ToArray();
		}		

		private static IEnumerable<EntityType> GetDescendantEntityTypes(string rootCode, IEnumerable<EntityType> allTypes)
		{
			List<EntityType> result = new List<EntityType>();

			Action<EntityType> traverse = null;
			traverse = (parent) =>
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
				.Select(n =>
					new ListItem(n.Id.ToString(), n.Name)
				)
				.ToArray();
		}
		#endregion

		#region Security
		private static CustomActionPrepareResult SecurityCheck(CustomActionPrepareResult result, CustomAction action, IEnumerable<int> IDs)
		{
			result.IsActionAccessable = true;
			result.SecurityErrorMesage = null;

			if (!SecurityRepository.IsActionAccessible(action.Action.Code))
			{
				result.IsActionAccessable = false;
				result.SecurityErrorMesage = String.Format(GlobalStrings.ActionIsNotAccessible, action.Name);
			}
			else
			{
				IEnumerable<int> notAccessedIDs = EntityPermissionCheck(action, IDs);
				if (notAccessedIDs.Any())
				{
					result.IsActionAccessable = false;
					result.SecurityErrorMesage = String.Format(GlobalStrings.EntityIsNotAccessible, action.Action.ActionType.Name, action.Action.EntityType.Name, String.Join(",", notAccessedIDs));;
				}								
					
			}

			return result;
		}

		private static IEnumerable<int> EntityPermissionCheck(CustomAction action, IEnumerable<int> IDs)
		{
			return IDs.Where(id => 
				!SecurityRepository.IsEntityAccessible(action.Action.EntityType.Code, id, action.Action.ActionType.Code)
			).ToArray();	
		}
		#endregion
	}
}
