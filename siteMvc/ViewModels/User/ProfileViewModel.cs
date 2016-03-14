using System.Collections.Generic;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;
using C = Quantumart.QP8.Constants;
using System.Web.Mvc;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using System.Linq;
using Quantumart.QP8.BLL.Services;


namespace Quantumart.QP8.WebMvc.ViewModels
{
	public class ProfileViewModel : UserViewModelBase
	{		
		public override void Validate(ModelStateDictionary modelState)
		{
			try
			{
				Data.ProfileValidate();
			}
			catch (RulesException ex)
			{
				ex.CopyTo(modelState, "Data");
				this.IsValid = false;
			}
		}

		#region creation

		public static ProfileViewModel Create(User user, string tabId, int parentId, IUserService service)
		{
			ProfileViewModel model = EntityViewModel.Create<ProfileViewModel>(user, tabId, parentId);
			model.service = service;
			return model;
		}

		#endregion

		#region read-only members

		public override string EntityTypeCode
		{
			get
			{
				return C.EntityTypeCode.User;
			}
		}

		public override string ActionCode
		{
			get
			{
				return C.ActionCode.EditProfile;
			}
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
		#endregion

	}
}