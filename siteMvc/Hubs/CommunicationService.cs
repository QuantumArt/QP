using Microsoft.AspNetCore.SignalR;
using Quantumart.QP8.BLL.Services.DbServices;
using System.Threading.Tasks;

namespace Quantumart.QP8.WebMvc.Hubs
{
    public class CommunicationService : ICommunicationService
    {
        private readonly IHubContext<CommunicationHub> _context;

        public CommunicationService(IHubContext<CommunicationHub> context)
        {
            _context = context;
        }

        public async Task Send(string key, object value)
        {
            string dbHash = DbService.GetDbHash();

            await _context.Clients.Group(dbHash).SendAsync("Message", key, value);
        }
    }
}
