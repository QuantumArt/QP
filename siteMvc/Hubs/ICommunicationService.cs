namespace Quantumart.QP8.WebMvc.Hubs
{
    public interface ICommunicationService
    {
        void Send(string key, object value);
    }
}
