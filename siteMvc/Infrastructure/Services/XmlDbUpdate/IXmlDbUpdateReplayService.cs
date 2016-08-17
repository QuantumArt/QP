using System.Collections.Generic;
using System.Xml.Linq;
using Quantumart.QP8.WebMvc.Infrastructure.Models;

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

        /// <summary>
        /// Replay actions and create log entries for each individual action
        /// </summary>
        /// <param name="actionElements">Collection of actions to replay</param>
        /// <param name="backendUrl">Backend url used to replay actions</param>
        /// <param name="updateId"></param>
        void ReplayActionsFromXml(IEnumerable<XElement> actionElements, string backendUrl, int updateId);

        /// <summary>
        /// Correct action and send through HttpContext action method to replay
        /// </summary>
        /// <param name="mvcScope">Mvc scope service to emulate HttpContext from not MVC projects</param>
        /// <param name="action">Action to replay</param>
        /// <param name="backendUrl">Backend url used to replay actions</param>
        /// <returns></returns>
        XmlDbUpdateRecordedAction CorrectActionAndReplay(MvcScope mvcScope, XmlDbUpdateRecordedAction action, string backendUrl);
    }
}
