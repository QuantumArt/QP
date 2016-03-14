using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL.Services.Audit
{
	public interface IRemovedEntitiesService
	{
		ListResult<RemovedEntity> GetPage(ListCommand cmd);
	}
	public class RemovedEntitiesService : IRemovedEntitiesService
	{
		private readonly IRemovedEntitiesPagesRepository repository;

		public RemovedEntitiesService(IRemovedEntitiesPagesRepository repository)
		{
			this.repository = repository;
		}

		#region IRemovedEntitiesService Members

		public ListResult<RemovedEntity> GetPage(ListCommand cmd)
		{
			int totalRecords;
			List<RemovedEntity> data = repository.GetPage(cmd, out totalRecords).ToList();
			return new ListResult<RemovedEntity>
			{
				Data = data,
				TotalRecords = totalRecords
			};
		}

		#endregion
	}
}
