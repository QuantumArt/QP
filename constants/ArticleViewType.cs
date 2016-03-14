using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.Constants
{
    public enum ArticleViewType
    {
        Normal,
        LockedByOtherUser,
		ReadOnlyBecauseOfWorkflow,
		ReadOnlyBecauseOfSecurity,
		ReadOnlyBecauseOfRelationSecurity,
        Virtual,
        Archived,
        PreviewVersion,
        CompareVersions
    }
}
