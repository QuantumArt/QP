using System.Collections.Generic;
using Quantumart.QP8.BLL.Adapters;
using Quantumart.QP8.BLL.Services.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Adapters;

namespace Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate
{
    public class XmlDbUpdateNonMvcReplayService : XmlDbUpdateReplayService
    {
        private readonly bool _isQpInstalled;

        public XmlDbUpdateNonMvcReplayService(bool disableFieldIdentity,
            bool disableContentIdentity,
            int userId,
            IXmlDbUpdateLogService dbLogService,
            IXmlDbUpdateActionService dbActionService,
            bool isQpInstalled = true)
            : base(disableFieldIdentity, disableContentIdentity, userId, dbLogService, dbActionService)
        {
            _isQpInstalled = isQpInstalled;
        }

        public XmlDbUpdateNonMvcReplayService(string connectionString,
            int userId,
            IXmlDbUpdateLogService dbLogService,
            IXmlDbUpdateActionService dbActionService,
            bool isQpInstalled = true)
            : base(connectionString, userId, dbLogService, dbActionService)
        {
            _isQpInstalled = isQpInstalled;
        }

        public XmlDbUpdateNonMvcReplayService(
            string connectionString,
            HashSet<string> identityInsertOptions,
            int userId,
            IXmlDbUpdateLogService dbLogService,
            IXmlDbUpdateActionService dbActionService,
            bool isQpInstalled = true)
            : base(connectionString, identityInsertOptions, userId, dbLogService, dbActionService)
        {
            _isQpInstalled = isQpInstalled;
        }

        public override void Process(string xmlString, IList<string> filePathes = null)
        {
            if (_isQpInstalled)
            {
                QpInstalledProcess(xmlString, filePathes);
            }
            else
            {
                QpNotInstalledProcess(xmlString, filePathes);
            }
        }

        private void QpInstalledProcess(string xmlString, IList<string> filePathes)
        {
            using (new FakeMvcApplicationContext())
            {
                base.Process(xmlString, filePathes);
            }
        }

        private void QpNotInstalledProcess(string xmlString, IList<string> filePathes)
        {
            using (new NonQpEnvironmentContext())
            using (new FakeMvcApplicationContext())
            {
                base.Process(xmlString, filePathes);
            }
        }
    }
}
