using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services
{
    public interface IWorkflowService
    {
        ListResult<WorkflowListItem> GetWorkflowsBySiteId(ListCommand cmd, int siteId);

        InitListResult InitList(int parentId);

        Workflow ReadProperties(int id);

        Workflow ReadPropertiesForUpdate(int id);

        Workflow UpdateProperties(Workflow workflow, IEnumerable<int> activeContents, IEnumerable<int> activeStatuses);

        IEnumerable<StatusType> GetStatusTypesBySiteId(int siteId);

        IEnumerable<Content> GetContentsBySiteId(int siteId);

        IEnumerable<int> GetBindedContetnsIds(int workflowId);

        Workflow NewWorkflowProperties(int parentId);

        Workflow NewWorkflowPropertiesForUpdate(int parentId);

        Workflow SaveWorkflowProperties(Workflow workflow);

        ContentInitListResult InitUnionSourceList(int parentId);

        MessageResult Remove(int id);

        bool IsContentAccessibleForUser(int contentId, int userId);

        bool IsContentAccessibleForUserGroup(int contentId, int userGroupId);

        string GetContentNameById(int contentId);
    }

    public class WorkflowService : IWorkflowService
    {
        public ListResult<WorkflowListItem> GetWorkflowsBySiteId(ListCommand cmd, int siteId)
        {
            var list = WorkflowRepository.GetSiteWorkflowsPage(cmd, siteId, out var totalRecords);
            return new ListResult<WorkflowListItem>
            {
                Data = list.ToList(),
                TotalRecords = totalRecords
            };
        }

        public InitListResult InitList(int parentId) => new InitListResult
        {
            IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewWorkflow) &&
                SecurityRepository.IsEntityAccessible(EntityTypeCode.Workflow, parentId, ActionTypeCode.Update)
        };

        public Workflow ReadProperties(int id)
        {
            var workflow = WorkflowRepository.GetById(id);
            if (workflow == null)
            {
                throw new ApplicationException(string.Format(WorkflowStrings.WorkflowNotFound, id));
            }

            return workflow;
        }

        public Workflow ReadPropertiesForUpdate(int id) => ReadProperties(id);

        public Workflow UpdateProperties(Workflow workflow, IEnumerable<int> activeContentsIds, IEnumerable<int> activeStatuses)
        {
            workflow.BindWithContent(activeContentsIds);
            return WorkflowRepository.UpdateProperties(workflow, activeStatuses);
        }

        public IEnumerable<StatusType> GetStatusTypesBySiteId(int siteId) => StatusTypeRepository.GetStatusList(siteId);

        public IEnumerable<Content> GetContentsBySiteId(int siteId) => ContentRepository.GetListBySiteId(siteId).ToList();

        public IEnumerable<int> GetBindedContetnsIds(int workflowId) => WorkflowRepository.GetBindedContetnsIds(workflowId);

        public Workflow NewWorkflowProperties(int parentId) => Workflow.Create(parentId);

        public Workflow NewWorkflowPropertiesForUpdate(int parentId) => NewWorkflowProperties(parentId);

        public Workflow SaveWorkflowProperties(Workflow workflow)
        {
            var result = WorkflowRepository.SaveProperties(workflow);
            return result;
        }

        public ContentInitListResult InitUnionSourceList(int parentId)
        {
            var site = SiteRepository.GetById(parentId);
            if (site == null)
            {
                throw new Exception(string.Format(SiteStrings.SiteNotFound, parentId));
            }

            return new ContentInitListResult { ParentName = site.Name };
        }

        public static ListResult<ContentListItem> GetAcceptableContentForWorkflow(ContentListFilter filter, ListCommand cmd, int[] selectedContentIDs) => ContentRepository.GetList(filter, cmd, selectedContentIDs);

        public MessageResult Remove(int id)
        {
            WorkflowRepository.Delete(id);
            return null;
        }

        public bool IsContentAccessibleForUser(int contentId, int userId) => SecurityRepository.IsEntityAccessible(EntityTypeCode.Content, contentId, ActionTypeCode.Update, userId);

        public bool IsContentAccessibleForUserGroup(int contentId, int userGroupId) => SecurityRepository.IsEntityAccessibleForUserGroup(EntityTypeCode.Content, contentId, ActionTypeCode.Update, userGroupId);

        public string GetContentNameById(int contentId) => ContentRepository.GetById(contentId).Name;
    }
}
