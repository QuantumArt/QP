using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Transactions;
using System.Xml.Linq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Models.XmlDbUpdate;
using Quantumart.QP8.BLL.Repository.XmlDbUpdate;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Constants.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Exceptions;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.Models;

namespace Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate
{
    /// <summary>
    /// Service for replaying xml data with recorded actions
    /// <exception cref="XmlDbUpdateLoggingException">Throws when logging is failed</exception>
    /// <exception cref="XmlDbUpdateReplayActionException">Throws when xml replaying was failed</exception>
    /// </summary>
    public class XmlDbUpdateReplayService
    {
        private readonly string _connectionString;

        private readonly HashSet<string> _identityInsertOptions;

        private readonly XmlDbUpdateActionCorrectionService _actionsCorrecter;

        private XmlDbUpdateLogRepository _dbUpdateLogRepository;

        private XmlDbUpdateActionsLogRepository _dbUpdateActionsLogRepository;

        public XmlDbUpdateReplayService(string connectionString)
            : this(connectionString, null)
        {
        }

        public XmlDbUpdateReplayService(bool disableFieldIdentity, bool disableContentIdentity)
            : this(QPConfiguration.ConfigConnectionString(QPContext.CurrentCustomerCode), GetIdentityInsertOptions(disableFieldIdentity, disableContentIdentity))
        {
        }

        public XmlDbUpdateReplayService(string connectionString, HashSet<string> identityInsertOptions)
        {
            _connectionString = connectionString;
            _identityInsertOptions = identityInsertOptions;
            _actionsCorrecter = new XmlDbUpdateActionCorrectionService();
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void Process(string xmlString, IList<string> filePathes = null)
        {
            Ensure.Argument.NotNullOrEmpty(xmlString, nameof(xmlString));
            Ensure.NotNullOrEmpty(_connectionString, "Connection string should be initialized");

            var filteredXmlDocument = FilterFromSubRootNodeDuplicates(xmlString);
            ValidateReplayInput(filteredXmlDocument);
            var filteredXmlString = filteredXmlDocument.ToStringWithDeclaration();

            var dbLogEntry = new XmlDbUpdateLogModel
            {
                Body = filteredXmlString,
                FileName = filePathes == null ? null : string.Join(",", filePathes),
                Applied = DateTime.Now,
                Hash = HashHelpers.CalculateMd5Hash(filteredXmlString)
            };

            _dbUpdateLogRepository = new XmlDbUpdateLogRepository();
            _dbUpdateActionsLogRepository = new XmlDbUpdateActionsLogRepository();
            using (var ts = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            using (new QPConnectionScope(_connectionString, _identityInsertOptions))
            {
                if (_dbUpdateLogRepository.IsExist(dbLogEntry.Hash))
                {
                    var throwEx = new XmlDbUpdateLoggingException("Current xml document already applied and exist at XmlDbUpdate database.");
                    throwEx.Data.Add("LogEntry", dbLogEntry.ToJsonLog());
                    throw throwEx;
                }

                var updateId = _dbUpdateLogRepository.Insert(dbLogEntry);
                ReplayActionsFromXml(filteredXmlDocument.Root.Elements(), filteredXmlDocument.Root.Attribute(XmlDbUpdateXDocumentConstants.RootBackendUrlAttribute).Value, updateId);
                ts.Complete();
            }
        }

        public void ReplayActionsFromXml(IEnumerable<XElement> actionElements, string backendUrl, int updateId)
        {
            using (var mvcScope = new MvcScope())
            {
                foreach (var xmlAction in actionElements)
                {
                    XmlDbUpdateRecordedAction action;
                    var xmlActionString = xmlAction.ToString();

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
                        Applied = DateTime.Now,
                        Ids = string.Join(",", action.Ids),
                        ParentId = action.ParentId,
                        UpdateId = updateId,
                        SourceXml = xmlActionString,
                        Hash = HashHelpers.CalculateMd5Hash(xmlActionString)
                    };

                    if (_dbUpdateActionsLogRepository.IsExist(logEntry.Hash))
                    {
                        var throwEx = new XmlDbUpdateLoggingException("Current action already applied and exist at XmlDbUpdateAction database.");
                        throwEx.Data.Add("LogEntry", logEntry.ToJsonLog());
                        throw throwEx;
                    }

                    var correctedAction = CorrectActions(mvcScope, action, backendUrl);
                    logEntry.ResultXml = XmlDbUpdateSerializerHelpers.SerializeAction(correctedAction, backendUrl).ToString();
                    _dbUpdateActionsLogRepository.Insert(logEntry);
                }
            }
        }

        private XmlDbUpdateRecordedAction CorrectActions(MvcScope mvcScope, XmlDbUpdateRecordedAction action, string backendUrl)
        {
            try
            {
                var recordedAction = _actionsCorrecter.CorrectAction(action);
                var httpContext = mvcScope.InitializeContext(recordedAction, backendUrl);
                return _actionsCorrecter.CorrectReplaces(recordedAction, httpContext);
            }
            catch (Exception ex)
            {
                var throwEx = new XmlDbUpdateReplayActionException("Error while replaying xml action.", ex);
                throwEx.Data.Add("ActionToReplay", action.ToJsonLog());
                throw throwEx;
            }
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static XDocument FilterFromSubRootNodeDuplicates(string xmlString)
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

        private static void ValidateReplayInput(XContainer xmlDocument)
        {
            if (!ValidateDbVersion(xmlDocument))
            {
                throw new InvalidOperationException("DB versions doesn't match");
            }

            if (ApplicationInfoHelper.RecordActions())
            {
                throw new InvalidOperationException("Replaying actions cannot be proceeded on the database which has recording option on");
            }
        }

        private static bool ValidateDbVersion(XContainer doc)
        {
            var root = doc.Elements(XmlDbUpdateXDocumentConstants.RootElement).Single();
            return root.Attribute(XmlDbUpdateXDocumentConstants.RootDbVersionAttribute) == null || root.Attribute(XmlDbUpdateXDocumentConstants.RootDbVersionAttribute).Value == new ApplicationInfoHelper().GetCurrentDbVersion();
        }

        private static HashSet<string> GetIdentityInsertOptions(bool disableFieldIdentity, bool disableContentIdentity)
        {
            var identityTypes = new HashSet<string>();
            if (!disableFieldIdentity)
            {
                identityTypes.Add(EntityTypeCode.Field);
                identityTypes.Add(EntityTypeCode.ContentLink);
            }

            if (!disableContentIdentity)
            {
                identityTypes.Add(EntityTypeCode.Content);
                identityTypes.Add(EntityTypeCode.ContentGroup);
            }

            return identityTypes;
        }
    }
}
