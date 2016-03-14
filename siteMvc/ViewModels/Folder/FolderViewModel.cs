using System;
using System.Collections.Generic;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class FolderViewModel : ViewModel
    {

		public List<LibraryItem> Data { get; set; }
	
		public string Name { get; set; }

		public PathInfo PathInfo { get; set; }

		public int TotalRecords { get; set; }

		public bool IsSiteFolder { get; set; }

        #region creation

		public FolderViewModel()
		{
			IsSiteFolder = true;
		}

		public static FolderViewModel Create(FolderResult result, string tabId, int parentId)
        {
			FolderViewModel model = ViewModel.Create<FolderViewModel>(tabId, parentId);
			model.Init(result);
			return model;
        }

		private void Init(FolderResult result)
		{
			Name = result.Name;
            PathInfo = result.PathInfo;
			Data = result.ListResult.Data;
            TotalRecords = result.ListResult.TotalRecords;		
		}
        #endregion

        #region read-only members

        public override string EntityTypeCode
        {
            get
            {
                return (IsSiteFolder) ? C.EntityTypeCode.SiteFolder : C.EntityTypeCode.ContentFolder;
            }
        }

        public override string ActionCode
        {
            get
            {
				return (IsSiteFolder) ? C.ActionCode.SiteFolder : C.EntityTypeCode.ContentFolder;
            }
        }

        public override string MediatorClassName
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

    }
}
