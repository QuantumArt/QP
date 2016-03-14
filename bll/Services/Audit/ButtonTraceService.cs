using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL.Services.Audit
{
	public interface IButtonTraceService
	{
		ListResult<ButtonTrace> GetPage(ListCommand cmd);
	}
	
	public class ButtonTraceService : IButtonTraceService
	{
		private readonly IButtonTracePagesRepository repository;

		public ButtonTraceService(IButtonTracePagesRepository repository)
		{
			this.repository = repository;

		}
		#region IButtonTraceService Members

		public ListResult<ButtonTrace> GetPage(ListCommand cmd)
		{
			int totalRecords;
			List<ButtonTrace> data = repository.GetPage(cmd, out totalRecords).ToList();
			return new ListResult<ButtonTrace>
			{
				Data = data,
				TotalRecords = totalRecords
			};
		}

		#endregion
	}
}
