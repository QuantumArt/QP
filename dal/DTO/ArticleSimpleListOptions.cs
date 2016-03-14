using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.DAL.DTO
{
	public class ArticleSimpleListOptions
	{

		public int UserId { get; set; }

		public int ContentId { get; set; }

		public int? ArticleId { get; set; }

		public int? FieldId { get; set; }

		public bool ReturnOnlySelected { get; set; }

		public int[] Ids { get; set; }

		public int PermissionLevel { get; set; }

		public string Filter { get; set; }

	}
}
