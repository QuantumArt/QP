using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QA.Workflow.Integration.QP.Models;
using QA.Workflow.Interfaces;
using QA.Workflow.Models;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Services.ExternalWorkflow.Models;
using Quantumart.QP8.Constants;
using UserTask = QA.Workflow.Models.UserTask;
using UserTaskInfo = Quantumart.QP8.BLL.Services.ExternalWorkflow.Models.UserTasksInfo;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow;

public class ExternalWorkflowService : IExternalWorkflowService
{
    private const string ContentIdSettingName = "EXTERNAL_WORKFLOW_CONTENT_ID";
    private const string SchemaNameFieldIdSettingName = "EXTERNAL_WORKFLOW_SCHEMA_NAME_FIELD_ID";
    private const string SchemaFileFieldIdSettingName = "EXTERNAL_WORKFLOW_SCHEMA_FILE_FIELD_ID";
    private const string WorkflowContentRelationFieldName = "ContentId";
    private const string AssignmentToWorkflowRelationFieldName = "Workflow";
    private const string WorkflowSchemaRelationFieldName = "WorkflowSchemas";
    private const string WorkflowIdentityFieldName = "WorkflowID";
    private const string MainSchemaFieldName = "IsMainSchema";
    private const string SchemaIdFieldName = "SchemaId";
    private const string ManuallyStoppingProcessStatus = "Остановка процесса по причине запуска нового процесса";
    private const string ExternalWorkflowEnabledParameterName = "EXTERNAL_WORKFLOW";

    private readonly IWorkflowDeploymentService _deploymentService;
    private readonly ILogger<ExternalWorkflowService> _logger;
    private readonly IWorkflowProcessService _workflowProcessService;
    private readonly IWorkflowUserTaskService _workflowUserTaskService;
    private readonly IUserTaskCollection _userTaskCollection;
    private readonly IServiceProvider _serviceProvider;

    public ExternalWorkflowService(IWorkflowDeploymentService deploymentService,
        ILogger<ExternalWorkflowService> logger,
        IWorkflowProcessService workflowProcessService,
        IWorkflowUserTaskService workflowUserTaskService,
        IUserTaskCollection userTaskCollection,
        IServiceProvider serviceProvider)
    {
        _deploymentService = deploymentService;
        _logger = logger;
        _workflowProcessService = workflowProcessService;
        _workflowUserTaskService = workflowUserTaskService;
        _userTaskCollection = userTaskCollection;
        _serviceProvider = serviceProvider;
    }

    public async Task<bool> PublishWorkflow(string customerCode, int contentItemId, int siteId, CancellationToken token)
    {
        try
        {
            _logger.LogInformation("Publishing workflow schema for customer {CustomerCode} from article {ArticleId} on site {SiteId}",
                customerCode,
                contentItemId,
                siteId);

            if (!IsExternalWorkflowEnabled())
            {
                throw new InvalidOperationException("External workflow is not enabled");
            }

            Article workflow = ArticleRepository.GetById(contentItemId);
            List<FieldValue> workflowFields = workflow.LoadFieldValues();
            string workflowName = workflowFields.Single(f => f.Field.Name == WorkflowIdentityFieldName).Value;
            Deployment deployment = new()
            {
                TenantId = customerCode,
                ProcessName = workflowName
            };

            int schemaNameFieldId = DbRepository.GetAppSettings<int>(SchemaNameFieldIdSettingName, true);
            int schemaFileFieldId = DbRepository.GetAppSettings<int>(SchemaFileFieldIdSettingName, true);

            int[] schemas = workflowFields.Single(f => f.Field.Name == WorkflowSchemaRelationFieldName).RelatedItems;

            foreach (int schema in schemas)
            {
                Article workflowSchema = ArticleRepository.GetById(schema);
                List<FieldValue> schemaFields = workflowSchema.LoadFieldValues();
                string schemaName = schemaFields.Single(f => f.Field.Id == schemaNameFieldId).Value;
                FieldValue schemaFileField = schemaFields.Single(f => f.Field.Id == schemaFileFieldId);
                string filePath = Path.Combine(schemaFileField.Field.PathInfo.Path, schemaFileField.Value);

                if (!File.Exists(filePath))
                {
                    throw new ArgumentException($"Unable to locate file in path {filePath}", filePath);
                }

                byte[] fileBytes = await File.ReadAllBytesAsync(filePath, token);

                deployment.Files.Add(new() { Name = schemaName, FileName = Path.GetFileName(filePath), FileBytes = fileBytes});
            }
            bool result = await _deploymentService.CreateDeployment(deployment);

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while publishing workflow");

            return false;
        }
    }

