using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Data;
using Quantumart.QP8;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Quantumart.QP8.Validators;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Quantumart.QP8.Utils;
using System.Linq.Expressions;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Microsoft.Practices.Unity;

namespace Quantumart.QP8.BLL
{
    [HasSelfValidation]
    public class User : EntityObject
    {
        internal static User Create()
        {
            User user = new User();
            user.Groups = Enumerable.Empty<UserGroup>();
            return user;
        }

        public User()
        {
            contentDefaultFilters = new InitPropertyValue<IEnumerable<UserDefaultFilter>>(() => QPContext.CurrentUnityContainer.Resolve<IUserService>().GetContentDefaultFilters(this.Id));
            EnableContentGroupingInTree = true;
        }

        [LocalizedDisplayName("Login", NameResourceType = typeof(UserStrings))]
        [RequiredValidator(MessageTemplateResourceName = "LoginNotEntered", MessageTemplateResourceType = typeof(UserStrings))]
        [MaxLengthValidator(30, MessageTemplateResourceName = "LoginLengthExceeded", MessageTemplateResourceType = typeof(UserStrings))]
        [FormatValidator(Constants.RegularExpressions.InvalidUserName, Negated = true, MessageTemplateResourceName = "NameInvalidFormat", MessageTemplateResourceType = typeof(UserStrings))]
        public string LogOn
        {
            get;
            set;
        }
        
        [LocalizedDisplayName("Password", NameResourceType = typeof(UserStrings))]
        public string Password
        {
            get;
            set;
        }

        [LocalizedDisplayName("OldPassword", NameResourceType = typeof(UserStrings))]
        public string OldPassword
        {
            get;
            set;
        }

        [LocalizedDisplayName("NewPassword", NameResourceType = typeof(UserStrings))]
        public string NewPassword
        {
            get;
            set;
        }

        [LocalizedDisplayName("NewPasswordCopy", NameResourceType = typeof(UserStrings))]
        public string NewPasswordCopy
        {
            get;
            set;
        }

        [LocalizedDisplayName("Disabled", NameResourceType = typeof(UserStrings))]
        public bool Disabled
        {
            get;
            set;
        }

