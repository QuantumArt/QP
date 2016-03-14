using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.Constants
{
    /// <summary>
    /// Типы действий SystemStatusType
    /// </summary>
    public enum SystemStatusType
    {
        PromotingByWorkflow = 9,
        DemotingByWorkflow = 10,
        ForcedDemoting = 11,
        PartialPromoting = 12,
        PartialDemoting = 13,
        ForcedPromoting = 14
    }
}