    public async Task<bool> StartProcess(string customerCode, int contentItemId, int contentId, CancellationToken token)
    {
        try
        {
            _logger.LogInformation("Starting processes for article id {Article} in content {Content} for customer code {CustomerCode}",
                contentItemId,
                contentId,
                customerCode);

            if (!IsExternalWorkflowEnabled())
            {
                throw new InvalidOperationException("External workflow is not enabled");
            }

            QPContext.IsAdmin = true;

            List<ListItem> list = ArticleRepository.GetSimpleList(new()
            {
                EntityId = contentItemId.ToString(),
                ParentEntityId = contentId,
                SelectedEntitiesIds = new[] { contentItemId },
                SelectionMode = ListSelectionMode.OnlySelectedItems
            });

            _logger.LogTrace("Article display name {Name}",
                list.Count == 0 ? "not found" : list.First().Text);

            int workflowContentId = GetContentIdForWorkflow(contentItemId, contentId);
            int externalWorkflowContentId = DbRepository.GetAppSettings<int>(ContentIdSettingName, true);

            Content assignmentsContent = ContentRepository.GetById(externalWorkflowContentId);
            int fieldId = assignmentsContent.Fields.Single(f => f.Name == WorkflowContentRelationFieldName).Id;
            Dictionary<int, string> workflowsToStart = ArticleRepository.GetRelatedItems(new[] { fieldId }, workflowContentId);

            if (workflowsToStart.Count == 0)
            {
                _logger.LogWarning("Unable to find assigned workflow for content {Content}", workflowContentId);

                return true;
            }

            List<string> processesToStop = await GetActiveProcessIdsByContentItemId(customerCode,
                contentId,
                contentItemId,
                token);

            await StopProcesses(customerCode, processesToStop, token);

            foreach (string workflowToStart in workflowsToStart.Values)
            {
                _logger.LogInformation("Starting workflow {Workflow}", workflowToStart);

                WorkflowToStart workflow = GetWorkflowDefinitionToStart(workflowToStart);

                ProcessDefinitionRequest request = new()
                {
                    Key = workflow.DefinitionName,
                    Version = "latest"
                };

                Dictionary<string, object> variables = new()
                {
                    { "ContentItemId", contentItemId },
                    { "ContentId", contentId }
                };

                string result = await _workflowProcessService.StartProcessInstance(request,
                    string.Empty,
                    customerCode,
                    variables);

                if (string.IsNullOrWhiteSpace(result))
                {
                    throw new InvalidOperationException("Process not started");
                }

                try
                {
                    ExternalWorkflowRepository.SaveStartedWorkflowInfoToDb(result,
                    workflow.WorkflowName,
                    list.Count == 0 ? contentItemId.ToString() : list.First().Text,
                    workflow.WorkflowId,
                    contentItemId);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while saving workflow process info to db");
                }
            }

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to start process");

            return false;
        }
        finally
        {
            QPContext.IsAdmin = false;
        }
    }


