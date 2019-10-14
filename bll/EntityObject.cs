using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NLog;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
    public abstract class EntityObject : BizObject
    {
        protected readonly ILogger CurrentLogger;

        public EntityObject()
        {
            CurrentLogger = LogManager.GetLogger(this.GetType().ToString());
        }

        [Display(Name = "ID", ResourceType = typeof(EntityObjectStrings))]
        public int Id { get; set; }

        public int ForceId { get; set; }

        [Display(Name = "Name", ResourceType = typeof(EntityObjectStrings))]
        [StringLength(255, ErrorMessageResourceName = "NameMaxLengthExceeded", ErrorMessageResourceType = typeof(EntityObjectStrings))]
        [Required(ErrorMessageResourceName = "NameNotEntered", ErrorMessageResourceType = typeof(EntityObjectStrings))]
        [RegularExpression(RegularExpressions.EntityName, ErrorMessageResourceName = "NameInvalidFormat", ErrorMessageResourceType = typeof(EntityObjectStrings))]
        public virtual string Name { get; set; }

        [Display(Name = "Description", ResourceType = typeof(EntityObjectStrings))]
        [StringLength(512, ErrorMessageResourceName = "DescriptionMaxLengthExceeded", ErrorMessageResourceType = typeof(EntityObjectStrings))]
        public virtual string Description { get; set; }

        [Display(Name = "Created", ResourceType = typeof(EntityObjectStrings))]
        public DateTime Created { get; set; }

        [Display(Name = "Modified", ResourceType = typeof(EntityObjectStrings))]
        public virtual DateTime Modified { get; set; }

        public int LastModifiedBy { get; set; }

        public virtual bool IsReadOnly { get; set; }

        [SuppressMessage("ReSharper", "ValueParameterNotUsed")]
        public virtual int ParentEntityId
        {
            get { return 0; }
            set { }
        }

        public virtual void DoCustomBinding()
        {

        }

        [Display(Name = "LastModifiedBy", ResourceType = typeof(EntityObjectStrings))]
        public string LastModifiedByUserToDisplay => LastModifiedByUser == null ? string.Empty : LastModifiedByUser.DisplayName;

        [Display(Name = "Created", ResourceType = typeof(EntityObjectStrings))]
        public string CreatedToDisplay => Created.ValueToDisplay();

        [Display(Name = "Modified", ResourceType = typeof(EntityObjectStrings))]
        public string ModifiedToDisplay => Modified.ValueToDisplay();

        public bool IsNew => Id == 0;

        public virtual string EntityTypeCode => Constants.EntityTypeCode.None;

        public virtual int? RecurringId => null;

        public bool IsUpdatable => IsAccessible(ActionTypeCode.Update);

        public virtual string CannotAddBecauseOfSecurityMessage => EntityObjectStrings.CannotAddBecauseOfSecurity;

        public virtual string CannotUpdateBecauseOfSecurityMessage => EntityObjectStrings.CannotUpdateBecauseOfSecurity;

        public virtual string PropertyIsNotUniqueMessage => EntityObjectStrings.NameNonUnique;

        public virtual string UniquePropertyName => "Name";

        [ValidateNever]
        [BindNever]
        public virtual User LastModifiedByUser { get; set; }

        public virtual PathInfo PathInfo => new PathInfo
        {
            Path = string.Empty,
            Url = string.Empty
        };

        [ValidateNever]
        [BindNever]
        public virtual IEnumerable<EntityObject> Children { get; set; }

        public virtual bool HasChildren { get; set; }

        [ValidateNever]
        [BindNever]
        public virtual EntityObject Parent => null;

        public virtual void Validate()
        {
            var errors = new RulesException<EntityObject>();
            Validate(errors);
            if (!errors.IsEmpty)
            {
                throw errors;
            }
        }

        protected virtual RulesException Validate(RulesException errors)
        {
            ValidateSecurity(errors);
            ValidateUnique(errors);
            return errors;
        }

        protected virtual RulesException ValidateUnique(RulesException errors)
        {
            if (!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(EntityTypeCode))
            {
                if (EntityObjectRepository.CheckNameUniqueness(this))
                {
                    errors.Error(UniquePropertyName, Name, PropertyIsNotUniqueMessage);
                }
            }

            return errors;
        }

        public bool IsAccessible(string code) => SecurityRepository.IsEntityAccessible(EntityTypeCode, Id, code);

        [ValidateNever]
        public BackendAction SaveAndCloseAction
        {
            get
            {
                var action = BackendActionRepository.SaveOrUpdate(EntityTypeCode, IsNew ? ActionTypeCode.Save : ActionTypeCode.Update);
                return action != null && SecurityRepository.IsActionAccessible(action.Code) ? action : null;
            }
        }

        internal static string TranslateSortExpression(string sortExpression)
        {
            var replaces = new Dictionary<string, string>
            {
                { "ByUser", "ByUser.LogOn" }
            };

            return TranslateHelper.TranslateSortExpression(sortExpression, replaces);
        }

        public static void VerifyIdentityInserting(string entityTypeCode, int id, int forceId)
        {
            if (QPConnectionScope.Current.IdentityInsertOptions.Contains(entityTypeCode))
            {
                if (id == 0 && forceId == 0)
                {
                    throw new InvalidOperationException($"Attempt to insert entity (key = '{entityTypeCode}', ID = 0)");
                }
            }
        }

        public void VerifyIdentityInserting(string entityTypeCode)
        {
            VerifyIdentityInserting(entityTypeCode, Id, ForceId);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Id != 0 && Id == ((EntityObject)obj).Id;
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        [SuppressMessage("ReSharper", "BaseObjectGetHashCodeCallInGetHashCode")]
        public override int GetHashCode() => Id == 0 ? base.GetHashCode() : Id;

        protected virtual RulesException ValidateSecurity(RulesException errors)
        {
            if (IsNew)
            {
                if (Parent != null && !Parent.IsUpdatable)
                {
                    errors.CriticalErrorForModel(CannotAddBecauseOfSecurityMessage);
                }
            }
            else
            {
                if (!IsUpdatable)
                {
                    errors.CriticalErrorForModel(CannotUpdateBecauseOfSecurityMessage);
                }
            }

            return errors;
        }
    }
}
