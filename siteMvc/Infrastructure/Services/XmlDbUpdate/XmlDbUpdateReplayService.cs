using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Xml.Linq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Models.XmlDbUpdate;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Infrastructure.Adapters;
using Quantumart.QP8.WebMvc.Infrastructure.Constants.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Exceptions;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Models;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate.Interfaces;

namespace Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate
{
    public class XmlDbUpdateReplayService : IXmlDbUpdateReplayService
    {
        protected readonly string ConnectionString;

        private readonly int _userId;

        private readonly bool _useGuidSubstitution;

        private readonly HashSet<string> _identityInsertOptions;

        private readonly IXmlDbUpdateLogService _dbLogService;

        private readonly IApplicationInfoRepository _appInfoRepository;

        private readonly IXmlDbUpdateActionCorrecterService _actionsCorrecterService;

        private readonly IXmlDbUpdateHttpContextProcessor _httpContextProcessor;

        public XmlDbUpdateReplayService(string connectionString, int userId, bool useGuidSubstitution, IXmlDbUpdateLogService dbLogService, IApplicationInfoRepository appInfoRepository, IXmlDbUpdateActionCorrecterService actionsCorrecterService, IXmlDbUpdateHttpContextProcessor httpContextProcessor)
            : this(connectionString, null, userId, useGuidSubstitution, dbLogService, appInfoRepository, actionsCorrecterService, httpContextProcessor) { }

        public XmlDbUpdateReplayService(string connectionString, HashSet<string> identityInsertOptions, int userId, bool useGuidSubstitution, IXmlDbUpdateLogService dbLogService, IApplicationInfoRepository appInfoRepository, IXmlDbUpdateActionCorrecterService actionsCorrecterService, IXmlDbUpdateHttpContextProcessor httpContextProcessor)
        {
            Ensure.NotNullOrWhiteSpace(connectionString, "Connection string should be initialized");

            _userId = userId;
            _useGuidSubstitution = useGuidSubstitution;
            _identityInsertOptions = identityInsertOptions ?? new HashSet<string>();

            ConnectionString = connectionString;
            QPContext.CurrentDbConnectionString = connectionString;

            _dbLogService = dbLogService;
            _appInfoRepository = appInfoRepository;
            _actionsCorrecterService = actionsCorrecterService;
            _httpContextProcessor = httpContextProcessor;
        }