    public async Task<int> GetTaskCount()
    {
        try
        {
            if (!IsExternalWorkflowEnabled())
            {
                return 0;
            }

            UserInfo userInfo = GetUserInfo();

            return await _workflowUserTaskService.GetUserTasksCount(userInfo.Login, userInfo.Roles, QPContext.CurrentCustomerCode);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while loading user tasks count");

            return 0;
        }
    }

    public async Task<UserTaskInfo> GetUserTasks(int page, int pageSize)
    {
        try
        {
            UserInfo userInfo = GetUserInfo();
            List<UserTask> userTasks = await _workflowUserTaskService.GetUserTasks(userInfo.Login, userInfo.Roles, QPContext.CurrentCustomerCode);

            UserTaskInfo taskInfo = new()
            {
                TotalCount = userTasks.Count
            };
            userTasks = userTasks.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            foreach (UserTask userTask in userTasks)
            {
                Dictionary<string, object> variables = await _workflowUserTaskService.GetTaskVariables(userTask.Id);
                int contentItemId = GetVariable<int>(variables, ExternalWorkflowQpDpcSettings.ContentItemId);
                int contentId = GetVariable<int>(variables, ExternalWorkflowQpDpcSettings.ContentId);

                Article article = ArticleRepository.GetById(contentItemId);
                var id = ExternalWorkflowRepository.GetId(userTask.ProcessId);

                UserTaskData data = new()
                {
                    Id = id,
                    TaskId = userTask.Id,
                    ProcessId = userTask.ProcessId,
                    TaskName = userTask.Name,
                    ParentId = contentItemId,
                    Name = article.Name,
                    ContentName = article.Content.Name,
                    SiteName = article.Content.Site.Name
                };

                taskInfo.Data.Add(data);
            }

            return taskInfo;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while loading user tasks list");

            throw;
        }
    }

    public async Task CompleteUserTask(string taskId, Dictionary<string, object> variables)
    {
        bool completionResult = await _workflowUserTaskService.CompleteUserTask(taskId, variables);

        if (!completionResult)
        {
            throw new InvalidOperationException($"Unable to complete task with id {taskId}");
        }
    }

    public T GetVariable<T>(Dictionary<string, object> variables, string name)
    {
        if (!variables.TryGetValue(name, out object variable))
        {
            throw new InvalidOperationException($"Unable to find variable {name} in task variables");
        }

        return (T)Convert.ChangeType(variable, typeof(T));
    }

    private static UserInfo GetUserInfo()
    {
        UserService userService = new();
        User user = userService.ReadProfile(QPContext.CurrentUserId);

        UserInfo info = new()
        {
            Login = user.LogOn
        };

        foreach (int groupId in QPContext.CurrentGroupIds)
        {
            info.Roles.Add(UserGroupRepository.GetById(groupId).Name);
        }

        return info;
    }

    private static WorkflowToStart GetWorkflowDefinitionToStart(string workflow)
    {
        if (!int.TryParse(workflow, out int workflowArticleId))
        {
            throw new InvalidOperationException("Unable to parse workflow assignment article id to int");
        }

        Article assignedWorkflow = ArticleRepository.GetById(workflowArticleId);
        int workflowId = assignedWorkflow.FieldValues
           .Single(f => f.Field.Name == AssignmentToWorkflowRelationFieldName)
           .RelatedItems
           .Single();

        Article workflowInfo = ArticleRepository.GetById(workflowId);
        int[] schemas = workflowInfo.FieldValues
           .Single(f => f.Field.Name == WorkflowSchemaRelationFieldName)
           .RelatedItems;

        string definitionName = string.Empty;

        foreach (int schema in schemas)
        {
            Article schemaArticle = ArticleRepository.GetById(schema);
            string isMain = schemaArticle.FieldValues.Single(f => f.Field.Name == MainSchemaFieldName).Value;

            if (isMain == "0")
            {
                continue;
            }

            definitionName = schemaArticle.FieldValues.Single(f => f.Field.Name == SchemaIdFieldName).Value;
            break;
        }

        if (string.IsNullOrWhiteSpace(definitionName))
        {
            throw new InvalidOperationException("Unable to retrieve process key from QP.");
        }

        return new()
        {
            DefinitionName = definitionName,
            WorkflowId = workflowId,
            WorkflowName = workflowInfo.FieldValues.Single(x => x.Field.Name == "Title").Value
        };
    }

