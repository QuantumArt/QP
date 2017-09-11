using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Quantumart.QP8.Validators;
using Quantumart.QP8.Resources;
using System.Data;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.BLL
{
    [HasSelfValidation]
    public class UserGroup : EntityObject
    {
        private static readonly int ADMIN_GROUP_ID = 1;

        public override string EntityTypeCode
        {
            get
            {
                return Constants.EntityTypeCode.UserGroup;
            }
        }

        #region Properties
        public bool BuiltIn { get; set; }

        [LocalizedDisplayName("SharedArticles", NameResourceType = typeof(UserGroupStrings))]
        public bool SharedArticles { get; set; }

        [LocalizedDisplayName("UseParallelWorkflow", NameResourceType = typeof(UserGroupStrings))]
        public bool UseParallelWorkflow { get; set; }

        [LocalizedDisplayName("CanUnlockItems", NameResourceType = typeof(UserGroupStrings))]
        public bool CanUnlockItems { get; set; }

        [MaxLengthValidator(255, MessageTemplateResourceName = "NtGroupLengthExceeded", MessageTemplateResourceType = typeof(UserGroupStrings))]
        [FormatValidator(Constants.RegularExpressions.InvalidUserName, Negated = true, MessageTemplateResourceName = "NtGroupInvalidFormat", MessageTemplateResourceType = typeof(UserGroupStrings))]
        [LocalizedDisplayName("NtGroup", NameResourceType = typeof(UserGroupStrings))]
        public string NtGroup { get; set; }

        public IEnumerable<User> Users { get; set; }

        public UserGroup ParentGroup { get; set; }

        public IEnumerable<UserGroup> ChildGroups { get; set; }
        #endregion

        #region Validation
        public override void Validate()
        {
            RulesException<UserGroup> errors = new RulesException<UserGroup>();
            base.Validate(errors);

            // Группа не может иметь родительскую группу с параллельным Worfklow
            if (ParentGroup != null && ParentGroup.UseParallelWorkflow)
                errors.ErrorFor(g => g.ParentGroup, UserGroupStrings.ParentCouldntUseWorkflow);

            // Если группа с параллельным Worfklow, то она не может иметь потомков
            if (UseParallelWorkflow && ChildGroups.Any())
                errors.ErrorFor(g => g.UseParallelWorkflow, UserGroupStrings.GroupCouldntUseWorkflow);

            // Если группа с параллельным Worfklow то проверить, не являеться ли родительская группа потомком группы Администраторы
            if (UseParallelWorkflow &&
                (
                    IsAdministrators || (ParentGroup != null && ParentGroup.IsAdminDescendant)
                )
            )
            {
                errors.ErrorForModel(UserGroupStrings.GroupCouldntBeAdminDescendant);
            }


            // проверка на циклы 
            if (!IsNew && ParentGroup != null)
            {
                if (UserGroupRepository.IsCyclePossible(Id, ParentGroup.Id))
                    errors.ErrorFor(g => g.ParentGroup, UserGroupStrings.IsGroupCycle);
            }

            // Группа с параллельным Worfklow не может содержать пользователей прямо или косвенно входящих в группу Администраторы
            AdminDescendantGroupUserValidation(errors);

            // Если группа является потомком группы "Администраторы", то она не может содержать пользователей входящих в группы использующие параллельный Workflow
            WorkflowGroupUsersInAdminDescendantValidation(errors);

            // Проверка на то, что нельзя удалять встроенных пользователей из встроенной группы
            BuiltInUsersRemovingValidation(errors);

            if (!errors.IsEmpty)
                throw errors;
        }

        /// <summary>
        /// Если группа является потомком группы "Администраторы", то она не может содержать пользователей входящих в группы использующие параллельный Workflow
        /// </summary>
        /// <param name="errors"></param>
        private void WorkflowGroupUsersInAdminDescendantValidation(RulesException<UserGroup> errors)
        {
            if (IsAdminDescendant && Users.Any())
            {
                IEnumerable<int> workflowGroupUsersIDs = UserGroupRepository.SelectWorkflowGroupUserIDs(Users.Select(u => u.Id).ToArray());
                if (workflowGroupUsersIDs.Any())
                {
                    var logins = Users
                            .Where(u => workflowGroupUsersIDs.Contains(u.Id))
                            .Select(u => u.LogOn);
                    string message = String.Format(UserGroupStrings.GroupCouldntBindWorkflowGroupUsers, String.Join(",", logins));
                    errors.ErrorForModel(message);
                }
            }
        }

        /// <summary>
        /// Группа с параллельным Worfklow не может содержать пользователей прямо или косвенно входящих в группу Администраторы
        /// </summary>
        /// <param name="errors"></param>
        private void AdminDescendantGroupUserValidation(RulesException<UserGroup> errors)
        {
            if (UseParallelWorkflow && Users.Any())
            {
                IEnumerable<int> adminDescendantUsersIDs = UserGroupRepository.SelectAdminDescendantGroupUserIDs(Users.Select(u => u.Id).ToArray(), Id);
                if (adminDescendantUsersIDs.Any())
                {
                    var logins = Users
                            .Where(u => adminDescendantUsersIDs.Contains(u.Id))
                            .Select(u => u.LogOn);
                    string message = String.Format(UserGroupStrings.GroupCouldntBindAdminDescendantUsers, String.Join(",", logins));
                    errors.ErrorForModel(message);
                }
            }
        }

        /// <summary>
        /// Проверка на то, что нельзя удалять встроенных пользователей из встроенной группы
        /// </summary>
        /// <param name="errors"></param>
        private void BuiltInUsersRemovingValidation(RulesException<UserGroup> errors)
        {
            if (BuiltIn)
            {
                UserGroup group = UserGroupRepository.GetPropertiesById(Id);
                if (group == null)
                    throw new ApplicationException(String.Format(UserGroupStrings.GroupNotFound, Id));

                IEnumerable<int> dbBuiltInUserIDs = group.Users.Where(u => u.BuiltIn).Select(u => u.Id);
                IEnumerable<int> builtInUserIDs = this.Users.Where(u => u.BuiltIn).Select(u => u.Id);
                IEnumerable<int> undindedBuiltInUserIDs = dbBuiltInUserIDs.Except(builtInUserIDs);
                if (undindedBuiltInUserIDs.Any())
                {
                    var logins = group.Users
                            .Where(u => undindedBuiltInUserIDs.Contains(u.Id))
                            .Select(u => u.LogOn);
                    string message = String.Format(UserGroupStrings.BuiltInUsersCouldntBeRemoved, String.Join(",", logins));
                    errors.ErrorForModel(message);
                }
            }
        }

        /// <summary>
        /// Является ли группа потомком группы Администраторы 
        /// </summary>
        internal bool IsAdminDescendant
        {
            get
            {
                if (IsNew)
                    return (ParentGroup != null && ParentGroup.IsAdminDescendant);
                else
                    return UserGroupRepository.IsGroupAdminDescendant(Id);
            }
        }

        public bool IsAdministrators
        {
            get { return Id == ADMIN_GROUP_ID; }
        }

        #endregion

        internal static UserGroup Create()
        {
            return new UserGroup
            {
                ChildGroups = Enumerable.Empty<UserGroup>(),
                Users = Enumerable.Empty<User>()
            };
        }

        internal void MutateName()
        {
            string name = Name;
            int index = 0;
            do
            {
                index++;
                Name = MutateHelper.MutateString(name, index);
            }
            while (EntityObjectRepository.CheckNameUniqueness(this));
        }
    }
}
