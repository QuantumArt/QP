using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QA.Workflow.Interfaces;
using QA.Workflow.Models;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Services.ExternalWorkflow.Models;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL.Entities;
using UserTask = QA.Workflow.Models.UserTask;
using UserTaskInfo = Quantumart.QP8.BLL.Services.ExternalWorkflow.Models.UserTasksInfo;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow;

public class ExternalWorkflowService : IExternalWorkflowService
{
    public const string ExternalWorkflowSettingName = "EXTERNAL_WORKFLOW";
    private const string ContentIdSettingName = "EXTERNAL_WORKFLOW_CONTENT_ID";
    private const string WorkflowContentRelationFieldName = "ContentId";
    private const string AssignmentToWorkflowRelationFieldName = "Workflow";
    private const string WorkflowSchemaRelationFieldName = "WorkflowSchemas";
    private const string WorkflowIdentityFieldName = "WorkflowID";
    private const string MainSchemaFieldName = "IsMainSchema";
    private const string SchemaIdFieldName = "SchemaId";
    private const string ContentItemIdName = "ContentItemId";
    private const string ContentIdName = "ContentId";

    private readonly IWorkflowDeploymentService _deploymentService;
    private readonly ILogger<ExternalWorkflowService> _logger;
    private readonly IWorkflowProcessService _workflowProcessService;
    private readonly IWorkflowUserTaskService _workflowUserTaskService;

    public ExternalWorkflowService(IWorkflowDeploymentService deploymentService,
        ILogger<ExternalWorkflowService> logger,
        IWorkflowProcessService workflowProcessService,
        IWorkflowUserTaskService workflowUserTaskService)
    {
        _deploymentService = deploymentService;
        _logger = logger;
        _workflowProcessService = workflowProcessService;
        _workflowUserTaskService = workflowUserTaskService;
    }

    public async Task<bool> PublishWorkflow(string customerCode, int contentItemId, int siteId, CancellationToken token)
    {
        try
        {
            Article workflow = ArticleRepository.GetById(contentItemId);
            List<FieldValue> workflowFields = workflow.LoadFieldValues();
            string workflowName = workflowFields.Single(f => f.Field.Name == WorkflowIdentityFieldName).Value;
            Deployment deployment = new()
            {
                TenantId = customerCode,
                ProcessName = workflowName
            };

            int[] schemas = workflowFields.Single(f => f.Field.Name == WorkflowSchemaRelationFieldName).RelatedItems;
            Site site = SiteRepository.GetById(siteId);

            foreach (int schema in schemas)
            {
                Article workflowSchema = ArticleRepository.GetById(schema);
                List<FieldValue> schemaFields = workflowSchema.LoadFieldValues();
                string schemaName = schemaFields.Single(f => f.Field.Name == "Title").Value;
                string fileName = schemaFields.Single(f => f.Field.Name == "SchemaFile").Value;
                byte[] fileBytes = await File.ReadAllBytesAsync(Path.Combine(site.UploadDir, fileName), token);

                deployment.Files.Add(new() { Name = schemaName, FileName = fileName, FileBytes = fileBytes});
            }
            bool result = await _deploymentService.CreateDeployment(deployment);

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while publishing workflow.");

            return false;
        }
    }

    public async Task<bool> StartProcess(string customerCode, int contentItemId, int contentId, CancellationToken token)
    {
        try
        {
            int workflowContentId = GetContentIdForWorkflow(contentItemId, contentId);
            int externalWorkflowContentId = GetValueFromQpConfig<int>(ContentIdSettingName);

            Content assignmentsContent = ContentRepository.GetById(externalWorkflowContentId);
            int fieldId = assignmentsContent.Fields.Single(f => f.Name == WorkflowContentRelationFieldName).Id;
            Dictionary<int, string> workflowsToStart = ArticleRepository.GetRelatedItems(new int[1] {fieldId}, workflowContentId);

            if (workflowsToStart.Count == 0)
            {
                _logger.LogWarning("Unable to find assigned workflow for content {Content}.", workflowContentId);

                return true;
            }

            foreach (string workflowToStart in workflowsToStart.Values)
            {
                string definitionName = GetWorkflowDefinitionToStart(workflowToStart);

                ProcessDefinitionRequest request = new()
                {
                    Key = definitionName,
                    Version = "latest"
                };

                Dictionary<string, object> variables = new()
                {
                    { "ContentItemId", contentItemId },
                    { "ContentId", contentId }
                };

                bool result = await _workflowProcessService.StartProcessInstance(request,
                    string.Empty,
                    customerCode,
                    variables);

                if (!result)
                {
                    throw new InvalidOperationException("Process not started.");
                }
            }

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to start process.");

            return false;
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
            _logger.LogError(e, "Error while loading user tasks count.");

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
                int contentItemId = GetVariable<int>(variables, ContentItemIdName);
                int contentId = GetVariable<int>(variables, ContentIdName);

                Content content = ContentRepository.GetById(contentId);
                Article article = ArticleRepository.GetById(contentItemId);

                UserTaskData data = new()
                {
                    TaskId = userTask.Id,
                    ProcessId = userTask.ProcessId,
                    TaskName = userTask.Name,
                    Id = contentItemId,
                    ContentName = content.Name,
                    ItemName = article.Name,
                    ParentId = article.Parent.Id,
                    SiteName = content.Site.Name
                };

                taskInfo.Data.Add(data);
            }

            return taskInfo;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while loading user tasks list.");

            throw;
        }
    }

    private static T GetVariable<T>(Dictionary<string, object> variables, string name)
    {
        if (!variables.TryGetValue(name, out object variable))
        {
            throw new InvalidOperationException($"Unable to find variable {name} in task variables.");
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
        info.Roles.AddRange(user.Groups.Select(x => x.Name).ToList());

        foreach (UserGroup userGroup in user.Groups)
        {
            UserGroup parentGroup = userGroup.ParentGroup;

            while (parentGroup != null)
            {
                info.Roles.Add(parentGroup.Name);
                parentGroup = parentGroup.ParentGroup;
            }
        }

        return info;
    }

    private static string GetWorkflowDefinitionToStart(string workflow)
    {
        if (!int.TryParse(workflow, out int workflowArticleId))
        {
            throw new InvalidOperationException("Unable to parse workflow assignment article id to int.");
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

        return definitionName;
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

    private static T GetValueFromQpConfig<T>(string name)
    {
        AppSettingsDAL setting = QPContext.EFContext.AppSettingsSet
           .FirstOrDefault(x => x.Key == name);

        if (setting is null || string.IsNullOrWhiteSpace(setting.Value))
        {
            throw new InvalidOperationException($"Unable to find setting {name} in QP settings.");
        }

        TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
        return (T)converter.ConvertFromString(null, CultureInfo.InvariantCulture, setting.Value);
    }

    public static bool IsExternalWorkflowEnabled()
    {
        AppSettingsDAL externalWorkflowSetting = QPContext.EFContext.AppSettingsSet.FirstOrDefault(x => x.Key == ExternalWorkflowSettingName);

        return externalWorkflowSetting != null && bool.TryParse(externalWorkflowSetting.Value, out bool externalWorkflowEnabled) && externalWorkflowEnabled;
    }
}
