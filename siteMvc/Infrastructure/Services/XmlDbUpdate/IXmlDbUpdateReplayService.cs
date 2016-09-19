using System.Collections.Generic;

namespace Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate
{
    /// <summary>
    /// Service for replaying xml data with recorded actions
    /// <exception cref="Quantumart.QP8.WebMvc.Infrastructure.Exceptions.XmlDbUpdateLoggingException">Throws when logging is failed</exception>
    /// <exception cref="Quantumart.QP8.WebMvc.Infrastructure.Exceptions.XmlDbUpdateReplayActionException">Throws when xml replaying was failed</exception>
    /// </summary>
    public interface IXmlDbUpdateReplayService
    {
        /// <summary>
        /// Parse and replay actions from xml file, and caching xml data at database
        /// </summary>
        /// <param name="xmlString">Xml string with actions to replay</param>
        /// <param name="filePathes">File pathes used for log entry, from which xml string was parsed</param>
        void Process(string xmlString, IList<string> filePathes = null);
    }
}
