using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QA.Workflow.Integration.QP.Models;
using QA.Workflow.TaskWorker.Extensions;
using QA.Workflow.TaskWorker.Interfaces;
using Quantumart.QP8.BLL.Services.ExternalWorkflow;
using Quantumart.QP8.BLL.Services.ExternalWorkflow.ExternalTasks;
using Quantumart.QP8.BLL.Services.ExternalWorkflow.Models;
using Quantumart.QP8.BLL.Services.ExternalWorkflow.UserTasks;

namespace Quantumart.QP8.WebMvc.Extensions.ServiceCollections;

public static class ExternalWorkflowRegistration
{
    public static IServiceCollection RegisterExternalWorkflow(this IServiceCollection services, IConfiguration configuration)
    {
        ExternalTaskProcessWatcherConfig settings = new();
        configuration.GetSection("Camunda").Bind(settings);

        if (!settings.IsEnabled)
        {
            return services;
        }

        services.AddSingleton(Options.Create(settings));
        services.AddSingleton(Options.Create((ExtendedCamundaSettings)settings));
        services.AddHostedService<TenantWatcherHostedService>();
        services.AddHostedService<ExternalWorkflowProcessWatcher>();
        services.AddScoped<IExternalWorkflowService, ExternalWorkflowService>();
        IExternalTaskCollection taskCollection = services.RegisterCamundaExternalTaskWorker(configuration);

        services.AddSingleton<UpdateProcessStatus>();
        taskCollection.Register<UpdateProcessStatus>();

        services.AddSingleton<SendNotification>();
        taskCollection.Register<SendNotification>();

        UserTaskCollection userTasks = new();
        services.AddSingleton<IUserTaskCollection>(userTasks);

        services.AddScoped<FillArticleUserTask>();
        userTasks.Register<FillArticleUserTask>();

        services.AddScoped<ApproveUserTask>();
        userTasks.Register<ApproveUserTask>();

        return services;
    }
}
