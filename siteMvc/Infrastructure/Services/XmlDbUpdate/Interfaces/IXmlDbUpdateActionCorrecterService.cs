using System.Web;
using Quantumart.QP8.WebMvc.Infrastructure.Models;

namespace Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate.Interfaces
{
    public interface IXmlDbUpdateActionCorrecterService
    {
        XmlDbUpdateRecordedAction PostActionCorrections(XmlDbUpdateRecordedAction action, HttpContextBase httpContext);

        XmlDbUpdateRecordedAction PreActionCorrections(XmlDbUpdateRecordedAction action, bool useGuidSubstitution);
    }
}
