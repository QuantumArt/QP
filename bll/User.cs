using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
    public class User : EntityObject
    {
        internal static User Create() => new User { Groups = Enumerable.Empty<UserGroup>() };

        public User()
        {
            _contentDefaultFilters = new InitPropertyValue<IEnumerable<UserDefaultFilter>>(() => UserRepository.GetContentDefaultFilters(Id));
            EnableContentGroupingInTree = true;
        }

        [Display(Name = "Login", ResourceType = typeof(UserStrings))]
        [Required(ErrorMessageResourceName = "LoginNotEntered", ErrorMessageResourceType = typeof(UserStrings))]
        [StringLength(30, ErrorMessageResourceName = "LoginLengthExceeded", ErrorMessageResourceType = typeof(UserStrings))]
        [RegularExpression(RegularExpressions.UserName, ErrorMessageResourceName = "LoginInvalidFormat", ErrorMessageResourceType = typeof(UserStrings))]
        public string LogOn { get; set; }

        [Display(Name = "Password", ResourceType = typeof(UserStrings))]
        public string Password { get; set; }

        [Display(Name = "NewPassword", ResourceType = typeof(UserStrings))]
        public string NewPassword { get; set; }

        [Display(Name = "NewPasswordCopy", ResourceType = typeof(UserStrings))]
        public string NewPasswordCopy { get; set; }

        [Display(Name = "Disabled", ResourceType = typeof(UserStrings))]
        public bool Disabled { get; set; }

        [Required(ErrorMessageResourceName = "FirstNameNotEntered", ErrorMessageResourceType = typeof(UserStrings))]
        [StringLength(255, ErrorMessageResourceName = "FirstNameMaxLengthExceeded", ErrorMessageResourceType = typeof(UserStrings))]
        [Display(Name = "FirstName", ResourceType = typeof(UserStrings))]
        public string FirstName { get; set; }

        [Required(ErrorMessageResourceName = "LastNameNotEntered", ErrorMessageResourceType = typeof(UserStrings))]
        [StringLength(255, ErrorMessageResourceName = "LastNameMaxLengthExceeded", ErrorMessageResourceType = typeof(UserStrings))]
        [Display(Name = "LastName", ResourceType = typeof(UserStrings))]
        public string LastName { get; set; }

        [Required(ErrorMessageResourceName = "EmailNotEntered", ErrorMessageResourceType = typeof(UserStrings))]
        [StringLength(255, ErrorMessageResourceName = "EmailMaxLengthExceeded", ErrorMessageResourceType = typeof(UserStrings))]
        [EmailAddress(ErrorMessageResourceName = "EmailInvalidFormat", ErrorMessageResourceType = typeof(UserStrings))]
        [Display(Name = "Email", ResourceType = typeof(UserStrings))]
        public string Email { get; set; }

        [Display(Name = "AutoLogOn", ResourceType = typeof(UserStrings))]
        public bool AutoLogOn { get; set; }

        [Display(Name = "NtLogin", ResourceType = typeof(UserStrings))]
        [StringLength(255, ErrorMessageResourceName = "NtLoginLengthExceeded", ErrorMessageResourceType = typeof(UserStrings))]
        [RegularExpression(RegularExpressions.UserName, ErrorMessageResourceName = "NtLoginInvalidFormat", ErrorMessageResourceType = typeof(UserStrings))]
        public string NtLogOn { get; set; }

        [Display(Name = "MustChangePassword", ResourceType = typeof(UserStrings))]
        public bool MustChangePassword { get; set; }

        [Display(Name = "EnableContentGroupingInTree", ResourceType = typeof(UserStrings))]
        public bool EnableContentGroupingInTree { get; set; }

        public DateTime? LastLogOn { get; set; }

        public bool Subscribed { get; set; }

        [Display(Name = "Language", ResourceType = typeof(UserStrings))]
        public int LanguageId { get; set; }

        public bool VMode { get; set; }

        public byte[] AdSid { get; set; }

        [Display(Name = "AllowStageEditField", ResourceType = typeof(UserStrings))]
        public bool AllowStageEditField { get; set; }

        [Display(Name = "AllowStageEditObject", ResourceType = typeof(UserStrings))]
        public bool AllowStageEditObject { get; set; }

        public bool BuiltIn { get; set; }

        public DateTime PasswordModified { get; set; }

        public override string EntityTypeCode => Constants.EntityTypeCode.User;

        [ValidateNever]
        public override string Name => LogOn;

        public override string Description => DisplayName;

        public string FullName => FirstName + " " + LastName;

        /// <summary>
        /// Имя пользователя для отображения в интерфейсе
        /// </summary>
        public string DisplayName
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append(FullName);
                if (You)
                {
                    sb.AppendFormat(" ({0})", GlobalStrings.You);
                }
                return sb.ToString();
            }
        }

        public bool You => QPContext.CurrentUserId == Id;

        [BindNever]
        [ValidateNever]
        public IEnumerable<UserGroup> Groups { get; set; }

        private readonly InitPropertyValue<IEnumerable<UserDefaultFilter>> _contentDefaultFilters;

        /// <summary>
        /// Фильтры по умолчанию
        /// </summary>
        public IEnumerable<UserDefaultFilter> ContentDefaultFilters
        {
            get => _contentDefaultFilters.Value;
            set => _contentDefaultFilters.Value = value;
        }

        private static bool IsPasswordComplex(string password)
        {
            string[] patterns = { @"[0-9]", @"[a-z]", @"[A-Z]", @"[~!@#$%^&*_+]" };
            var count = patterns.Count(pattern => Regex.IsMatch(password, pattern));
            return count >= 3;
        }

        public void ProfileValidate()
        {
            var errors = new RulesException<User>();
            base.Validate(errors);


            if (MustChangePassword)
            {
                if (string.IsNullOrEmpty(NewPassword))
                {
                    errors.ErrorFor(u => u.NewPassword, UserStrings.NewPasswordNotEntered);
                }
                if (string.IsNullOrEmpty(NewPasswordCopy))
                {
                    errors.ErrorFor(u => u.NewPasswordCopy, UserStrings.NewPasswordCopyNotEntered);
                }
            }

            if (!string.IsNullOrEmpty(NewPassword))
            {
                if (NewPassword.Length < 7 || NewPassword.Length > 20)
                {
                    errors.ErrorFor(u => u.NewPassword, string.Format(UserStrings.PasswordLengthExceeded, 7, 20));
                }

                if (NewPassword.Contains(LogOn) || LogOn.Contains(NewPassword))
                {
                    errors.ErrorFor(u => u.LogOn, UserStrings.PasswordContainsLogin);
                }

                if (!IsPasswordComplex(NewPassword))
                {
                    errors.ErrorFor(u => u.NewPassword, UserStrings.PasswordNotComplex);
                }

                if (CheckMatchingPassword(NewPassword, Id))
                {
                    errors.ErrorFor(u => u.NewPassword, UserStrings.NewPasswordMatchCurrentPassword);
                }

                if (NewPassword != NewPasswordCopy)
                {
                    errors.ErrorFor(u => u.NewPasswordCopy, UserStrings.NewPasswordAndCopyDoesntMatch);
                }
            }

            if (!errors.IsEmpty)
            {
                throw errors;
            }
        }

        public static bool CheckMatchingPassword(string newPassword, int userId)
        {
            return UserRepository.NewPasswordMathCurrentPassword(userId, newPassword);
        }

        public override void DoCustomBinding()
        {
            VMode = false;
            Subscribed = true;
            Password = !string.IsNullOrEmpty(NewPassword) ? NewPassword : string.Empty;
        }

        public override void Validate()
        {
            var errors = new RulesException<User>();
            base.Validate(errors);
            if (IsNew)
            {
                if (string.IsNullOrEmpty(NewPassword))
                {
                    errors.ErrorFor(u => u.NewPassword, UserStrings.NewPasswordNotEntered);
                }
            }
            // Пользователь не может быть одновременно добавлен в группу использующую параллельный Workflow ,в группу "Администраторы" или в ее потомков
            if (Groups.Any(g => g.UseParallelWorkflow))
            {
                var groupIDs = Groups.Select(g => g.Id);
                var userInAdminDescGroups =
                    UserGroupRepository.GetAdministratorsHierarhy()
                        .Select(r => Convert.ToInt32(r["CHILD"]))
                        .Intersect(groupIDs)
                        .Any();
                if (userInAdminDescGroups)
                {
                    errors.ErrorForModel(UserStrings.UserCouldntBindToWFAndAdminDescGroups);
                }
            }

            if (!string.IsNullOrEmpty(NewPassword))
            {
                if (NewPassword.Length < 7 || NewPassword.Length > 20)
                {
                    errors.ErrorFor(u => u.NewPassword, string.Format(UserStrings.PasswordLengthExceeded, 7, 20));
                }

                if (NewPassword.Contains(LogOn) || LogOn.Contains(NewPassword))
                {
                    errors.ErrorFor(u => u.LogOn, UserStrings.PasswordContainsLogin);
                }

                if (!IsPasswordComplex(NewPassword))
                {
                    errors.ErrorFor(u => u.NewPassword, UserStrings.PasswordNotComplex);
                }

                if (CheckMatchingPassword(NewPassword, Id))
                {
                    errors.ErrorFor(u => u.NewPassword, UserStrings.NewPasswordMatchCurrentPassword);
                }

                if (NewPassword != NewPasswordCopy)
                {
                    errors.ErrorFor(u => u.NewPasswordCopy, UserStrings.NewPasswordAndCopyDoesntMatch);
                }
            }

            if (!errors.IsEmpty)
            {
                throw errors;
            }
        }

        public override string PropertyIsNotUniqueMessage => UserStrings.LoginNotUnique;

        public override string UniquePropertyName
        {
            get
            {
                Expression<Func<object>> f = () => LogOn;
                return f.GetPropertyName();
            }
        }

        internal void MutateLogin()
        {
            var login = LogOn;
            var index = 0;

            do
            {
                index++;
                LogOn = MutateHelper.MutateUserLogin(login, index);
            } while (EntityObjectRepository.CheckNameUniqueness(this));
        }
    }
}
