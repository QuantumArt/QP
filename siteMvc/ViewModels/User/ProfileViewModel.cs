using System.Collections.Generic;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;

namespace Quantumart.QP8.WebMvc.ViewModels.User
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
                ex.Extend(modelState, "Data");
                IsValid = false;
            }
        }

        public static ProfileViewModel Create(BLL.User user, string tabId, int parentId, IUserService service)
        {
            var model = Create<ProfileViewModel>(user, tabId, parentId);
            model.Service = service;
            return model;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.User;

        public override string ActionCode => Constants.ActionCode.EditProfile;

        public IEnumerable<ListItem> AvailableLanguages => new List<ListItem>
        {
            new ListItem(LanguageId.English.ToString(), UserStrings.English),
            new ListItem(LanguageId.Russian.ToString(), UserStrings.Russian)
        };
    }
}
