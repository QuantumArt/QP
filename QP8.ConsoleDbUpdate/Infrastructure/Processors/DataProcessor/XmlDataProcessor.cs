using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Models.XmlDbUpdate;
using Quantumart.QP8.BLL.Repository.XmlDbUpdate;
using Quantumart.QP8.BLL.Services.XmlDbUpdate;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.FileSystemReaders;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Exceptions;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.DataProcessor
{
    internal class XmlDataProcessor : IDataProcessor
    {
        private readonly BaseSettingsModel _settings;
        private readonly IXmlDbUpdateReplayService _xmlDbUpdateReplayService;

        public XmlDataProcessor(XmlSettingsModel settings)
        {
            QPContext.CurrentCustomerCode = settings.CustomerCode;
            var dbLogService = new XmlDbUpdateLogService(new XmlDbUpdateLogRepository(), new XmlDbUpdateActionsLogRepository());

            _settings = settings;
            _xmlDbUpdateReplayService = new XmlDbUpdateNonMvcReplayService(settings.DisableFieldIdentity, settings.DisableContentIdentity, _settings.UserId, dbLogService);
        }

        public void Process()
        {
            //TODO: DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!! And remove unusing references then.
            #region DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!!
            if (_settings.FilePathes.Count == 1 && _settings.FilePathes[0].EndsWith(".xml"))
            {
                var logService = new XmlDbUpdateLogService(new XmlDbUpdateLogRepository(), new XmlDbUpdateActionsLogRepository());
                var xmlString = XmlReaderProcessor.Process(_settings.FilePathes, _settings.ConfigPath);
                var logEntry = new XmlDbUpdateLogModel
                {
                    Body = xmlString,
                    FileName = _settings.FilePathes[0],
                    Applied = DateTime.Now,
                    UserId = 1
                };

                var md5 = MD5.Create();
                var inputBytes = Encoding.UTF8.GetBytes(xmlString);
                var hash = md5.ComputeHash(inputBytes);
                var sb = new StringBuilder();
                foreach (var t in hash)
                {
                    sb.Append(t.ToString("X2"));
                }

                logEntry.Hash = sb.ToString();

                var identityTypes = new HashSet<string>();
                if (!((XmlSettingsModel)_settings).DisableFieldIdentity)
                {
                    identityTypes.Add(EntityTypeCode.Field);
                    identityTypes.Add(EntityTypeCode.ContentLink);
                }

                if (!((XmlSettingsModel)_settings).DisableContentIdentity)
                {
                    identityTypes.Add(EntityTypeCode.Content);
                    identityTypes.Add(EntityTypeCode.ContentGroup);
                }

                using (new QPConnectionScope(QPConfiguration.ConfigConnectionString(QPContext.CurrentCustomerCode), identityTypes))
                {
                    if (logService.IsFileAlreadyReplayed(logEntry.Hash))
                    {
                        var throwEx = new XmlDbUpdateLoggingException("Current xml document already applied and exist at XmlDbUpdate database.");
                        throwEx.Data.Add("LogEntry", logEntry.ToJsonLog());
                        throw throwEx;
                    }
                }
            }
            #endregion DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!!

            _xmlDbUpdateReplayService.Process(XmlReaderProcessor.Process(_settings.FilePathes, _settings.ConfigPath), _settings.FilePathes);
        }
    }
}
