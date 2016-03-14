using System;
using System.Dynamic;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.ViewModels.DirectLink;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public abstract class EntityViewModel : ViewModel
    {
		
		public EntityObject EntityData { get; set; }

		public string SuccesfulActionCode { get; set; }

		public bool IsValid { get; set; }

		public EntityObject Data
		{
			get { return EntityData; }
			set { EntityData = value; }
		}

		#region creation
			
		public static T Create<T>(EntityObject obj, string tabId, int parentId) where T : EntityViewModel, new()
		{
			T model = ViewModel.Create<T>(tabId, parentId);
			model.EntityData = obj;

			model.IsValid = true;
			return model;
		}

		#endregion

		#region read-only members

		public bool IsNew
		{
			get
			{
				return EntityData.IsNew;
			}
		}

		public override MainComponentType MainComponentType
		{
			get { return MainComponentType.Editor; }
		}

		public override string MainComponentId
		{
			get { return UniqueId("Editor"); }
		}

		public override ExpandoObject MainComponentParameters
		{
			get
			{
				dynamic result = base.MainComponentParameters;
				result.entityId = Id;
				result.entityName = Name;
				result.previousActionCode = SuccesfulActionCode;
				return result;
			}
		}

		public override ExpandoObject MainComponentOptions
		{
			get
			{
				dynamic result = base.MainComponentParameters;
				result.validationSummaryElementId = ValidationSummaryId;
				result.modifiedDateTime = (Data != null && !Data.IsNew) ? Data.Modified.Ticks : new Nullable<long>();
				result.entityTypeAllowedToAutosave = (Data != null && EntityType.CheckToAutosave(Data.EntityTypeCode));

                if (Data != null)
                {
                    BackendAction saveAndCloseAction = Data.SaveAndCloseAction;
                    if (saveAndCloseAction != null)
                        result.saveAndCloseActionCode = saveAndCloseAction.Code;
                }
                return result;
			}
		}

		public override DocumentContextState DocumentContextState
		{
			get
			{
				return String.IsNullOrWhiteSpace(SuccesfulActionCode) ? Constants.DocumentContextState.Loaded : Constants.DocumentContextState.Saved;
			}
		}

		public virtual string Id
		{
			get
			{
				return EntityData.Id.ToString();
			}
		}

		public virtual string Name
		{
			get
			{
				return EntityData.Name;
			}
		}

		public virtual DirectLinkOptions DirectLinkOptions
		{
			get
			{
				int entityId;
				if(!Int32.TryParse(Id, out entityId))
					entityId = 0;
				return new DirectLinkOptions
				{
					actionCode = ActionCode,
					customerCode = QPContext.CurrentCustomerCode,
					entityId = entityId > 0 ? entityId : new Nullable<int>(),
					entityTypeCode = EntityTypeCode,
					parentEntityId = ParentEntityId > 0 ? ParentEntityId : new Nullable<int>()
				};
			}
		}

		#endregion

        public virtual void Validate(ModelStateDictionary modelState)
        {
            try 
			{
				EntityData.Validate();
			}
            catch (RulesException ex)
			{
				ex.CopyTo(modelState, "Data");
				this.IsValid = false;
			}
        }
	}
}