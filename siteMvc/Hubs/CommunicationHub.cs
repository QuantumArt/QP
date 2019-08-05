using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Quantumart.QP8.BLL.Services.DbServices;

namespace Quantumart.QP8.WebMvc.Hubs
{
    public class CommunicationHub : Hub, ICommunicationService
    {
        public async Task Send(string key, object value)
        {
            string dbHash = DbService.GetDbHash();

            await Clients.Group(dbHash).SendAsync("Message", key, value);
        }

        public async Task AddHash(string dbHash)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, dbHash);
        }
    }
}
