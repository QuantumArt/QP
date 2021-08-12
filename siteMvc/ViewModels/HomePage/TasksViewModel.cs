using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Scheduler.API.Models;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.HomePage
{
    public class ScheduledTasksViewModel : AreaViewModel
    {

        public static ScheduledTasksViewModel Create(string tabId, int parentId, List<JobInfo> tasks)
        {
            var model = Create<ScheduledTasksViewModel>(tabId, parentId);
            model.Tasks = tasks;
            return model;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.CustomerCode;

        public override string ActionCode => Constants.ActionCode.ScheduledTasks;
        public List<JobInfo> Tasks { get; set; }
        
        public bool CanManageScheduledTasks => QPContext.CanManageScheduledTasks;
    }

    public class ScheduledTaskViewModel
    {
        public string Name { get; set; }
    }
}
