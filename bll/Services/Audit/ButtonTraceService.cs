using System.Linq;
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
            var data = repository.GetPage(cmd, out var totalRecords).ToList();
            return new ListResult<ButtonTrace>
			{
				Data = data,
				TotalRecords = totalRecords
			};
		}

		#endregion
	}
}
