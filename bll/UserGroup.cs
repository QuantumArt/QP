using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL
{
    public class UserGroup : EntityObject
    {
        private static readonly int ADMIN_GROUP_ID = 1;

        public override string EntityTypeCode => Constants.EntityTypeCode.UserGroup;

        #region Properties

        public bool BuiltIn { get; set; }

        [Display(Name = "SharedArticles", ResourceType = typeof(UserGroupStrings))]
        public bool SharedArticles { get; set; }

        [Display(Name = "UseParallelWorkflow", ResourceType = typeof(UserGroupStrings))]
        public bool UseParallelWorkflow { get; set; }

        [Display(Name = "CanUnlockItems", ResourceType = typeof(UserGroupStrings))]
        public bool CanUnlockItems { get; set; }

        [StringLength(255, ErrorMessageResourceName = "NtGroupLengthExceeded", ErrorMessageResourceType = typeof(UserGroupStrings))]
        [RegularExpression(RegularExpressions.UserName, ErrorMessageResourceName = "NtGroupInvalidFormat", ErrorMessageResourceType = typeof(UserGroupStrings))]
        [Display(Name = "NtGroup", ResourceType = typeof(UserGroupStrings))]
        public string NtGroup { get; set; }

        [BindNever]
        [ValidateNever]
        public IEnumerable<User> Users { get; set; }

        [BindNever]
        [ValidateNever]
        public UserGroup ParentGroup { get; set; }

        [BindNever]
        [ValidateNever]
        public IEnumerable<UserGroup> ChildGroups { get; set; }

        #endregion

        #region Validation

        public override void Validate()
        {
            var errors = new RulesException<UserGroup>();
            base.Validate(errors);

            // Группа не может иметь родительскую группу с параллельным Worfklow
            if (ParentGroup != null && ParentGroup.UseParallelWorkflow)
            {
                errors.ErrorFor(g => g.ParentGroup, UserGroupStrings.ParentCouldntUseWorkflow);
            }

            // Если группа с параллельным Worfklow, то она не может иметь потомков
            if (UseParallelWorkflow && ChildGroups.Any())
            {
                errors.ErrorFor(g => g.UseParallelWorkflow, UserGroupStrings.GroupCouldntUseWorkflow);
            }

            // Если группа с параллельным Worfklow то проверить, не являеться ли родительская группа потомком группы Администраторы
            if (UseParallelWorkflow &&
                (
                    IsAdministrators || ParentGroup != null && ParentGroup.IsAdminDescendant
                )
            )
            {
                errors.ErrorForModel(UserGroupStrings.GroupCouldntBeAdminDescendant);
            }

            // проверка на циклы
            if (!IsNew && ParentGroup != null)
            {
                if (UserGroupRepository.IsCyclePossible(Id, ParentGroup.Id))
                {
                    errors.ErrorFor(g => g.ParentGroup, UserGroupStrings.IsGroupCycle);
                }
            }

            // Группа с параллельным Worfklow не может содержать пользователей прямо или косвенно входящих в группу Администраторы
            AdminDescendantGroupUserValidation(errors);

            // Если группа является потомком группы "Администраторы", то она не может содержать пользователей входящих в группы использующие параллельный Workflow
            WorkflowGroupUsersInAdminDescendantValidation(errors);

            // Проверка на то, что нельзя удалять встроенных пользователей из встроенной группы
            BuiltInUsersRemovingValidation(errors);

            if (!errors.IsEmpty)
            {
                throw errors;
            }
        }

        /// <summary>
        /// Если группа является потомком группы "Администраторы", то она не может содержать пользователей входящих в группы использующие параллельный Workflow
        /// </summary>
        /// <param name="errors"></param>
        private void WorkflowGroupUsersInAdminDescendantValidation(RulesException<UserGroup> errors)
        {
            if (IsAdminDescendant && Users.Any())
            {
                var workflowGroupUsersIDs = UserGroupRepository.SelectWorkflowGroupUserIDs(Users.Select(u => u.Id).ToArray());
                if (workflowGroupUsersIDs.Any())
                {
                    var logins = Users
                        .Where(u => workflowGroupUsersIDs.Contains(u.Id))
                        .Select(u => u.LogOn);
                    var message = string.Format(UserGroupStrings.GroupCouldntBindWorkflowGroupUsers, string.Join(",", logins));
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
                var adminDescendantUsersIDs = UserGroupRepository.SelectAdminDescendantGroupUserIDs(Users.Select(u => u.Id).ToArray(), Id);
                if (adminDescendantUsersIDs.Any())
                {
                    var logins = Users
                        .Where(u => adminDescendantUsersIDs.Contains(u.Id))
                        .Select(u => u.LogOn);
                    var message = string.Format(UserGroupStrings.GroupCouldntBindAdminDescendantUsers, string.Join(",", logins));
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
                var group = UserGroupRepository.GetPropertiesById(Id);
                if (group == null)
                {
                    throw new ApplicationException(string.Format(UserGroupStrings.GroupNotFound, Id));
                }

                var dbBuiltInUserIDs = group.Users.Where(u => u.BuiltIn).Select(u => u.Id);
                var builtInUserIDs = Users.Where(u => u.BuiltIn).Select(u => u.Id);
                var undindedBuiltInUserIDs = dbBuiltInUserIDs.Except(builtInUserIDs);
                if (undindedBuiltInUserIDs.Any())
                {
                    var logins = group.Users
                        .Where(u => undindedBuiltInUserIDs.Contains(u.Id))
                        .Select(u => u.LogOn);
                    var message = string.Format(UserGroupStrings.BuiltInUsersCouldntBeRemoved, string.Join(",", logins));
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
                {
                    return ParentGroup != null && ParentGroup.IsAdminDescendant;
                }

                return UserGroupRepository.IsGroupAdminDescendant(Id);
            }
        }

        public bool IsAdministrators => Id == ADMIN_GROUP_ID;

        #endregion

        internal static UserGroup Create() => new UserGroup
        {
            ChildGroups = Enumerable.Empty<UserGroup>(),
            Users = Enumerable.Empty<User>()
        };

        internal void MutateName()
        {
            var name = Name;
            var index = 0;
            do
            {
                index++;
                Name = MutateHelper.MutateString(name, index);
            } while (EntityObjectRepository.CheckNameUniqueness(this));
        }
    }
}
