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
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow;

public class ExternalWorkflowService : IExternalWorkflowService
{
    private const string ContentIdSettingName = "EXTERNAL_WORKFLOW_CONTENT_ID";
    private const string WorkflowContentRelationFieldName = "ContentId";
    private const string AssignmentToWorkflowRelationFieldName = "Workflow";
    private const string WorkflowSchemaRelationFieldName = "WorkflowSchemas";
    private const string WorkflowIdentityFieldName = "WorkflowID";
    private const string MainSchemaFieldName = "IsMainSchema";
    private const string SchemaIdFieldName = "SchemaId";

    private readonly IWorkflowDeploymentService _deploymentService;
    private readonly ILogger<ExternalWorkflowService> _logger;
    private readonly IWorkflowProcessService _workflowProcessService;

    public ExternalWorkflowService(IWorkflowDeploymentService deploymentService,
        ILogger<ExternalWorkflowService> logger,
        IWorkflowProcessService workflowProcessService)
    {
        _deploymentService = deploymentService;
        _logger = logger;
        _workflowProcessService = workflowProcessService;
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
                _logger.LogWarning("Unable to find assigned workflow for content {content}.", workflowContentId);

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
}
