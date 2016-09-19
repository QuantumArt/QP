using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class UserSearchBlockViewModel
    {
        private readonly string _hostId;

        public UserSearchBlockViewModel(string hostId)
        {
            _hostId = hostId;
        }

        public string LoginElementId => HtmlHelperFieldExtensions.UniqueId("txtLogin", _hostId);

        public string EmailElementId => HtmlHelperFieldExtensions.UniqueId("txtEmail", _hostId);

        public string FirstNameElementId => HtmlHelperFieldExtensions.UniqueId("txtFirstName", _hostId);

        public string LastNameElementId => HtmlHelperFieldExtensions.UniqueId("txtLastName", _hostId);
    }
}
