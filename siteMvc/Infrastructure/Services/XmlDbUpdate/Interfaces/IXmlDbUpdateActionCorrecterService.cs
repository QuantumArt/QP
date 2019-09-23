using Microsoft.AspNetCore.Http;
using Quantumart.QP8.WebMvc.Infrastructure.Models;

namespace Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate.Interfaces
{
    public interface IXmlDbUpdateActionCorrecterService
    {
        XmlDbUpdateRecordedAction PostActionCorrections(XmlDbUpdateRecordedAction action, HttpContext httpContext);

        XmlDbUpdateRecordedAction PreActionCorrections(XmlDbUpdateRecordedAction action, bool useGuidSubstitution);
    }
}
