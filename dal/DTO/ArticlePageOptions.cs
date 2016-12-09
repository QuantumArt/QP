using System;
using System.Collections.Generic;

namespace Quantumart.QP8.DAL.DTO
{
	public class ArticlePageOptions : PageOptionsBase
	{
		public bool UseSecurity { get; set; }

		public bool UseRelationSecurity { get; set; }

		public int UserId { get; set; }

		public int ContentId { get; set; }

		public int[] ExtensionContentIds { get; set; }

		public ContentReference[] ContentReferences { get; set; }

		public ArticleFullTextSearchParameter FullTextSearch { get; set; }

		public IEnumerable<ArticleLinkSearchParameter> LinkFilters { get; set; }

		public IEnumerable<ArticleRelationSecurityParameter> RelationSecurityFilters { get; set; }

		public string CommonFilter { get; set; }

		public bool IsVirtual { get; set; }

		public string ContextFilter { get; set; }

		public string VariationFieldName { get; set; }

		public bool UseMainTableForVariations { get; set; }

		public bool OnlyIds { get; set; }

		public bool UseSql2012Syntax { get; set; }

        public int[] FilterIds { get; set; }
	}

	public class ContentReference : IEquatable<ContentReference>
	{
		public int ReferenceFieldID { get; set; }

		public int TargetContentId { get; set; }

	    public bool Equals(ContentReference otherRef)
	    {
	        if (ReferenceEquals(otherRef, null))
	        {
	            return false;
	        }

	        if (ReferenceEquals(this, otherRef))
	        {
	            return true;
	        }

	        return (otherRef.TargetContentId == TargetContentId && otherRef.ReferenceFieldID == ReferenceFieldID);
	    }

	    public override int GetHashCode()
	    {
	        return $"{TargetContentId};{ReferenceFieldID}".GetHashCode();
	    }
	}
}
