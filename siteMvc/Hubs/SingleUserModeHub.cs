using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DbServices;

namespace Quantumart.QP8.WebMvc.Hubs
{
    public class SingleUserModeHub : Hub
    {
        private readonly IUserService _userService;

        public SingleUserModeHub(IUserService userService)
        {
            _userService = userService;
        }

        public override async Task OnConnectedAsync()
        {
            var settings = DbService.ReadSettings();

            object message = null;

            if (settings.SingleUserId.HasValue)
            {
                string userName = null;

                if (settings.SingleUserId.Value != QPContext.CurrentUserId)
                {
                    var user = _userService.ReadProfile(settings.SingleUserId.Value);
                    userName = user.Name;
                }

                message = new
                {
                    userId = settings.SingleUserId.Value,
                    userName
                };
            }

            await Clients.Caller.SendAsync("Message", message);

            await base.OnConnectedAsync();
        }
    }
}
