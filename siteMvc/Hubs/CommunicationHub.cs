using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Quantumart.QP8.BLL.Services.DbServices;

namespace Quantumart.QP8.WebMvc.Hubs
{
    public class CommunicationHub : Hub
    {
        public async Task AddHash(string dbHash)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, dbHash);
        }
    }
}
