using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.DAL;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using System.Transactions;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services.ArticleScheduler
{
	public class ArticleSchedulerService : IArticleSchedulerService, IArticleOnetimeSchedulerService, IArticlePublishingSchedulerService, IArticleRecurringSchedulerService
	{
		string connectionString;

		public ArticleSchedulerService(string connectionString)
		{
			if (String.IsNullOrEmpty(connectionString))
				throw new ArgumentNullException("connectionString");

			this.connectionString = connectionString;
		}		

		public IEnumerable<ArticleScheduleTask> GetScheduleTaskList()
		{			
			using (new QPConnectionScope(connectionString))
			{
				return ScheduleRepository.GetScheduleTaskList();
			}
		}

		public DateTime GetCurrentDBDateTime()
		{						
			using (var scope = new QPConnectionScope(connectionString))
			{
				return Common.GetSqlDate(scope.DbConnection);
			}		
		}

		public Article ShowArticle(int articleId)
		{			
			using (new QPConnectionScope(connectionString))
			{
				var article = ArticleRepository.GetById(articleId);
				if (article != null && !article.Visible) {
					QPContext.EFContext.SetContentItemVisible(articleId, true);
				}

				if (article != null)
					article.LoadFieldValues();
				return article;
			}		
		}

		public Article HideArticle(int articleId)
		{
			using (new QPConnectionScope(connectionString))
			{
				var article = ArticleRepository.GetById(articleId);
				if (article != null && article.Visible) {
					QPContext.EFContext.SetContentItemVisible(articleId, false);
				}

				if (article != null)
					article.LoadFieldValues();
				return article;
			}
		}

		public Article ShowAndCloseSchedule(int scheduleId)
		{
			Article article = null; 
			using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions {IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted}))
			{
				ArticleSchedule schedule = null;
				using (new QPConnectionScope(connectionString))
				{
					schedule = ScheduleRepository.GetScheduleById(scheduleId);
					if (schedule != null)
					{
						article = ShowArticle(schedule.ArticleId);
						schedule.Article = article;
						ScheduleRepository.Delete(schedule);
					}
				}
				transaction.Complete();
			}


			if (article != null)
			{
				article.SendNotification(NotificationCode.Update, connectionString, true);
			}

			return article;
		}

		public Article HideAndCloseSchedule(int scheduleId)
		{
			Article article = null; 
			using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
			{
				ArticleSchedule schedule = null;
				using (new QPConnectionScope(connectionString))
				{
					schedule = ScheduleRepository.GetScheduleById(scheduleId);
					if (schedule != null)
					{
						article = HideArticle(schedule.ArticleId);
						schedule.Article = article;
						ScheduleRepository.Delete(schedule);
					}
				}
				transaction.Complete();
			}

			if (article != null)
			{
				article.SendNotification(NotificationCode.Update, connectionString, true);
			}

			return article;
		}

		public Article PublishAndCloseSchedule(int scheduleId)
		{
			Article article = null; 
			using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
			{

				using (new QPConnectionScope(connectionString))
				{
					ArticleSchedule schedule = ScheduleRepository.GetScheduleById(scheduleId);
					if (schedule != null)
					{
						article = ArticleRepository.GetById(schedule.ArticleId);
						schedule.Article = article;
						if (article != null && article.Delayed)
						{
							QPContext.EFContext.MergeArticle(schedule.ArticleId);
						}
						else
							ScheduleRepository.Delete(schedule);

						if (article != null)
							article.LoadFieldValues();
					}
				}
				transaction.Complete();
			}

			if (article != null)
			{
				article.SendNotification(NotificationCode.DelayedPublication, connectionString, true);
			}

			return article;
		}					
	}
}
