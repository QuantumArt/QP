using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Services.DTO
{
	public class ContentInitListResult : InitListResultBase
    {
		
		public ContentInitListResult()
		{
			IsVirtual = false;
		}

		public List<Content> Data { get; set; }

		public string ParentName { get; set; }
        
		public bool IsVirtual { get; set; }		

    }
}
