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
using Quantumart.QP8.Resources;
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

    public async Task<ExternalWorkflowActionResult> PublishWorkflow(string customerCode, int contentItemId, int siteId, CancellationToken token)
    {
        try
        {
            _logger.LogInformation("Publishing workflow schema for customer {CustomerCode} from article {ArticleId} on site {SiteId}",
                customerCode,
                contentItemId,
                siteId);

            if (!IsExternalWorkflowEnabled())
            {
                throw new ExternalWorkflowPublishException(ExternalWorkflowStrings.WorkflowDisabled);
            }

            Article workflow = ArticleRepository.GetById(contentItemId);
            List<FieldValue> workflowFields = workflow.LoadFieldValues();
            string workflowName = workflowFields.SingleOrDefault(f => f.Field.Name == WorkflowIdentityFieldName)?.Value;

            if (string.IsNullOrEmpty(workflowName))
            {
                throw new ExternalWorkflowPublishException(ExternalWorkflowStrings.WorkflowNameNotFound);
            }

            Deployment deployment = new()
            {
                TenantId = customerCode,
                ProcessName = workflowName
            };

            int schemaNameFieldId = DbRepository.GetAppSettings<int>(SchemaNameFieldIdSettingName);

            if (schemaNameFieldId == 0)
            {
                throw new ExternalWorkflowPublishException(ExternalWorkflowStrings.SchemaNameFieldNotSpecifiedInSettings);
            }

            int schemaFileFieldId = DbRepository.GetAppSettings<int>(SchemaFileFieldIdSettingName);

            if (schemaFileFieldId == 0)
            {
                throw new ExternalWorkflowPublishException(ExternalWorkflowStrings.SchemaFileFieldNotScepifiedInSettings);
            }

            int[] schemas = workflowFields
                .SingleOrDefault(f => f.Field.Name == WorkflowSchemaRelationFieldName)?
                .RelatedItems ?? Array.Empty<int>();

            if (schemas.Length == 0)
            {
                throw new ExternalWorkflowPublishException(ExternalWorkflowStrings.NoSchemasAttachedToArticle);
            }

            foreach (int schema in schemas)
            {
                Article workflowSchema = ArticleRepository.GetById(schema);
                List<FieldValue> schemaFields = workflowSchema.LoadFieldValues();
                string schemaName = schemaFields.SingleOrDefault(f => f.Field.Id == schemaNameFieldId)?.Value;

                if (string.IsNullOrWhiteSpace(schemaName))
                {
                    throw new ExternalWorkflowPublishException(ExternalWorkflowStrings.SchemaNameNotSpecified);
                }

                FieldValue schemaFileField = schemaFields.SingleOrDefault(f => f.Field.Id == schemaFileFieldId);

                if (schemaFileField == null || string.IsNullOrWhiteSpace(schemaFileField.Value))
                {
                    throw new ExternalWorkflowPublishException(ExternalWorkflowStrings.SchemaNameNotSpecified);
                }

                string filePath = Path.Combine(schemaFileField.Field.PathInfo.Path, schemaFileField.Value);

                if (!File.Exists(filePath))
                {
                    throw new ExternalWorkflowPublishException(string.Format(ExternalWorkflowStrings.SchemaFileNotFoundTemplate, filePath));
                }

                byte[] fileBytes = await File.ReadAllBytesAsync(filePath, token);

                deployment.Files.Add(new() { Name = schemaName, FileName = Path.GetFileName(filePath), FileBytes = fileBytes });
            }

            bool result = await _deploymentService.CreateDeployment(deployment);

            return new() { Success = result, Message = result ? ExternalWorkflowStrings.SuccessfullyPublished : ExternalWorkflowStrings.PublishError };
        }
        catch (ExternalWorkflowPublishException ex)
        {
            return new() { Success = false, Message = ex.Message };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while publishing workflow");

            return new() { Success = false, Message = ExternalWorkflowStrings.PublishError };
        }
    }

    public async Task<ExternalWorkflowActionResult> StartProcess(string customerCode, int contentItemId, int contentId, CancellationToken token)
    {
        try
        {
            _logger.LogInformation("Starting processes for article id {Article} in content {Content} for customer code {CustomerCode}",
                contentItemId,
                contentId,
                customerCode);

            if (!IsExternalWorkflowEnabled())
            {
                throw new ExternalWorkflowStartException(ExternalWorkflowStrings.WorkflowDisabled);
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
            int externalWorkflowContentId = DbRepository.GetAppSettings<int>(ContentIdSettingName);

            if (externalWorkflowContentId == 0)
            {
                throw new ExternalWorkflowStartException(ExternalWorkflowStrings.WorkflowContentIdNotSpecifiedInSettings);
            }

            Content assignmentsContent = ContentRepository.GetById(externalWorkflowContentId);
            int fieldId = assignmentsContent.Fields.SingleOrDefault(f => f.Name == WorkflowContentRelationFieldName)?.Id ?? -1;

            if (fieldId == -1)
            {
                throw new ExternalWorkflowStartException(ExternalWorkflowStrings.WorkflowToContentRelationFieldNotFound);
            }

            Dictionary<int, string> workflowsToStart = ArticleRepository.GetRelatedItems(new[] { fieldId }, workflowContentId);

            if (workflowsToStart.All(x => string.IsNullOrWhiteSpace(x.Value)))
            {
                throw new ExternalWorkflowStartException(string.Format(ExternalWorkflowStrings.WorkflowToStartNotFoundTemplate, workflowContentId));
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
                    throw new ExternalWorkflowStartException(ExternalWorkflowStrings.ProcessNotStarted);
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

            return new() { Success = true, Message = ExternalWorkflowStrings.SuccessfullyStarted };
        }
        catch (ExternalWorkflowStartException ex)
        {
            return new() { Success = false, Message = ex.Message };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to start process");

            return new() { Success = false, Message = ExternalWorkflowStrings.StartError };
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
            List<UserTask> userTasks = await _workflowUserTaskService.GetUserTasks(userInfo.Login, userInfo.Roles, QPContext.CurrentCustomerCode);
            Dictionary<string, int> dbWorkflows = ExternalWorkflowRepository.GetExistingWorkflowIdsByProcessIds(userTasks.Select(x => x.ProcessId).Distinct().ToArray());

            return userTasks.Count(x => dbWorkflows.ContainsKey(x.ProcessId));
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
            List<UserTask> userTasks = await _workflowUserTaskService.GetUserTasks(userInfo.Login,
                userInfo.Roles,
                QPContext.CurrentCustomerCode);
            Dictionary<string, int> dbWorkflows = ExternalWorkflowRepository.GetExistingWorkflowIdsByProcessIds(userTasks.Select(x => x.ProcessId).Distinct().ToArray());
            List<UserTask> nonExistedTasks = userTasks.Where(x => !dbWorkflows.ContainsKey(x.ProcessId)).ToList();

            if (nonExistedTasks.Any())
            {
                _logger.LogWarning("Camunda contains processes with ids [{ProcessIds}] not presented in database", string.Join(", ", nonExistedTasks.Select(x => x.ProcessId)));
            }

            userTasks = userTasks.Where(x => dbWorkflows.ContainsKey(x.ProcessId)).ToList();

            UserTaskInfo taskInfo = new()
            {
                TotalCount = userTasks.Count
            };

            userTasks = userTasks.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            foreach (UserTask userTask in userTasks)
            {
                Dictionary<string, object> variables = await _workflowUserTaskService.GetTaskVariables(userTask.Id);
                int contentItemId = GetVariable<int>(variables, ExternalWorkflowQpDpcSettings.ContentItemId);
                Article article = ArticleRepository.GetById(contentItemId);

                UserTaskData data = new()
                {
                    Id = dbWorkflows[userTask.ProcessId],
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
        FieldValue workflowField = assignedWorkflow.FieldValues
           .SingleOrDefault(f => f.Field.Name == AssignmentToWorkflowRelationFieldName);

        if (workflowField is null)
        {
            throw new ExternalWorkflowStartException(ExternalWorkflowStrings.WorkflowAssignmentFieldNotFound);
        }

        int workflowId = workflowField.RelatedItems.SingleOrDefault(-1);

        if (workflowId == -1)
        {
            throw new ExternalWorkflowStartException(ExternalWorkflowStrings.WorkflowNoSetToAssignment);
        }

        Article workflowInfo = ArticleRepository.GetById(workflowId);
        int[] schemas = workflowInfo.FieldValues
            .SingleOrDefault(f => f.Field.Name == WorkflowSchemaRelationFieldName)?
            .RelatedItems ?? Array.Empty<int>();

        if (schemas.Length == 0)
        {
            throw new ExternalWorkflowStartException(ExternalWorkflowStrings.NoSchemasAttachedToArticle);
        }

        string definitionName = string.Empty;

        foreach (int schema in schemas)
        {
            Article schemaArticle = ArticleRepository.GetById(schema);
            FieldValue isMainField = schemaArticle.FieldValues.SingleOrDefault(f => f.Field.Name == MainSchemaFieldName);

            if (isMainField is null)
            {
                throw new ExternalWorkflowStartException(ExternalWorkflowStrings.IsMainSchemaFieldNotFound);
            }

            if (isMainField.Value == "0")
            {
                continue;
            }

            FieldValue definitionNameField = schemaArticle.FieldValues.SingleOrDefault(f => f.Field.Name == SchemaIdFieldName);

            if (definitionNameField is null)
            {
                throw new ExternalWorkflowStartException(ExternalWorkflowStrings.SchemaIdFieldNotFound);
            }

            definitionName = definitionNameField.Value;

            break;
        }

        if (string.IsNullOrWhiteSpace(definitionName))
        {
            throw new ExternalWorkflowStartException(ExternalWorkflowStrings.DefinitionNameNotFound);
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
        string actualContentId = article.FieldValues.SingleOrDefault(f => f.Field.ExactType == FieldExactTypes.Classifier)?.Value;

        if (string.IsNullOrWhiteSpace(actualContentId) || !int.TryParse(actualContentId, out int workflowContentId))
        {
            throw new ExternalWorkflowStartException(ExternalWorkflowStrings.UnableToGetContentIdByClassifier);
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
