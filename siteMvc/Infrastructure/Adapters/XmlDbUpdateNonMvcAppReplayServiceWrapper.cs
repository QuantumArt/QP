using System.Collections.Generic;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;

namespace Quantumart.QP8.WebMvc.Infrastructure.Adapters
{
    public class XmlDbUpdateNonMvcAppReplayServiceWrapper : IXmlDbUpdateReplayService
    {
        private readonly IXmlDbUpdateReplayService _xmlDbUpdateReplayService;

        public XmlDbUpdateNonMvcAppReplayServiceWrapper(IXmlDbUpdateReplayService xmlDbUpdateReplayService)
        {
            _xmlDbUpdateReplayService = xmlDbUpdateReplayService;
        }

        public void Process(string xmlString, IList<string> filePathes = null)
        {
            using (new FakeMvcApplication())
            {
                _xmlDbUpdateReplayService.Process(xmlString, filePathes);
            }
        }
    }
}
