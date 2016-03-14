using System;
using System.Dynamic;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public abstract class ViewModel
    {
        public string TabId { get; set; }

        public int ParentEntityId { get; set; }

        public bool IsVirtual { get; set; }

        public bool IsViewChangable { get; protected set; }

		#region creation
		public ViewModel()
		{
			IsVirtual = false;
			IsViewChangable = true;
			HostUID = Guid.NewGuid().ToString();
		}
		
		public static T Create<T>(string tabId, int parentId) where T : ViewModel, new()
		{
		    var model = new T
		    {
		        ParentEntityId = parentId,
		        TabId = tabId
		    };

		    return model;
		}
		#endregion

        #region read-only members
		public abstract string EntityTypeCode { get; }

		public abstract string ActionCode { get; }

		public abstract C.MainComponentType MainComponentType { get; }

		public abstract string MainComponentId { get; }

		public virtual C.DocumentContextState DocumentContextState 
		{
			get { return C.DocumentContextState.None; } 
		}

		public virtual ExpandoObject MainComponentParameters
		{
			get 
			{
				dynamic result = new ExpandoObject();
				result.hostId = DocumentHostId;
				result.hostUID = HostUID;
				result.isWindow = IsWindow;
				result.parentEntityId = ParentEntityId;
				result.entityTypeCode = EntityTypeCode;
				result.actionCode = ActionCode;
				result.state = DocumentContextState;
				result.mainComponentType = (int)MainComponentType;
				result.mainComponentId = MainComponentId;
				return result;
			}
		}

		public virtual ExpandoObject MainComponentOptions 
		{
			get { return new ExpandoObject();  }
		}
		
		public string AddNewItemLinkId
        {
            get
            {
                return UniqueId("addNewItemLink");
            }
        }

		public string ContextObjectName
		{
			get
			{
				return UniqueId("context");
			}
		}

        public string DocumentHostId
        {
            get
            {
                if (IsWindow)
                {
                    return TabId;
                }

                return UniqueId("document");
            }
        }

		public string ValidationSummaryId
		{
			get { return UniqueId("Summary"); }
		}

		public string HostUID { get; private set; }

        public bool IsWindow
        {
            get
            {
                return HtmlHelperFieldExtensions.IsWindow(TabId);
            }
        }

        public string UniqueId(string id)
        {
            return HtmlHelperFieldExtensions.UniqueId(id, TabId);
        }

		public bool IsAdmin
		{
			get
			{
				return QPContext.IsAdmin;
			}
		}
        #endregion
     }
}