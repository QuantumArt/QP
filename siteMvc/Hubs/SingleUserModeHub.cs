using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;

namespace Quantumart.QP8.WebMvc.Hubs
{
    [HubName("singleUserMode")]
    public class SingleUserModeHub : Hub
    {
        private readonly IUserService _userService;

        public SingleUserModeHub(IUserService userService)
        {
            _userService = userService;
        }

        public override Task OnConnected()
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

            Clients.Caller.send(message);
            return base.OnConnected();
        }
    }
}
