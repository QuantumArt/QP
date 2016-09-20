using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{
    /// <summary>
    /// Супертип слоя. Содержит общие свойства для сущностей QP8
    /// </summary>
    [HasSelfValidation]
    public abstract class EntityObject : BizObject
    {
        /// <summary>
        /// идентификатор сущности
        /// </summary>
        [LocalizedDisplayName("ID", NameResourceType = typeof(EntityObjectStrings))]
        public int Id { get; set; }

        public int ForceId { get; set; }

        /// <summary>
        /// название сущности
        /// </summary>
        [LocalizedDisplayName("Name", NameResourceType = typeof(EntityObjectStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "NameMaxLengthExceeded", MessageTemplateResourceType = typeof(EntityObjectStrings))]
        [RequiredValidator(MessageTemplateResourceName = "NameNotEntered", MessageTemplateResourceType = typeof(EntityObjectStrings))]
        [FormatValidator(RegularExpressions.InvalidEntityName, Negated = true, MessageTemplateResourceName = "NameInvalidFormat", MessageTemplateResourceType = typeof(EntityObjectStrings))]
        public virtual string Name { get; set; }

        /// <summary>
        /// описание сущности
        /// </summary>
        [LocalizedDisplayName("Description", NameResourceType = typeof(EntityObjectStrings))]
        [MaxLengthValidator(512, MessageTemplateResourceName = "DescriptionMaxLengthExceeded", MessageTemplateResourceType = typeof(EntityObjectStrings))]
        public virtual string Description { get; set; }

        /// <summary>
        /// дата создания сущности
        /// </summary>
        [LocalizedDisplayName("Created", NameResourceType = typeof(EntityObjectStrings))]
        public DateTime Created { get; set; }

        /// <summary>
        /// дата последнего изменения сущности
        /// </summary>
        [LocalizedDisplayName("Modified", NameResourceType = typeof(EntityObjectStrings))]
        public virtual DateTime Modified { get; set; }

        /// <summary>
        /// идентификатор пользователя, который последним редактировал сущность
        /// </summary>
        public int LastModifiedBy { get; set; }

        /// <summary>
        /// является ли сущность только для чтения
        /// </summary>
        public virtual bool IsReadOnly { get; set; }

        /// <summary>
        /// Родительский Id для проверки уникальности
        /// </summary>
        [SuppressMessage("ReSharper", "ValueParameterNotUsed")]
        public virtual int ParentEntityId
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }

        /// <summary>
        /// идентификатор пользователя, который последним редактировал сущность
        /// </summary>
        [LocalizedDisplayName("LastModifiedBy", NameResourceType = typeof(EntityObjectStrings))]
        public string LastModifiedByUserToDisplay => LastModifiedByUser == null ? string.Empty : LastModifiedByUser.DisplayName;

        /// <summary>
        /// Дата создания сущности для отображения в форме
        /// </summary>
        [LocalizedDisplayName("Created", NameResourceType = typeof(EntityObjectStrings))]
        public string CreatedToDisplay => Created.ValueToDisplay();

        /// <summary>
        /// Дата последнего изменения сущности для отображения в форме
        /// </summary>
        [LocalizedDisplayName("Modified", NameResourceType = typeof(EntityObjectStrings))]
        public string ModifiedToDisplay => Modified.ValueToDisplay();

        /// <summary>
        /// Признак, сохранена ли сущность в БД
        /// </summary>
        public bool IsNew => Id == 0;

        /// <summary>
        /// Код сущности (например для проверки уникальности)
        /// </summary>
        public virtual string EntityTypeCode => Constants.EntityTypeCode.None;

        /// <summary>
        /// Родительский Id в иерархических сущностях для проверки уникальности
        /// </summary>
        public virtual int? RecurringId => null;

        /// <summary>
        /// Доступна ли сущность для обновления (по Security)
        /// </summary>
        public bool IsUpdatable => IsAccessible(ActionTypeCode.Update);

        public virtual string CannotAddBecauseOfSecurityMessage => EntityObjectStrings.CannotAddBecauseOfSecurity;

        public virtual string CannotUpdateBecauseOfSecurityMessage => EntityObjectStrings.CannotUpdateBecauseOfSecurity;

        public virtual string PropertyIsNotUniqueMessage => EntityObjectStrings.NameNonUnique;

        public virtual string UniquePropertyName => "Name";

        /// <summary>
        /// информация о пользователе, который последним редактировал сущность
        /// </summary>
        public virtual User LastModifiedByUser { get; set; }

        /// <summary>
        /// Виртуальный и физический пути, связанные с сущностью
        /// </summary>
        public virtual PathInfo PathInfo => new PathInfo
        {
            Path = string.Empty,
            Url = string.Empty
        };

        public virtual IEnumerable<EntityObject> Children { get; set; }

        public virtual bool HasChildren { get; set; }

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

        protected virtual void Validate(RulesException errors)
        {
            EntLibValidate(errors, this);
            ValidateSecurity(errors);
            ValidateUnique(errors);
        }

        protected virtual void ValidateUnique(RulesException errors)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                if (EntityObjectRepository.CheckNameUniqueness(this))
                {
                    errors.Error(UniquePropertyName, Name, PropertyIsNotUniqueMessage);
                }
            }
        }

        public bool IsAccessible(string code)
        {
            return SecurityRepository.IsEntityAccessible(EntityTypeCode, Id, code);
        }

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
                {"ByUser", "ByUser.LogOn"}
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
        public override int GetHashCode()
        {
            return Id == 0 ? base.GetHashCode() : Id;
        }

        protected virtual void ValidateSecurity(RulesException errors)
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

        }

        private static void EntLibValidate(RulesException ex, object obj)
        {
            var validator = ValidationFactory.CreateValidator(obj.GetType());
            var results = validator.Validate(obj);
            SaveResults(ex, results);
        }

        private static void SaveResults(RulesException ex, IEnumerable<ValidationResult> validationResults)
        {
            if (validationResults != null)
            {
                foreach (var validationResult in validationResults)
                {
                    if (validationResult.NestedValidationResults != null)
                    {
                        SaveResults(ex, validationResult.NestedValidationResults);
                    }

                    ex.Error(validationResult.Key, string.Empty, validationResult.Message);
                }
            }
        }
    }
}