    private static int GetContentIdForWorkflow(int contentItemId, int contentId)
    {
        Content content = ContentRepository.GetById(contentId);
        Field classifier = content.Fields.FirstOrDefault(f => f.ExactType == FieldExactTypes.Classifier);

        if (classifier == null)
        {
            return contentId;
        }
        Article article = ArticleRepository.GetById(contentItemId);
        string actualContentId = article.FieldValues.Single(f => f.Field.ExactType == FieldExactTypes.Classifier).Value;

        if (!int.TryParse(actualContentId, out int workflowContentId))
        {
            throw new InvalidOperationException("Unable to get content id by classifier field");
        }

        return workflowContentId;
    }

    public static bool IsExternalWorkflowEnabled()
    {
        var result = DbRepository.GetAppSettings<string>(ExternalWorkflowQpDpcSettings.ExternalWorkflowSettingName);
        return result != null && bool.TryParse(result, out var externalWorkflowEnabled) && externalWorkflowEnabled;
    }

    public async Task<string> GetUserTaskKey(string taskId)
    {
        UserTask task = await _workflowUserTaskService.GetUserTaskById(taskId);

        return task?.FormKey;
    }

    public async Task<AbstractUserTask> GetUserTaskHandler(string taskId)
    {
        string taskKey = await GetUserTaskKey(taskId);

        if (string.IsNullOrWhiteSpace(taskKey))
        {
            throw new InvalidOperationException($"Unable to find task key for task {taskId}");
        }

        Type userTaskType = _userTaskCollection.UserTasks.SingleOrDefault(x => x.Name == taskKey);

        if (userTaskType is null)
        {
            throw new InvalidOperationException($"Unable to find registered user task type {taskKey}");
        }

        return (AbstractUserTask)_serviceProvider.GetRequiredService(userTaskType);
    }

    public async Task<Dictionary<string, object>> GetTaskVariables(string taskId) =>
        await _workflowUserTaskService.GetTaskVariables(taskId);

    private async Task<List<string>> GetActiveProcessIdsByContentItemId(string customerCode, int contentId, int contentItemId, CancellationToken token)
    {
        _logger.LogTrace("Retrieving active process instances ids for customer code {CustomerCode} with content id {ContentId} and content item id {ContentItemId}",
            customerCode,
            contentId,
            contentItemId);

        Dictionary<string, object> variables = new()
        {
            { "ContentId", contentId },
            { "ContentItemId", contentItemId }
        };

        List<string> processIds = await _workflowProcessService.GetProcessInstancesIdsByVariables(string.Empty, customerCode, variables);

        _logger.LogTrace("Found active process instances with ids: {ProcessIds}", string.Join(", ", processIds));

        return processIds;
    }

    private async Task StopProcesses(string customerCode, List<string> processesIds, CancellationToken token)
    {
        foreach (string processesId in processesIds)
        {
            _logger.LogTrace("Stopping process with Id {ProcessId}", processesId);

            try
            {
                ExternalWorkflowRepository.UpdateStatus(processesId, ManuallyStoppingProcessStatus);
                _logger.LogTrace("For process with id {ProcessId} was set status {Status}", processesId, ManuallyStoppingProcessStatus);
            }
            catch (ExternalWorkflowNotFoundInDbException e)
            {
                _logger.LogWarning(e, "Workflow with process id {ProcessId} was not found in database, but it will be stopped anyway", processesId);
            }

            await _workflowProcessService.DeleteProcessInstanceById(processesId, customerCode);

            _logger.LogTrace("Process with id {ProcessId} was stopped", processesId);
        }
    }
}
