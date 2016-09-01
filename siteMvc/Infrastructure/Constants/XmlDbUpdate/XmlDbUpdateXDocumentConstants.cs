﻿using Quantumart.QP8.BLL;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.WebMvc.Infrastructure.Constants.XmlDbUpdate
{
    internal class XmlDbUpdateXDocumentConstants
    {
        internal static readonly string XmlFilePath = $"{QPConfiguration.TempDirectory}{QPContext.CurrentCustomerCode}.xml";

        internal const string RootElement = "actions";

        internal const string RootBackendUrlAttribute = "backendUrl";

        internal const string RootDbVersionAttribute = "dbVersion";

        internal const string ActionElement = "action";

        internal const string ActionCodeAttribute = "code";

        internal const string ActionIdsAttribute = "ids";

        internal const string ActionParentIdAttribute = "parentId";

        internal const string ActionLcidAttribute = "lcid";

        internal const string ActionExecutedAttribute = "executed";

        internal const string ActionExecutedByAttribute = "executedBy";

        internal const string ActionNewLinkIdAttribute = "newLinkId";

        internal const string ActionFormatIdAttribute = "formatId";

        internal const string ActionActionId = "actionId";

        internal const string ActionActionCode = "actionCode";

        internal const string ActionFieldIdsAttribute = "fieldIds";

        internal const string ActionNewChildFieldIdsAttribute = "newChildFieldIds";

        internal const string ActionCommandIdsAttribute = "commandIds";

        internal const string ActionRulesIdsAttribute = "rulesIds";

        internal const string ActionLinkIdsAttribute = "linkIds";

        internal const string ActionNewChildLinkIdsAttribute = "newChildLinkIds";

        internal const string ActionNewBackwardIdAttribute = "newBackwardId";

        internal const string ActionResultIdAttribute = "resultId";

        internal const string ActionNewVirtualFieldIdsAttribute = "newVirtualFieldIds";

        internal const string FieldElement = "field";

        internal const string FieldNameAttribute = "name";
    }
}