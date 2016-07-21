using System;
using System.Collections.Generic;
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

        #region private fields

        #endregion

        #region properties

        #region simple read-write
            
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
            public virtual string Name
            {
                get;
                set;
            }

            /// <summary>
            /// описание сущности
            /// </summary>
            [LocalizedDisplayName("Description", NameResourceType = typeof(EntityObjectStrings))]
            [MaxLengthValidator(512, MessageTemplateResourceName = "DescriptionMaxLengthExceeded", MessageTemplateResourceType = typeof(EntityObjectStrings))]
            public virtual string Description
            {
                get;
                set;
            }

            /// <summary>
            /// дата создания сущности
            /// </summary>
            [LocalizedDisplayName("Created", NameResourceType = typeof(EntityObjectStrings))]
            public DateTime Created
            {
                get;
                set;
            }

            /// <summary>
            /// дата последнего изменения сущности
            /// </summary>
            [LocalizedDisplayName("Modified", NameResourceType = typeof(EntityObjectStrings))]
            public virtual DateTime Modified
            {
                get;
                set;
            }

            /// <summary>
            /// идентификатор пользователя, который последним редактировал сущность
            /// </summary>
            public int LastModifiedBy
            {
                get;
                set;
            }

            /// <summary>
            /// является ли сущность только для чтения
            /// </summary>
            public virtual bool IsReadOnly
            {
                get;
                set;
            }


            /// <summary>
            /// Родительский Id для проверки уникальности
            /// </summary>
            public virtual int ParentEntityId
            {
                get
                {
                    return 0;
                }
                set { }
            }
        #endregion

        #region simple read-only

            /// <summary>
            /// идентификатор пользователя, который последним редактировал сущность
            /// </summary>
            [LocalizedDisplayName("LastModifiedBy", NameResourceType = typeof(EntityObjectStrings))]
            public string LastModifiedByUserToDisplay
            {
                get
                {
                    return (LastModifiedByUser == null) ? String.Empty : LastModifiedByUser.DisplayName;
                }
            }

            /// <summary>
            /// Дата создания сущности для отображения в форме
            /// </summary>
            [LocalizedDisplayName("Created", NameResourceType = typeof(EntityObjectStrings))]
            public string CreatedToDisplay
            {
                get
                {
                    return Created.ValueToDisplay();
                }
            }

            /// <summary>
            /// Дата последнего изменения сущности для отображения в форме
            /// </summary>
            [LocalizedDisplayName("Modified", NameResourceType = typeof(EntityObjectStrings))]
            public string ModifiedToDisplay
            {
                get
                {
                    return Modified.ValueToDisplay();
                }
            }

            /// <summary>
            /// Признак, сохранена ли сущность в БД
            /// </summary>
            public bool IsNew => (Id == 0);

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

        #endregion

        #region references

            /// <summary>
            /// информация о пользователе, который последним редактировал сущность
            /// </summary>		
            public virtual User LastModifiedByUser
            {
                get;
                set;
            }	
        
            /// <summary>
            /// Виртуальный и физический пути, связанные с сущностью
            /// </summary>		
            public virtual PathInfo PathInfo => new PathInfo { Path = String.Empty, Url = String.Empty };

            public virtual IEnumerable<EntityObject> Children { get; set; }

            public virtual bool HasChildren { get; set; }
        
            public virtual EntityObject Parent => null;

        #endregion

        #endregion

        #region methods


        public virtual void Validate()
        {
            RulesException errors = new RulesException<EntityObject>();

            Validate(errors);

            if (!errors.IsEmpty)
                throw errors;
        }

        protected virtual void Validate(RulesException errors)
        {
            EntLibValidate(errors, this);
            ValidateSecurity(errors);
            ValidateUnique(errors);
        }

        protected virtual void ValidateUnique(RulesException errors)
        {
            if (!String.IsNullOrEmpty(Name))
            {
                if (EntityObjectRepository.CheckNameUniqueness(this))
                    errors.Error(UniquePropertyName, Name, PropertyIsNotUniqueMessage);
            }
        }

        /// <summary>
        /// Доступно ли для сущности выполнение заданного типа действия (по Security)
        /// </summary>
        public bool IsAccessible(string code)
        {
            return SecurityRepository.IsEntityAccessible(EntityTypeCode, Id, code);
        }

       
        /// <summary>
        /// Action для операции "Save and Close"
        /// Если IsNew то Action с типом 'save' для типа сущности
        /// Если NotNew то Action с типом 'update' для типа сущности
        /// Если пользователь не имеет доступа к Action вернет null
        /// </summary>
        public BackendAction SaveAndCloseAction
        {
            get
            {
                BackendAction action;
                if (IsNew)
                    action = BackendActionRepository.GetAction(EntityTypeCode, ActionTypeCode.Save);
                else
                    action = BackendActionRepository.GetAction(EntityTypeCode, ActionTypeCode.Update);
                return (action != null && SecurityRepository.IsActionAccessible(action.Code)) ? action : null;
            }
        }

        internal static string TranslateSortExpression(string sortExpression)
        {
            Dictionary<string, string> replaces = new Dictionary<string, string>
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
                throw new InvalidOperationException($"Attempt to insert entity (key = '{entityTypeCode}', ID = 0)");
            }
        }

        public void VerifyIdentityInserting(string entityTypeCode)
        {
            VerifyIdentityInserting(entityTypeCode, Id, ForceId);
        }

        #region override
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Id != 0 && Id == ((EntityObject)obj).Id;
        }

        public override int GetHashCode()
        {
            return Id == 0 ? base.GetHashCode() : Id;
        }

        #endregion

        #region private

        protected virtual void ValidateSecurity(RulesException errors)
        {
            if (IsNew)
            {
                if (Parent != null && !Parent.IsUpdatable)
                    errors.CriticalErrorForModel(CannotAddBecauseOfSecurityMessage);
            }
            else
            {
                if (!IsUpdatable)
                    errors.CriticalErrorForModel(CannotUpdateBecauseOfSecurityMessage);
            }

        }

        private static void EntLibValidate(RulesException ex, object obj)
        {
            Validator validator = ValidationFactory.CreateValidator(obj.GetType());
            ValidationResults results = validator.Validate(obj);
            SaveResults(ex, results);
        }

        private static void SaveResults(RulesException ex, IEnumerable<ValidationResult> validationResults)
        {
            if (validationResults != null)
            {
                foreach (ValidationResult validationResult in validationResults)
                {
                    if (validationResult.NestedValidationResults != null)
                    {
                        SaveResults(ex, validationResult.NestedValidationResults);
                    }
                    ex.Error(validationResult.Key, String.Empty, validationResult.Message);
                }
            }
        }


        #endregion

        #endregion

    }
}