        [RequiredValidator(MessageTemplateResourceName = "FirstNameNotEntered", MessageTemplateResourceType = typeof(UserStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "FirstNameMaxLengthExceeded", MessageTemplateResourceType = typeof(UserStrings))]
        [LocalizedDisplayName("FirstName", NameResourceType = typeof(UserStrings))]
        public string FirstName
        {
            get;
            set;
        }

        [RequiredValidator(MessageTemplateResourceName = "LastNameNotEntered", MessageTemplateResourceType = typeof(UserStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "LastNameMaxLengthExceeded", MessageTemplateResourceType = typeof(UserStrings))]
        [LocalizedDisplayName("LastName", NameResourceType = typeof(UserStrings))]
        public string LastName
        {
            get;
            set;
        }

        [RequiredValidator(MessageTemplateResourceName = "EmailNotEntered", MessageTemplateResourceType = typeof(UserStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "EmailMaxLengthExceeded", MessageTemplateResourceType = typeof(UserStrings))]
        [FormatValidator(Constants.RegularExpressions.Email, Negated = false, MessageTemplateResourceName = "EmailInvalidFormat", MessageTemplateResourceType = typeof(UserStrings))]
        [LocalizedDisplayName("Email", NameResourceType = typeof(UserStrings))]
        public string Email
        {
            get;
            set;
        }

        [LocalizedDisplayName("AutoLogOn", NameResourceType = typeof(UserStrings))]
        public bool AutoLogOn
        {
            get;
            set;
        }

        [LocalizedDisplayName("NtLogin", NameResourceType = typeof(UserStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "NtLoginLengthExceeded", MessageTemplateResourceType = typeof(UserStrings))]
        [FormatValidator(Constants.RegularExpressions.InvalidUserName, Negated = true, MessageTemplateResourceName = "NtLoginInvalidFormat", MessageTemplateResourceType = typeof(UserStrings))]
        public string NtLogOn
        {
            get;
            set;
        }

        [LocalizedDisplayName("EnableContentGroupingInTree", NameResourceType = typeof(UserStrings))]
        public bool EnableContentGroupingInTree
        {
            get;
            set;
        }


        public DateTime? LastLogOn
        {
            get;
            set;
        }


        public bool Subscribed
        {
            get;
            set;
        }


        [LocalizedDisplayName("Language", NameResourceType = typeof(UserStrings))]
        public int LanguageId
        {
            get;
            set;
        }


        public bool VMode
        {
            get;
            set;
        }

        public Byte[] AdSid
        {
            get;
            set;
        }

        [LocalizedDisplayName("AllowStageEditField", NameResourceType = typeof(UserStrings))]
        public bool AllowStageEditField
        {
            get;
            set;
        }


        [LocalizedDisplayName("AllowStageEditObject", NameResourceType = typeof(UserStrings))]
        public bool AllowStageEditObject
        {
            get;
            set;
        }


        public bool BuiltIn
        {
            get;
            set;
        }

        public DateTime PasswordModified
        {
            get;
            set;
        }

        public override string EntityTypeCode
        {
            get
            {
                return Constants.EntityTypeCode.User;
            }
        }

        public override string Name
        {
            get
            {
                return LogOn;
            }
        }

        public override string Description
        {
            get
            {
                return DisplayName;
            }
        }

        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
     
     
        /// <summary>
        /// Имя пользователя для отображения в интерфейсе
        /// </summary>
        public string DisplayName
        {
            get 
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(FullName);
                if (You)
                {
                    sb.AppendFormat(" ({0})", GlobalStrings.You);
                }
                return sb.ToString();
            }
        }

        public bool You
        {
            get { return QPContext.CurrentUserId == Id; }
        }

        public IEnumerable<UserGroup> Groups { get; set; }

        InitPropertyValue<IEnumerable<UserDefaultFilter>> contentDefaultFilters = null;
        /// <summary>
        /// Фильтры по умолчанию
        /// </summary>
        public IEnumerable<UserDefaultFilter> ContentDefaultFilters 
        {
            get { return contentDefaultFilters.Value; }
            set { contentDefaultFilters.Value = value; }
        }

        private static bool IsPasswordComplex(string password)
        {
            int count = 0;
            string[] patterns = { @"[0-9]", @"[a-z]", @"[A-Z]", @"[~!@#$%^&*_+]" };
            foreach (string pattern in patterns)
            {
                if (Regex.IsMatch(password, pattern))
                    count++;
            }
            return count >= 3;
        }

        public void ProfileValidate()
        {
            RulesException<User> errors = new RulesException<User>();

            base.Validate(errors);
            
            if (!String.IsNullOrEmpty(NewPassword))
            {
                if (String.IsNullOrEmpty(OldPassword))
                    errors.ErrorFor(u => u.OldPassword, UserStrings.OldPasswordNotEntered);
                else if (!UserRepository.CheckAuthenticate(LogOn, OldPassword))
                    errors.ErrorFor(u => u.OldPassword, UserStrings.OldPasswordIncorrect);

                if (NewPassword.Length < 7 || NewPassword.Length > 20)
                    errors.ErrorFor(u => u.NewPassword, String.Format(UserStrings.PasswordLengthExceeded, 7, 20));
                
                if (NewPassword.Contains(LogOn) || LogOn.Contains(NewPassword))
                    errors.ErrorFor(u => u.LogOn, UserStrings.PasswordContainsLogin);

                if (!IsPasswordComplex(NewPassword))
                    errors.ErrorFor(u => u.NewPassword, UserStrings.PasswordNotComplex);

                if (NewPassword != NewPasswordCopy)
                    errors.ErrorFor(u => u.NewPasswordCopy, UserStrings.NewPasswordAndCopyDoesntMatch);
            }
                
            if (!errors.IsEmpty)
                throw errors;
        }

        public void DoCustomBinding()
        {
            VMode = false;
            Subscribed = true;

            if (!String.IsNullOrEmpty(NewPassword))
                Password = NewPassword;
            else
                Password = "";
        }

        public override void Validate()
        {
            RulesException<User> errors = new RulesException<User>();
            base.Validate(errors);

            if (IsNew)
            {
                if (String.IsNullOrEmpty(NewPassword))
                    errors.ErrorFor(u => u.NewPassword, UserStrings.PasswordNotEntered);
            }

            // Пользователь не может быть одновременно добавлен в группу использующую параллельный Workflow ,в группу "Администраторы" или в ее потомков
            if(Groups.Where(g => g.UseParallelWorkflow).Any())
            {
                IEnumerable<int> groupIDs = Groups.Select(g => g.Id);
                bool userInAdminDescGroups =  
                    UserGroupRepository.GetAdministratorsHierarhy()
                    .Select(r => Convert.ToInt32(r.Field<decimal>("CHILD")))
                    .Intersect(groupIDs)
                    .Any();
                if (userInAdminDescGroups)
                    errors.ErrorForModel(UserStrings.UserCouldntBindToWFAndAdminDescGroups);
            }
            

            if (!String.IsNullOrEmpty(NewPassword))
            {
                if (NewPassword.Length < 7 || NewPassword.Length > 20)
                    errors.ErrorFor(u => u.NewPassword, String.Format(UserStrings.PasswordLengthExceeded, 7, 20));

                if (NewPassword.Contains(LogOn) || LogOn.Contains(NewPassword))
                    errors.ErrorFor(u => u.LogOn, UserStrings.PasswordContainsLogin);

                if (!IsPasswordComplex(NewPassword))
                    errors.ErrorFor(u => u.NewPassword, UserStrings.PasswordNotComplex);

                if (NewPassword != NewPasswordCopy)
                    errors.ErrorFor(u => u.NewPasswordCopy, UserStrings.NewPasswordAndCopyDoesntMatch);
            }

            if (!errors.IsEmpty)
                throw errors;
        }

        public override string PropertyIsNotUniqueMessage
        {
            get
            {
                return UserStrings.LoginNotUnique;
            }
        }

        public override string UniquePropertyName
        {
            get
            {
                Expression<Func<object>> f = (() => this.LogOn);
                return f.GetPropertyName();
            }
        }

        internal void MutateLogin()
        {
            string login = LogOn;
            int index = 0;
            do
            {
                index++;
                LogOn = MutateHelper.MutateUserLogin(login, index);
            }
            while (EntityObjectRepository.CheckNameUniqueness(this));
        }
    }
}