        public virtual void Process(string xmlString, IList<string> filePathes = null)
        {
            Ensure.Argument.NotNullOrWhiteSpace(xmlString, nameof(xmlString));

            var filteredXmlDocument = FilterFromSubRootNodeDuplicates(xmlString);
            var currentDbVersion = _appInfoRepository.GetCurrentDbVersion();
            ValidateReplayInput(filteredXmlDocument, currentDbVersion);

            var filteredXmlString = filteredXmlDocument.ToStringWithDeclaration(SaveOptions.DisableFormatting);
            var dbLogEntry = new XmlDbUpdateLogModel
            {
                UserId = _userId,
                Body = filteredXmlString,
                FileName = filePathes == null ? null : string.Join(",", filePathes),
                Applied = DateTime.Now,
                Hash = HashHelpers.CalculateMd5Hash(filteredXmlString)
            };

            using (new ThreadStorageScopeContext())
            using (var ts = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            using (new QPConnectionScope(ConnectionString, _identityInsertOptions))
            {
                if (_dbLogService.IsFileAlreadyReplayed(dbLogEntry.Hash))
                {
                    var throwEx = new XmlDbUpdateLoggingException("XmlDbUpdate conflict: current xml document(s) already applied and exist at database.");
                    throwEx.Data.Add("LogEntry", dbLogEntry.ToJsonLog());
                    throw throwEx;
                }

                var updateId = _dbLogService.InsertFileLogEntry(dbLogEntry);
                ReplayActionsFromXml(filteredXmlDocument.Root.Elements(), currentDbVersion, filteredXmlDocument.Root.Attribute(XmlDbUpdateXDocumentConstants.RootBackendUrlAttribute).Value, updateId);
                ts.Complete();
            }
        }

        internal static XDocument FilterFromSubRootNodeDuplicates(string xmlString)
        {
            var document = XDocument.Parse(xmlString);
            var comparer = new XNodeEqualityComparer();
            var distinctElements = document.Root.Elements().Distinct(comparer);
            foreach (var element in distinctElements)
            {
                document.Root.Elements().Where(n => comparer.Equals(n, element)).Skip(1).Remove();
            }

            return document;
        }

        private void ReplayActionsFromXml(IEnumerable<XElement> actionElements, string currentDbVersion, string backendUrl, int updateId)
        {
            foreach (var xmlAction in actionElements)
            {
                XmlDbUpdateRecordedAction action;
                var xmlActionString = xmlAction.ToString(SaveOptions.DisableFormatting);

                try
                {
                    action = XmlDbUpdateSerializerHelpers.DeserializeAction(xmlAction);
                }
                catch (Exception ex)
                {
                    var throwEx = new XmlDbUpdateReplayActionException("Error while deserializing xml action string.", ex);
                    throwEx.Data.Add("XmlActionString", xmlActionString.ToJsonLog());
                    throw throwEx;
                }

                var logEntry = new XmlDbUpdateActionsLogModel
                {
                    UserId = _userId,
                    Applied = DateTime.Now,
                    ParentId = action.ParentId,
                    UpdateId = updateId,
                    SourceXml = xmlActionString,
                    Hash = HashHelpers.CalculateMd5Hash(xmlActionString)
                };

                if (_dbLogService.IsActionAlreadyReplayed(logEntry.Hash))
                {
                    Logger.Log.Warn($"XmlDbUpdateAction conflict: Current action already applied and exist at database. Entry: {logEntry.ToJsonLog()}");
                    continue;
                }

                var xmlActionStringLog = xmlAction.RemoveDescendants().ToString(SaveOptions.DisableFormatting);
                Logger.Log.Debug($"-> Begin replaying action: -> {xmlActionStringLog}");
                var replayedAction = ReplayAction(action, backendUrl);
                Logger.Log.Debug($"End replaying action: {xmlActionStringLog}");

                logEntry.Ids = string.Join(",", replayedAction.Ids);
                logEntry.ResultXml = XmlDbUpdateSerializerHelpers.SerializeAction(replayedAction, currentDbVersion, backendUrl).ToStringWithDeclaration(SaveOptions.DisableFormatting);
                _dbLogService.InsertActionLogEntry(logEntry);
            }
        }

        private XmlDbUpdateRecordedAction ReplayAction(XmlDbUpdateRecordedAction xmlAction, string backendUrl)
        {
            try
            {
                var correctedAction = _actionsCorrecterService.PreActionCorrections(xmlAction, _useGuidSubstitution);
                var httpContext = _httpContextProcessor.PostAction(correctedAction, backendUrl, _userId, _useGuidSubstitution);
                return _actionsCorrecterService.PostActionCorrections(correctedAction, httpContext);
            }
            catch (Exception ex)
            {
                var throwEx = new XmlDbUpdateReplayActionException("Error while replaying xml action.", ex);
                throwEx.Data.Add("ActionToReplay", xmlAction.ToJsonLog());
                throw throwEx;
            }
        }

        private void ValidateReplayInput(XContainer xmlDocument, string currentDbVersion)
        {
            using (new QPConnectionScope(ConnectionString, _identityInsertOptions))
            {
                if (!ValidateDbVersion(xmlDocument, currentDbVersion))
                {
                    throw new InvalidOperationException("DB versions doesn't match");
                }

                if (_appInfoRepository.RecordActions())
                {
                    throw new InvalidOperationException("Replaying actions cannot be proceeded on the database which has recording option on");
                }
            }
        }

        private static bool ValidateDbVersion(XContainer doc, string currentDbVersion)
        {
            var root = doc.Elements(XmlDbUpdateXDocumentConstants.RootElement).Single();
            return root.Attribute(XmlDbUpdateXDocumentConstants.RootDbVersionAttribute) == null || root.Attribute(XmlDbUpdateXDocumentConstants.RootDbVersionAttribute).Value == currentDbVersion;
        }
    }
}
