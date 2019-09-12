using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.ViewModels.User
{
    public class UserViewModel : UserViewModelBase
    {
        public static UserViewModel Create(BLL.User user, string tabId, int parentId, IUserService service)
        {
            var model = Create<UserViewModel>(user, tabId, parentId);
            model.Service = service;
            model.Init();
            return model;
        }

        public override IEnumerable<ValidationResult> ValidateViewModel()
        {
            if (!Data.IsNew && Data.BuiltIn)
            {
                // проверка на то, что удалять builtin пользователей из builtin групп запрещено
                var dbUser = Service.ReadProperties(Data.Id);
                var builtinGroups = dbUser.Groups
                    .Where(g => g.BuiltIn && !g.IsReadOnly)
                    .Select(g => g.Id)
                    .Except(Data.Groups.Where(g => g.BuiltIn && !g.IsReadOnly).Select(g => g.Id));

                if (builtinGroups.Any())
                {
                    var message = string.Format(UserStrings.UnbindBuitInGroup, string.Join(",", Service.GetUserGroups(builtinGroups).Select(g => "\"" + g.Name + "\"")));
                    Expression<Func<object>> f = () => SelectedGroups;
                    yield return new ValidationResult(message, new []{ "SelectedGroups"});
                }
            }
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.User;

        public override string ActionCode => IsNew ? Constants.ActionCode.AddNewUser : Constants.ActionCode.UserProperties;

        public IEnumerable<ListItem> AvailableLanguages => new List<ListItem>
        {
            new ListItem(LanguageId.English.ToString(), UserStrings.English),
            new ListItem(LanguageId.Russian.ToString(), UserStrings.Russian)
        };

        /// <summary>
        /// Группы в которые входит пользователь
        /// </summary>
        [Display(Name = "SelectedGroupIDs", ResourceType = typeof(UserStrings))]
        public IList<QPCheckedItem> SelectedGroups { get; set; }

        /// <summary>
        /// Возвращает список групп
        /// </summary>
        public IEnumerable<ListItem> GroupListItems
        {
            get { return Service.GetBindableUserGroups().Select(g => new ListItem { Value = g.Id.ToString(), Text = g.Name }).ToArray(); }
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
        public override void DoCustomBinding()
        {
            base.DoCustomBinding();

            SelectedGroups = SelectedGroups?.Where(n => n != null).ToArray() ?? new QPCheckedItem[] { };
            Data.Groups = SelectedGroups.Any() ? Service.GetUserGroups(Converter.ToInt32Collection(SelectedGroups.Select(g => g.Value).ToArray())) : Enumerable.Empty<BLL.UserGroup>();
        }
    }
}
