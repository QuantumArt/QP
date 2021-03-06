﻿using QP8.Infrastructure.Logging;
using Quartz;

namespace Quantumart.QP8.CdcDataImport.Common.Infrastructure.JobListeners
{
    public class TriggerListener : ITriggerListener
    {
        private const string LogStartMessage = "TRIGGER LISTENER:";

        public TriggerListener()
            : this("Trigger Listener")
        {
        }

        public TriggerListener(string triggerListenerName)
        {
            Name = triggerListenerName;
        }

        public string Name { get; }

        public void TriggerFired(ITrigger trigger, IJobExecutionContext context)
        {
            Logger.Log.Trace($"{LogStartMessage} Trigger {trigger.Key.Name} fired for job {context.JobDetail.Key.Name}");
        }

        public bool VetoJobExecution(ITrigger trigger, IJobExecutionContext context)
        {
            Logger.Log.Trace($"{LogStartMessage} Trigger {trigger.Key.Name} vetoed for job {context.JobDetail.Key.Name}");
            return false;
        }

        public void TriggerMisfired(ITrigger trigger)
        {
            Logger.Log.Trace($"{LogStartMessage} Trigger {trigger.Key.Name} misfired");
        }

        public void TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode)
        {
            Logger.Log.Trace($"{LogStartMessage} Trigger {trigger.Key.Name} completed for job {context.JobDetail.Key.Name} with code {triggerInstructionCode}");
        }
    }
}
