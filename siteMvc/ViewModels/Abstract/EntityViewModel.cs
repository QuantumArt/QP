using System.Dynamic;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.ViewModels.DirectLink;

namespace Quantumart.QP8.WebMvc.ViewModels.Abstract
{
    public abstract class EntityViewModel : ViewModel
    {
        public EntityObject EntityData { get; set; }

        public string SuccesfulActionCode { get; set; }

        public bool IsValid { get; set; }

        public EntityObject Data
        {
            get => EntityData;
            set => EntityData = value;
        }

        public static T Create<T>(EntityObject obj, string tabId, int parentId) where T : EntityViewModel, new()
        {
            var model = Create<T>(tabId, parentId);
            model.EntityData = obj;
            model.IsValid = true;
            return model;
        }

        public bool IsNew => EntityData.IsNew;

        public override MainComponentType MainComponentType => MainComponentType.Editor;

        public override string MainComponentId => UniqueId("Editor");

        public override ExpandoObject MainComponentParameters
        {
            get
            {
                dynamic result = base.MainComponentParameters;
                result.entityId = Id;
                result.entityName = Name;
                result.previousActionCode = SuccesfulActionCode;
                result.userId = QPContext.CurrentUserId;
                result.userGroupIds = QPContext.CurrentGroupIds;
                return result;
            }
        }

        public override ExpandoObject MainComponentOptions
        {
            get
            {
                dynamic result = base.MainComponentParameters;
                result.validationSummaryElementId = ValidationSummaryId;
                result.modifiedDateTime = Data != null && !Data.IsNew ? Data.Modified.Ticks : new long?();
                result.entityTypeAllowedToAutosave = Data != null && EntityType.CheckToAutosave(Data.EntityTypeCode);

                var saveAndCloseAction = Data?.SaveAndCloseAction;
                if (saveAndCloseAction != null)
                {
                    result.saveAndCloseActionCode = saveAndCloseAction.Code;
                }

                return result;
            }
        }

        public override DocumentContextState DocumentContextState => string.IsNullOrWhiteSpace(SuccesfulActionCode) ? DocumentContextState.Loaded : DocumentContextState.Saved;

        public virtual string Id => EntityData.Id.ToString();

        public virtual string Name => EntityData.Name;

        public virtual DirectLinkOptions DirectLinkOptions
        {
            get
            {
                if (!int.TryParse(Id, out var entityId))
                {
                    entityId = 0;
                }

                return new DirectLinkOptions
                {
                    ActionCode = ActionCode,
                    CustomerCode = QPContext.CurrentCustomerCode,
                    EntityId = entityId > 0 ? entityId : new int?(),
                    EntityTypeCode = EntityTypeCode,
                    ParentEntityId = ParentEntityId > 0 ? ParentEntityId : new int?()
                };
            }
        }

        public virtual void Validate(ModelStateDictionary modelState)
        {
            try
            {
                EntityData.Validate();
            }
            catch (RulesException ex)
            {
                ex.Extend(modelState, "Data");
                IsValid = false;
            }
        }
    }
}
