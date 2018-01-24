using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Quantumart.QP8.BLL.Services.DbServices;

namespace Quantumart.QP8.WebMvc.Hubs
{
    [HubName("communication")]
    public class CommunicationHub : Hub, ICommunicationService
    {
        public void Send(string key, object value)
        {
            var dbHash = DbService.GetDbHash();
            Clients.Group(dbHash).send(key, value);
        }

        public void AddHash(string dbHash)
        {
            Groups.Add(Context.ConnectionId, dbHash);
        }
    }
}
