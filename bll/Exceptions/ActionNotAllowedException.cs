using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Exceptions
{
	public class ActionNotAllowedException : ApplicationException
	{
		public ActionNotAllowedException() : base()
		{

		}

		public ActionNotAllowedException(string message) : base(message)
		{

		}

		public static ActionNotAllowedException CreateNotAllowedForAggregatedArticleException() { return new ActionNotAllowedException(ArticleStrings.OperationIsNotAllowedForAggregated); }
		public static ActionNotAllowedException CreateNotAllowedForAggregatedContentException() { return new ActionNotAllowedException(ContentStrings.OperationIsNotAllowedForAggregated); }
		public static ActionNotAllowedException CreateNotAllowedForRootContentException() { return new ActionNotAllowedException(ContentStrings.OperationIsNotAllowedForRoot); }
		public static ActionNotAllowedException CreateNotAllowedForArticleChangingActionException() { return new ActionNotAllowedException(ContentStrings.ArticleChangingIsProhibited); }
		public static ActionNotAllowedException CreateNotAllowedForContentChangingActionException() { return new ActionNotAllowedException(ContentStrings.ContentChangingIsProhibited); }

		internal static Exception UpdateNotAllowedBecauseOfRelationSecurityException()
		{
			return new ActionNotAllowedException(ArticleStrings.CannotUpdateBecauseOfRelationSecurity);
		}

		internal static Exception CreateNotAllowedBecauseOfRelationSecurityException()
		{
			return new ActionNotAllowedException(ArticleStrings.CannotAddBecauseOfRelationSecurity);
		}
	}
}
