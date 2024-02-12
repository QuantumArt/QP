using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Newtonsoft.Json;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.ViewModels.DirectLink;

namespace Quantumart.QP8.WebMvc.ViewModels.Abstract
{
    public abstract class EntityViewModel : ViewModel, IValidatableObject
    {
        protected EntityObject EntityData { get; set; }

        public string SuccesfulActionCode { get; set; }

        public static T Create<T>(EntityObject obj, string tabId, int parentId)
            where T : EntityViewModel, new()
        {
            var model = Create<T>(tabId, parentId);
            model.EntityData = obj;
            return model;
        }

        [ValidateNever]
        [BindNever]
        [JsonIgnore]
        public bool IsNew => EntityData.IsNew;

        [ValidateNever]
        [BindNever]
        [JsonIgnore]
        public bool IsPostgres => QPContext.DatabaseType == DatabaseType.Postgres;

        public override MainComponentType MainComponentType => MainComponentType.Editor;

        [ValidateNever]
        [BindNever]
        [JsonIgnore]
        public override string MainComponentId => UniqueId("Editor");

        [ValidateNever]
        [BindNever]
        [JsonIgnore]
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

        [ValidateNever]
        [BindNever]
        [JsonIgnore]
        public override ExpandoObject MainComponentOptions
        {
            get
            {
                dynamic result = base.MainComponentParameters;
                result.validationSummaryElementId = ValidationSummaryId;
                result.modifiedDateTime = EntityData != null && !EntityData.IsNew ? EntityData.Modified.Ticks : new long?();
                result.entityTypeAllowedToAutosave = EntityData != null && EntityType.CheckToAutosave(EntityData.EntityTypeCode);

                var saveAndCloseAction = EntityData?.SaveAndCloseAction;
                if (saveAndCloseAction != null)
                {
                    result.saveAndCloseActionCode = saveAndCloseAction.Code;
                }

                return result;
            }
        }

        public override DocumentContextState DocumentContextState => string.IsNullOrWhiteSpace(SuccesfulActionCode) ? DocumentContextState.Loaded : DocumentContextState.Saved;

        [ValidateNever]
        [BindNever]
        [JsonIgnore]
        public virtual string Id => EntityData.Id.ToString();

        [ValidateNever]
        [BindNever]
        [JsonIgnore]
        public virtual string Name => EntityData.Name;

        [ValidateNever]
        [BindNever]
        [JsonIgnore]
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

        public virtual void DoCustomBinding()
        {
            EntityData.DoCustomBinding();
        }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var result = new List<ValidationResult>();

            DoCustomBinding();

            try
            {
                Validate();
            }
            catch (RulesException ex)
            {
                result = ex.GetValidationResults("Data").ToList();
            }

            var viewModelResult = ValidateViewModel();
            if (viewModelResult != null)
            {
                result.AddRange(viewModelResult);
            }

            return result;
        }

        public virtual void Validate()
        {
            EntityData.Validate();
        }

        public virtual IEnumerable<ValidationResult> ValidateViewModel()
        {
            return new ValidationResult[] { };
        }
    }
}
