using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using QA.Workflow.Interfaces;
using QA.Workflow.Models;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants.Mvc;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow;

public class ExternalWorkflowService : IExternalWorkflowService
{
    private readonly IWorkflowDeploymentService _deploymentService;
    private readonly ILogger<ExternalWorkflowService> _logger;
    private readonly IHttpContextAccessor _contextAccessor;

    public ExternalWorkflowService(IWorkflowDeploymentService deploymentService,
        ILogger<ExternalWorkflowService> logger,
        IHttpContextAccessor contextAccessor)
    {
        _deploymentService = deploymentService;
        _logger = logger;
        _contextAccessor = contextAccessor;
    }

    public async Task<bool> PublishWorkflow(string customerCode, int contentItemId, int siteId, CancellationToken token)
    {
        try
        {
            QpConnectionInfo cnnInfo = QPConfiguration.GetConnectionInfo(customerCode);
            _contextAccessor.HttpContext.Items.Add(HttpContextItems.CurrentDbConnectionStringKey, cnnInfo);
            Article workflow = ArticleRepository.GetById(contentItemId);

            List<FieldValue> workflowFields = workflow.LoadFieldValues();
            string workflowName = workflowFields.Single(f => f.Field.Name == "WorkflowID").Value;
            Deployment deployment = new()
            {
                TenantId = customerCode,
                ProcessName = workflowName
            };

            int[] schemas = workflowFields.Single(f => f.Field.Name == "WorkflowSchemas").RelatedItems;
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

            await _deploymentService.CreateDeployment(deployment);

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while publishing workflow.");

            return false;
        }

    }
}
