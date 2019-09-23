using System;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using Newtonsoft.Json;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.ViewModels.Abstract
{
    public abstract class ViewModel
    {
        public string TabId { get; set; }

        public int ParentEntityId { get; set; }

        public bool IsVirtual { get; set; }

        public bool IsViewChangable { get; protected set; }

        protected ViewModel()
        {
            IsVirtual = false;
            IsViewChangable = true;
            HostUid = Guid.NewGuid().ToString();
        }

        public static T Create<T>(string tabId, int parentId)
            where T : ViewModel, new()
        {
            var model = new T
            {
                ParentEntityId = parentId,
                TabId = tabId
            };

            return model;
        }

        public abstract string EntityTypeCode { get; }

        public abstract string ActionCode { get; }

        public abstract MainComponentType MainComponentType { get; }

        public abstract string MainComponentId { get; }

        public virtual DocumentContextState DocumentContextState => DocumentContextState.None;

        public virtual ExpandoObject MainComponentParameters
        {
            get
            {
                dynamic result = new ExpandoObject();
                result.hostId = DocumentHostId;
                result.hostUID = HostUid;
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

        public virtual ExpandoObject MainComponentOptions => new ExpandoObject();

        public string AddNewItemLinkId => UniqueId("addNewItemLink");

        public string ContextObjectName => UniqueId("context");

        public string DocumentHostId => IsWindow ? TabId : UniqueId("document");

        public string ValidationSummaryId => UniqueId("Summary");

        [JsonProperty("HostUID")]
        public string HostUid { get; }

        public bool IsWindow => HtmlHelperFieldExtensions.IsWindow(TabId);

        public string UniqueId(string id) => HtmlHelperFieldExtensions.UniqueId(id, TabId);

        public bool IsAdmin => QPContext.IsAdmin;
    }
}
