using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using Quantumart.QP8.ArticleScheduler.Onetime;
using Quantumart.QP8.ArticleScheduler.Publishing;
using Quantumart.QP8.ArticleScheduler.Recurring;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.ArticleScheduler;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.ArticleScheduler
{
    public class DbScheduler
    {
        private readonly string _connectionString;
        private readonly IUnityContainer _unityContainer;

        public DbScheduler(string connectionString, IUnityContainer unityContainer)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (unityContainer == null)
            {
                throw new ArgumentNullException(nameof(unityContainer));
            }

            _connectionString = connectionString;
            _unityContainer = unityContainer;
        }

        /// <summary>
        /// Параллельно обработать задачи (расписания)
        /// </summary>
        public void ParallelRun()
        {
            GetScheduleTaskActions().AsParallel().ForAll(action => action());
        }

        /// <summary>
        /// Обработать задачи (расписания)
        /// </summary>
        public void Run()
        {
            foreach (var action in GetScheduleTaskActions())
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
            var ashBllService = _unityContainer.Resolve<IArticleSchedulerService>(new ParameterOverride("connectionString", _connectionString));

            // Получить список задач из указанной БД
            var scheduleTasks = ashBllService.GetScheduleTaskList();

            // ToArray() обязателен, так как создание результирующей коллекции должно выполняться в одном потоке, при этом обработка результата уже может (и должна) быть обработанна параллельно
            return scheduleTasks.Select(CreateScheduleTaskAction).ToArray();
        }

        /// <summary>
        /// Создать action для обработки задачи
        /// </summary>
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
        private void RunRecurringTaskAction(ArticleScheduleTask task)
        {
            _unityContainer.Resolve<RecurringTaskScheduler>(new ParameterOverride("connectionString", _connectionString)).Run(RecurringTask.Create(task));
        }

        /// <summary>
        /// Выполнить Publishing задачу
        /// </summary>
        private void RunPublishingTaskAction(ArticleScheduleTask task)
        {
            _unityContainer.Resolve<PublishingTaskScheduler>(new ParameterOverride("connectionString", _connectionString)).Run(PublishingTask.Create(task));
        }

        /// <summary>
        /// Выполнить Onetime задачу
        /// </summary>
        private void RunOnetimeTaskAction(ArticleScheduleTask task)
        {
            _unityContainer.Resolve<OnetimeTaskScheduler>(new ParameterOverride("connectionString", _connectionString)).Run(OnetimeTask.Create(task));
        }
    }
}
