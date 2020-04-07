using System.Threading.Tasks;

namespace Quantumart.QP8.WebMvc.Hubs
{
    public interface ICommunicationService
    {
        Task Send(string key, object value);
    }
}
