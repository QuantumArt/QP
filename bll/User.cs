using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.Practices.Unity;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{
    [HasSelfValidation]
    public class User : EntityObject
    {
        internal static User Create() => new User { Groups = Enumerable.Empty<UserGroup>() };

        public User()
        {
            _contentDefaultFilters = new InitPropertyValue<IEnumerable<UserDefaultFilter>>(() => QPContext.CurrentUnityContainer.Resolve<IUserService>().GetContentDefaultFilters(Id));
            EnableContentGroupingInTree = true;
        }

        [LocalizedDisplayName("Login", NameResourceType = typeof(UserStrings))]
        [RequiredValidator(MessageTemplateResourceName = "LoginNotEntered", MessageTemplateResourceType = typeof(UserStrings))]
        [MaxLengthValidator(30, MessageTemplateResourceName = "LoginLengthExceeded", MessageTemplateResourceType = typeof(UserStrings))]
        [FormatValidator(RegularExpressions.InvalidUserName, Negated = true, MessageTemplateResourceName = "NameInvalidFormat", MessageTemplateResourceType = typeof(UserStrings))]
        public string LogOn { get; set; }

        [LocalizedDisplayName("Password", NameResourceType = typeof(UserStrings))]
        public string Password { get; set; }

        [LocalizedDisplayName("OldPassword", NameResourceType = typeof(UserStrings))]
        public string OldPassword { get; set; }

        [LocalizedDisplayName("NewPassword", NameResourceType = typeof(UserStrings))]
        public string NewPassword { get; set; }

        [LocalizedDisplayName("NewPasswordCopy", NameResourceType = typeof(UserStrings))]
        public string NewPasswordCopy { get; set; }

        [LocalizedDisplayName("Disabled", NameResourceType = typeof(UserStrings))]
        public bool Disabled { get; set; }

        [RequiredValidator(MessageTemplateResourceName = "FirstNameNotEntered", MessageTemplateResourceType = typeof(UserStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "FirstNameMaxLengthExceeded", MessageTemplateResourceType = typeof(UserStrings))]
        [LocalizedDisplayName("FirstName", NameResourceType = typeof(UserStrings))]
        public string FirstName { get; set; }

        [RequiredValidator(MessageTemplateResourceName = "LastNameNotEntered", MessageTemplateResourceType = typeof(UserStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "LastNameMaxLengthExceeded", MessageTemplateResourceType = typeof(UserStrings))]
        [LocalizedDisplayName("LastName", NameResourceType = typeof(UserStrings))]
        public string LastName { get; set; }

        [RequiredValidator(MessageTemplateResourceName = "EmailNotEntered", MessageTemplateResourceType = typeof(UserStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "EmailMaxLengthExceeded", MessageTemplateResourceType = typeof(UserStrings))]
        [FormatValidator(RegularExpressions.Email, Negated = false, MessageTemplateResourceName = "EmailInvalidFormat", MessageTemplateResourceType = typeof(UserStrings))]
        [LocalizedDisplayName("Email", NameResourceType = typeof(UserStrings))]
        public string Email { get; set; }

        [LocalizedDisplayName("AutoLogOn", NameResourceType = typeof(UserStrings))]
        public bool AutoLogOn { get; set; }

        [LocalizedDisplayName("NtLogin", NameResourceType = typeof(UserStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "NtLoginLengthExceeded", MessageTemplateResourceType = typeof(UserStrings))]
        [FormatValidator(RegularExpressions.InvalidUserName, Negated = true, MessageTemplateResourceName = "NtLoginInvalidFormat", MessageTemplateResourceType = typeof(UserStrings))]
        public string NtLogOn { get; set; }

        [LocalizedDisplayName("EnableContentGroupingInTree", NameResourceType = typeof(UserStrings))]
        public bool EnableContentGroupingInTree { get; set; }

        public DateTime? LastLogOn { get; set; }

        public bool Subscribed { get; set; }

        [LocalizedDisplayName("Language", NameResourceType = typeof(UserStrings))]
        public int LanguageId { get; set; }

        public bool VMode { get; set; }

        public byte[] AdSid { get; set; }

        [LocalizedDisplayName("AllowStageEditField", NameResourceType = typeof(UserStrings))]
        public bool AllowStageEditField { get; set; }

        [LocalizedDisplayName("AllowStageEditObject", NameResourceType = typeof(UserStrings))]
        public bool AllowStageEditObject { get; set; }

        public bool BuiltIn { get; set; }

        public DateTime PasswordModified { get; set; }

        public override string EntityTypeCode => Constants.EntityTypeCode.User;

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
            if (!string.IsNullOrEmpty(NewPassword))
            {
                if (string.IsNullOrEmpty(OldPassword))
                {
                    errors.ErrorFor(u => u.OldPassword, UserStrings.OldPasswordNotEntered);
                }
                else if (!UserRepository.CheckAuthenticate(LogOn, OldPassword))
                {
                    errors.ErrorFor(u => u.OldPassword, UserStrings.OldPasswordIncorrect);
                }

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

        public void DoCustomBinding()
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
                    errors.ErrorFor(u => u.NewPassword, UserStrings.PasswordNotEntered);
                }
            }

            // Пользователь не может быть одновременно добавлен в группу использующую параллельный Workflow ,в группу "Администраторы" или в ее потомков
            if (Groups.Any(g => g.UseParallelWorkflow))
            {
                var groupIDs = Groups.Select(g => g.Id);
                var userInAdminDescGroups =
                    UserGroupRepository.GetAdministratorsHierarhy()
                    .Select(r => Convert.ToInt32(r.Field<decimal>("CHILD")))
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
            }
            while (EntityObjectRepository.CheckNameUniqueness(this));
        }
    }
}
