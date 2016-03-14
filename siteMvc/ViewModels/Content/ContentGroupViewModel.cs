using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using B = Quantumart.QP8.BLL;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.ViewModels
{
	public class ContentGroupViewModel : EntityViewModel
	{
		public new B.ContentGroup Data
		{
			get
			{
				return (B.ContentGroup)EntityData;
			}
			set
			{
				EntityData = value;
			}
		}

		#region creation

		public static ContentGroupViewModel Create(ContentGroup group, string tabId, int parentId)
		{
			return EntityViewModel.Create<ContentGroupViewModel>(group, tabId, parentId);
		}

		#endregion

		#region read-only members

		public override string EntityTypeCode
		{
			get
			{
				return C.EntityTypeCode.ContentGroup;
			}
		}

		public override string ActionCode
		{
			get
			{
				if (IsNew)
				{
					return C.ActionCode.AddNewContentGroup;
				}
				else
				{
					return C.ActionCode.ContentGroupProperties;
				}
			}
		}

		#endregion
	}
}