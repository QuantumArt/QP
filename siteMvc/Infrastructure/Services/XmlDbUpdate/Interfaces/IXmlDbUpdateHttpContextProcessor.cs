using System.Web;
using Quantumart.QP8.WebMvc.Infrastructure.Models;

namespace Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate.Interfaces
{
    public interface IXmlDbUpdateHttpContextProcessor
    {
        HttpContextBase PostAction(XmlDbUpdateRecordedAction recordedAction, string backendUrl, int userId, bool useGuidSubstitution);
    }
}
