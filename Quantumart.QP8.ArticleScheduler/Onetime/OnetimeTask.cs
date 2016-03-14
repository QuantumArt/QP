using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.ArticleScheduler.Onetime
{
	/// <summary>
	/// Задача единорозового расписания  
	/// </summary>
	public class OnetimeTask
	{
		/// <summary>
		/// Создать PublishingTask из ArticleScheduleTask
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static OnetimeTask Create(ArticleScheduleTask task)
		{
			if (task == null)
				throw new ArgumentNullException("task");

			if (task.FreqType != 1)
				throw new ArgumentException("Undefined FreqType value: " + task.FreqType);

			return new OnetimeTask(task.Id, task.ArticleId, 
				task.StartDate + task.StartTime, 
				task.EndDate + task.EndTime);
		}

		public OnetimeTask(int id, int articleId, DateTime startDateTime, DateTime endDateTime)
		{
			Id = id;
			ArticleId = articleId;

			StartDateTime = startDateTime;
			EndDateTime = endDateTime;
		}

		public int Id { get; private set; }

		public int ArticleId { get; private set; }

		public DateTime StartDateTime { get; private set; }

		public DateTime EndDateTime { get; private set; }
	}
}
