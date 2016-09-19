using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class UserViewModel : UserViewModelBase
    {
        public static UserViewModel Create(User user, string tabId, int parentId, IUserService service)
        {
            var model = Create<UserViewModel>(user, tabId, parentId);
            model.Service = service;
            model.Init();
            return model;
        }

        public override void Validate(ModelStateDictionary modelState)
        {
            base.Validate(modelState);

            // проверка на то, что удалять builtin пользователей из builtin групп запрещено
            if (!Data.IsNew && Data.BuiltIn)
            {
                var dbUser = Service.ReadProperties(Data.Id);
                var builtinGroups = dbUser.Groups
                    .Where(g => g.BuiltIn && !g.IsReadOnly)
                    .Select(g => g.Id)
                    .Except(Data.Groups.Where(g => g.BuiltIn && !g.IsReadOnly).Select(g => g.Id));

                if (builtinGroups.Any())
                {
                    IsValid = false;
                    var message = string.Format(UserStrings.UnbindBuitInGroup, string.Join(",", Service.GetUserGroups(builtinGroups).Select(g => "\"" + g.Name + "\"")));
                    Expression<Func<object>> f = (() => SelectedGroups);
                    modelState.AddModelError(f.GetPropertyName(), message);
                }
            }
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.User;

        public override string ActionCode => IsNew ? Constants.ActionCode.AddNewUser : Constants.ActionCode.UserProperties;

        public IEnumerable<ListItem> AvailableLanguages => new List<ListItem>
        {
            new ListItem(Constants.LanguageId.English.ToString(), UserStrings.English),
            new ListItem(Constants.LanguageId.Russian.ToString(), UserStrings.Russian)
        };

        /// <summary>
        /// Группы в которые входит пользователь
        /// </summary>
        [LocalizedDisplayName("SelectedGroupIDs", NameResourceType = typeof(UserStrings))]
        public IList<QPCheckedItem> SelectedGroups { get; set; }

        /// <summary>
        /// Возвращает список групп
        /// </summary>
        public IEnumerable<ListItem> GroupListItems
        {
            get
            {
                return Service.GetBindableUserGroups().Select(g => new ListItem{Value = g.Id.ToString(),Text = g.Name}).ToArray();
            }
        }

        /// <summary>
        /// Инициализирует View Model
        /// </summary>
        private void Init()
        {
            SelectedGroups = Data.Groups.Select(g => new QPCheckedItem { Value = g.Id.ToString() }).ToList();
        }

        /// <summary>
        /// Устанавливает свойства модели, которые не могут быть установлены автоматически
        /// </summary>
        internal override void DoCustomBinding()
        {
            Data.DoCustomBinding();
            Data.Groups = SelectedGroups.Any() ? Service.GetUserGroups(Converter.ToInt32Collection(SelectedGroups.Select(g => g.Value).ToArray())) : Enumerable.Empty<UserGroup>();
        }
    }
}
