using System.Collections.Generic;
using Quantumart.QP8.BLL.Adapters;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Adapters;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate.Interfaces;

namespace Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate
{
    public class XmlDbUpdateNonMvcReplayService : XmlDbUpdateReplayService
    {
        private readonly bool _isQpInstalled;

        public XmlDbUpdateNonMvcReplayService(
            string connectionString,
            int userId,
            bool useGuidSubstitution,
            IXmlDbUpdateLogService dbLogService,
            IApplicationInfoRepository appInfoRepository,
            IXmlDbUpdateActionCorrecterService actionsCorrecterService,
            IXmlDbUpdateHttpContextProcessor httpContextProcessor,
            bool isQpInstalled = true)
            : base(connectionString, userId, useGuidSubstitution, dbLogService, appInfoRepository, actionsCorrecterService, httpContextProcessor)
        {
            _isQpInstalled = isQpInstalled;
        }

        public XmlDbUpdateNonMvcReplayService(
            string connectionString,
            DatabaseType dbType,
            HashSet<string> identityInsertOptions,
            int userId,
            bool useGuidSubstitution,
            IXmlDbUpdateLogService dbLogService,
            IApplicationInfoRepository appInfoRepository,
            IXmlDbUpdateActionCorrecterService actionsCorrecterService,
            IXmlDbUpdateHttpContextProcessor httpContextProcessor,
            bool isQpInstalled = true)
            : base(connectionString, dbType, identityInsertOptions, userId, useGuidSubstitution, dbLogService, appInfoRepository, actionsCorrecterService, httpContextProcessor)
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
            using (new NonQpEnvironmentContext(ConnectionInfo))
            using (new FakeMvcApplicationContext())
            {
                base.Process(xmlString, filePathes);
            }
        }
    }
}
