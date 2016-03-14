using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Utils;
using System.Web.Mvc;
using System.Linq.Expressions;

namespace Quantumart.QP8.WebMvc.ViewModels
{
	public class UserViewModel : UserViewModelBase
	{		
		public static UserViewModel Create(User user, string tabId, int parentId, IUserService service)
		{
			UserViewModel model = EntityViewModel.Create<UserViewModel>(user, tabId, parentId);
			model.service = service;
			model.Init();
			return model;
		}

		public override void Validate(ModelStateDictionary modelState)
		{
			base.Validate(modelState);
			// проверка на то, что удалять builtin пользователей из builtin групп запрещено
			if(!Data.IsNew && Data.BuiltIn)
			{
				User dbUser = service.ReadProperties(Data.Id);
				IEnumerable<int> builtinGroups = dbUser.Groups
					.Where(g => g.BuiltIn && !g.IsReadOnly)
					.Select(g => g.Id)
					.Except(Data.Groups
						.Where(g => g.BuiltIn && !g.IsReadOnly)
						.Select(g => g.Id));

				if (builtinGroups.Any())
				{
					this.IsValid = false;
					string message = String.Format(UserStrings.UnbindBuitInGroup, 
						String.Join(",", 
							service.GetUserGroups(builtinGroups).Select(g => "\"" + g.Name + "\"")
						)
					);
					Expression<Func<object>> f = (() => this.SelectedGroups);
					modelState.AddModelError(f.GetPropertyName(), message);
				}
			}
		}
		
		public override string EntityTypeCode
		{
			get { return C.EntityTypeCode.User; }
		}

		public override string ActionCode
		{
			get { return IsNew ? C.ActionCode.AddNewUser : C.ActionCode.UserProperties; }
		}


		public IEnumerable<ListItem> AvailableLanguages
		{
			get
			{
				return new List<ListItem>() {
                    new ListItem(C.LanguageId.English.ToString(), UserStrings.English),
                    new ListItem(C.LanguageId.Russian.ToString(), UserStrings.Russian),
                 };
			}
		}

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
				return service.GetBindableUserGroups()
					.Select(g => new ListItem
					{
						Value = g.Id.ToString(),
						Text = g.Name
					})
					.ToArray();
			}
		}

		/// <summary>
		/// Инициализирует View Model
		/// </summary>
		private void Init()
		{
			SelectedGroups = Data.Groups
				.Select(g => new QPCheckedItem { Value = g.Id.ToString() })
				.ToList();
		}

		/// <summary>
		/// Устанавливает свойства модели, которые не могут быть установлены автоматически
		/// </summary>
		internal override void DoCustomBinding()
		{
			Data.DoCustomBinding();

			if (SelectedGroups.Any())
				Data.Groups = service.GetUserGroups(Converter.ToInt32Collection(SelectedGroups.Select(g => g.Value).ToArray()));
			else
				Data.Groups = Enumerable.Empty<UserGroup>();
		}
	}
}