using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.ViewModels.Library
{
	public class ContentFolderViewModel : FolderViewModel
	{
		internal static ContentFolderViewModel Create(ContentFolder folder, string tabId, int parentId)
		{
			return EntityViewModel.Create<ContentFolderViewModel>(folder, tabId, parentId);
		}

		public new ContentFolder Data
		{
			get
			{
				return (ContentFolder)EntityData;
			}
			set
			{
				EntityData = value;
			}
		}

		public override string EntityTypeCode
		{
			get { return C.EntityTypeCode.ContentFolder; }
		}

		public override string ActionCode
		{
			get
			{
				if (this.IsNew)
				{
					return C.ActionCode.AddNewContentFolder;
				}
				else
				{
					return C.ActionCode.ContentFolderProperties;
				}
			}
		}
	}
}