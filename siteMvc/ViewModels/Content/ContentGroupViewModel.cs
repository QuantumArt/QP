using Quantumart.QP8.WebMvc.ViewModels.Abstract;
using B = Quantumart.QP8.BLL;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels
{
	public class ContentGroupViewModel : EntityViewModel
	{
		public new B.ContentGroup Data
		{
			get => (B.ContentGroup)EntityData;
		    set => EntityData = value;
		}

		#region creation

		public static ContentGroupViewModel Create(B.ContentGroup group, string tabId, int parentId) => Create<ContentGroupViewModel>(group, tabId, parentId);

	    #endregion

		#region read-only members

		public override string EntityTypeCode => C.EntityTypeCode.ContentGroup;

	    public override string ActionCode
		{
			get
			{
			    if (IsNew)
				{
					return C.ActionCode.AddNewContentGroup;
				}

			    return C.ActionCode.ContentGroupProperties;
			}
		}

		#endregion
	}
}
