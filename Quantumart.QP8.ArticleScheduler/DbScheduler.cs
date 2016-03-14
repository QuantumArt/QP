using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.ArticleScheduler;
using Quantumart.QP8.ArticleScheduler.Recurring;
using Quantumart.QP8.ArticleScheduler.Publishing;
using Quantumart.QP8.ArticleScheduler.Onetime;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.ArticleScheduler
{
	/// <summary>
	/// Класс выполняет задачи (расписания) из указанной БД
	/// </summary>
	public class DbScheduler
	{
		string connectionString;
		IUnityContainer unityContainer;
		
		public DbScheduler(string connectionString, IUnityContainer unityContainer)
		{
			if (String.IsNullOrEmpty(connectionString))
				throw new ArgumentNullException("connectionString");
			if(unityContainer == null)
				throw new ArgumentNullException("unityContainer");
			
			this.connectionString = connectionString;
			this.unityContainer = unityContainer;
		}

		/// <summary>
		/// Параллельно обработать задачи (расписания)
		/// </summary>
		public void ParallelRun()
		{
		    GetScheduleTaskActions()
				.AsParallel()
				.ForAll(action => action());
		}

		/// <summary>
		/// Обработать задачи (расписания)
		/// </summary>
		public void Run()
		{
			foreach(var action in GetScheduleTaskActions())
			{
				action();
			}
		}

		/// <summary>
		/// Получить список action для выполнения всех существующих в БД на текущий момент расписаний 
		/// </summary>
		/// <returns></returns>
		private IEnumerable<Action> GetScheduleTaskActions()
		{
			IEnumerable<ArticleScheduleTask> scheduleTasks = null;
			IArticleSchedulerService ashBllService = unityContainer.Resolve<IArticleSchedulerService>(new ParameterOverride("connectionString", connectionString));

			// Получить список задач из указанной БД						
			scheduleTasks = ashBllService.GetScheduleTaskList();
			
			return scheduleTasks.Select(task => CreateScheduleTaskAction(task))
				.ToArray(); // ToArray() обязателен, так как создание результирующей коллекции должно выполняться в одном потоке, при этом обработка результата уже может (и должна) быть обработанна параллельно
		}		

		/// <summary>
		/// Создать action для обработки задачи
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		private Action CreateScheduleTaskAction(ArticleScheduleTask task)
		{
			switch (task.FreqType)
			{
				case ScheduleFreqTypes.OneTime:
					return () => RunOnetimeTaskAction(task);
				case ScheduleFreqTypes.Publishing:
					return () => RunPublishingTaskAction(task);
				case ScheduleFreqTypes.RecurringDaily:
				case ScheduleFreqTypes.RecurringWeekly:
				case ScheduleFreqTypes.RecurringMonthly:
				case ScheduleFreqTypes.RecurringMonthlyRelative:
					return () => RunRecurringTaskAction(task);
				default:
					throw new ArgumentException("Undefined FreqType value: " + task.FreqType);
			}			
		}

		/// <summary>
		/// Выполнить Recurring задачу
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		private void RunRecurringTaskAction(ArticleScheduleTask task)
		{
			unityContainer
				.Resolve<RecurringTaskScheduler>(new ParameterOverride("connectionString", connectionString))
				.Run(RecurringTask.Create(task));
		}

		/// <summary>
		/// Выполнить Publishing задачу
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		private void RunPublishingTaskAction(ArticleScheduleTask task)
		{
			unityContainer
				.Resolve<PublishingTaskScheduler>(new ParameterOverride("connectionString", connectionString))
				.Run(PublishingTask.Create(task));
		}

		/// <summary>
		/// Выполнить Onetime задачу
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		private void RunOnetimeTaskAction(ArticleScheduleTask task)
		{
			unityContainer
				.Resolve<OnetimeTaskScheduler>(new ParameterOverride("connectionString", connectionString))
				.Run(OnetimeTask.Create(task));
		}
	}
}
