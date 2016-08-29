using Microsoft.AspNet.SignalR;
using Quantumart.QP8.BLL.Services;

namespace Quantumart.QP8.WebMvc.Hubs
{
    public class CommunicationService : ICommunicationService
    {
        private readonly IHubContext<ICommunicationService> _context;

        public CommunicationService()
        {
            _context = GlobalHost.ConnectionManager.GetHubContext<ICommunicationService>("communication");
        }

        public void Send(string key, object value)
        {
            var dbHash = DbService.GetDbHash();
            _context.Clients.Group(dbHash).Send(key, value);
        }
    }
}
