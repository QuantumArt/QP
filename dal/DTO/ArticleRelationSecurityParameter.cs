using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.DAL.DTO
{
	public class ArticleRelationSecurityParameter
	{
		public bool IsManyToMany { get; set; } // logic

		public bool IsClassifier { get; set; }

		public int[] AllowedContentIds { get; set; }

		public int FieldId { get; set; } // for alias

		public string FieldName { get; set; } // O2M JOIN

		public int? LinkId { get; set; } //M2M JOIN

		public int RelatedContentId { get; set; } //JOIN

	}
}
