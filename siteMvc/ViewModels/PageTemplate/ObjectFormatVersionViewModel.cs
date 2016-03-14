using Quantumart.QP8.BLL;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
	public class ObjectFormatVersionViewModel : EntityViewModel
	{
		protected bool _pageOrTemplate;
		protected int _parentId;		

		public override string EntityTypeCode
		{
			get { return _pageOrTemplate ? Constants.EntityTypeCode.PageObjectFormatVersion : Constants.EntityTypeCode.TemplateObjectFormatVersion; }
		}

		public override string ActionCode
		{
			get
			{
				return _pageOrTemplate ? Constants.ActionCode.PageObjectFormatVersionProperties : Constants.ActionCode.TemplateObjectFormatVersionProperties;
			}
		}

		internal static ObjectFormatVersionViewModel Create(BLL.ObjectFormatVersion version, string tabId, int parentId, bool pageOrTemplate)
		{
			var model = EntityViewModel.Create<ObjectFormatVersionViewModel>(version, tabId, parentId);
			model._parentId = parentId;
			model._pageOrTemplate = pageOrTemplate;
			return model;
		}

		public new ObjectFormatVersion Data
		{
			get
			{
				return (ObjectFormatVersion)EntityData;
			}
			set
			{
				EntityData = value;
			}
		}		
	}

	public class ObjectFormatVersionCompareViewModel : EntityViewModel
	{
		bool _pageOrTemplate;
		public bool IsComparison
		{
			get
			{
				return Data.VersionToMerge != null;
			}
		}

		public override string EntityTypeCode
		{
			get { return _pageOrTemplate? C.EntityTypeCode.PageObjectFormatVersion: C.EntityTypeCode.TemplateObjectFormatVersion; }
		}

		public override string ActionCode
		{
			get { return _pageOrTemplate? C.ActionCode.ComparePageObjectFormatVersions : C.ActionCode.CompareTemplateObjectFormatVersions; }
		}

		public new ObjectFormatVersion Data
		{
			get
			{
				return (ObjectFormatVersion)EntityData;
			}

			set
			{
				EntityData = value;
			}
		}

		public override string Id
		{
			get
			{
				return (IsComparison) ? "0" : base.Id;
			}
		}

		public override ExpandoObject MainComponentParameters
		{
			get
			{
				dynamic result = base.MainComponentParameters;
				if (IsComparison)
				{
					string firstId = Id.ToString();
					string secondId = Data.VersionToMerge.Id.ToString();
					result.entities = new ClientEntity[] { new ClientEntity { Id = firstId, Name = firstId }, new ClientEntity { Id = secondId, Name = secondId } };
				}
				return result;
			}
		}

		public static ObjectFormatVersionCompareViewModel Create(ObjectFormatVersion version, string tabId, int parentId, bool pageOrTemplate)
		{
			ObjectFormatVersionCompareViewModel model = EntityViewModel.Create<ObjectFormatVersionCompareViewModel>(version, tabId, parentId);
			model.SuccesfulActionCode = string.Empty;
			model._pageOrTemplate = pageOrTemplate;
			return model;
		}
	}
}