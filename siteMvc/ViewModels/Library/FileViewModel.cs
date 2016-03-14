using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using C=Quantumart.QP8.Constants;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.ViewModels
{
	public class FileViewModel : EntityViewModel
	{
		#region creation

		public FileViewModel()
		{
			IsSite = true;
		}

		public static FileViewModel Create(FolderFile file, string tabId, int parentId, bool isSite)
		{
			FileViewModel model = new FileViewModel();
			model.File = file;
			model.TabId = tabId;
			model.ParentEntityId = parentId;
			model.IsSite = isSite;
			return model;
		}

		#endregion

		public FolderFile File { get; set; }

		public bool IsSite { get; set; }

		#region overrides

		public override string EntityTypeCode
		{
			get { return (IsSite) ? C.EntityTypeCode.SiteFile : C.EntityTypeCode.ContentFile; }
		}

		public override string ActionCode
		{
			get { return (IsSite) ? C.ActionCode.SiteFileProperties : C.ActionCode.ContentFileProperties; }
		}

		public override void Validate(ModelStateDictionary modelState)
		{
			try
			{
				File.Validate();
			}
			catch (RulesException ex)
			{
				ex.CopyTo(modelState, "Data");
				this.IsValid = false;
			}
		}

		public override string Id
		{
			get
			{
				return File.Name;
			}
		}

		public override string Name
		{
			get
			{
				return File.Name;
			}
		}

		#endregion


    }
}
