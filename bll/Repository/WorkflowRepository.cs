using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;
using Quantumart.QP8.Utils;
using EntityState = Microsoft.EntityFrameworkCore.EntityState;

namespace Quantumart.QP8.BLL.Repository
{
    internal class WorkflowRepository
    {
        internal static ContentWorkflowBindDAL GetContentWorkflowDal(int contentId, QPModelDataContext context = null)
        {
            var currentContext = context ?? QPContext.EFContext;
            return currentContext.ContentWorkflowBindSet.SingleOrDefault(s => s.ContentId == (decimal)contentId);
        }

        internal static ContentWorkflowBind GetContentWorkflow(int contentId) => MapperFacade.ContentWorkflowBindMapper.GetBizObject(GetContentWorkflowDal(contentId));

        internal static ContentWorkflowBind GetContentWorkflow(Content content)
        {
            var binding = GetContentWorkflow(content.Id);
            if (binding != null)
            {
                binding.Content = content;
            }
            else
            {
                binding = ContentWorkflowBind.Create(content);
            }

            return binding;
        }

        internal static ContentWorkflowBind GetDefaultWorkflow(Content content, QPModelDataContext context = null)
        {
            var currentContext = context ?? QPContext.EFContext;
            var workflowDal = currentContext.WorkflowSet.FirstOrDefault(s => s.IsDefault);
            if (workflowDal != null)
            {
                var workflow = MapperFacade.WorkflowMapper.GetBizObject(workflowDal);
                return new ContentWorkflowBind { Content = content, WorkflowId = workflow.Id };
            }

            return null;
        }

        internal static ArticleWorkflowBindDAL GetArticleWorkflowDal(int articleId)
        {
            return QPContext.EFContext.ArticleWorkflowBindSet.SingleOrDefault(s => s.ArticleId == (decimal)articleId);
        }

        internal static ArticleWorkflowBind GetArticleWorkflow(int articleId) => MapperFacade.ArticleWorkflowBindMapper.GetBizObject(GetArticleWorkflowDal(articleId));

        internal static ArticleWorkflowBind GetArticleWorkflow(Article article)
        {
            var binding = GetArticleWorkflow(article.Id);
            if (binding != null)
            {
                binding.Article = article;
            }
            else
            {
                binding = ArticleWorkflowBind.Create(article);
            }

            return binding;
        }

        internal static List<StatusType> GetStatuses(int workflowId)
        {
            var result = new List<StatusType>();
            var workflowResults = MapperFacade.StatusTypeMapper.GetBizList(QPContext.EFContext.WorkflowRulesSet.Where(s => s.WorkflowId == workflowId).Select(s => s.StatusType).OrderBy(n => n.Weight).ToList());
            result.Add(StatusTypeRepository.GetByName("None", workflowResults[0].SiteId));
            result.AddRange(workflowResults);
            return result;
        }

        internal static StatusType GetMaxStatus(int workflowId)
        {
            return MapperFacade.StatusTypeMapper.GetBizObject(QPContext.EFContext.WorkflowRulesSet.Where(s => s.WorkflowId == workflowId).Select(s => s.StatusType).OrderByDescending(n => n.Weight).First());
        }

        internal static bool DoesWorkflowUseStatus(int workflowId, int statusTypeId)
        {
            return QPContext.EFContext.WorkflowRulesSet
                .Where(s => s.WorkflowId == workflowId)
                .Select(s => s.StatusType)
                .Any(s => s.Id == statusTypeId);
        }

        internal static int AnotherDefaultExists(int workflowId)
        {
            return QPContext.EFContext.WorkflowSet
                .Where(s => s.Id != workflowId && s.IsDefault)
                .Select(n => (int)n.Id)
                .SingleOrDefault();
        }

        internal static StatusType GetClosestStatus(int workflowId, int statusWeight)
        {
            return
                MapperFacade.StatusTypeMapper.GetBizObject(
                    QPContext.EFContext.WorkflowRulesSet
                        .Where(s => s.WorkflowId == workflowId)
                        .Select(s => s.StatusType)
                        .FirstOrDefault(s => s.Weight >= statusWeight) ?? QPContext.EFContext.WorkflowRulesSet
                        .Where(s => s.WorkflowId == workflowId)
                        .Select(s => s.StatusType).OrderByDescending(x => x.Weight).First()
                );
        }

        internal static int GetCurrentUserMaxWeight(int workflowId)
        {
            using (new QPConnectionScope())
            {
                return Common.GetMaxUserWeight(QPConnectionScope.Current.DbConnection, QPContext.CurrentUserId, workflowId);
            }
        }

        internal static IEnumerable<Workflow> GetSiteWorkflows(int siteId)
        {
            return MapperFacade.WorkflowMapper.GetBizList(QPContext.EFContext.WorkflowSet.Where(s => s.SiteId == siteId).OrderBy(n => n.Id).ToList());
        }

        internal static IEnumerable<WorkflowListItem> GetSiteWorkflowsPage(ListCommand cmd, int siteId, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                var rows = Common.GetWorkflowsPage(scope.DbConnection, siteId, null, out totalRecords, cmd.StartRecord, cmd.PageSize);
                return MapperFacade.WorkflowListItemRowMapper.GetBizList(rows.ToList());
            }
        }

        internal static IEnumerable<Workflow> GetUserWorkflows(int userId)
        {
            var efc = QPContext.EFContext;
            var q = (from w in efc.WorkflowSet
                join r in efc.WorkflowRulesSet on w.Id equals r.Workflow.Id
                where r.User.Id == userId
                select w).Distinct();

            return MapperFacade.WorkflowMapper.GetBizList(q.ToList());
        }

        internal static IEnumerable<Workflow> GetUserGroupWorkflows(int groupId)
        {
            var efc = QPContext.EFContext;
            var q = (from w in efc.WorkflowSet
                join r in efc.WorkflowRulesSet on w.Id equals r.Workflow.Id
                where r.UserGroup.Id == groupId
                select w).Distinct();

            return MapperFacade.WorkflowMapper.GetBizList(q.ToList());
        }

        internal static IEnumerable<Workflow> GetList(IEnumerable<int> ids)
        {
            IEnumerable<decimal> decIDs = Converter.ToDecimalCollection(ids).Distinct().ToArray();
            return MapperFacade.WorkflowMapper.GetBizList(QPContext.EFContext.WorkflowSet.Where(f => decIDs.Contains(f.Id)).ToList());
        }

        internal static void UpdateContentWorkflowBind(ContentWorkflowBind binding)
        {
            var context = QPContext.EFContext;
            var oldDal = GetContentWorkflowDal(binding.Content.Id, context);
            var persisted = oldDal != null;
            var needToPersist = binding.WorkflowId != WorkflowBind.UnassignedId;
            var changed = persisted && needToPersist && (oldDal.WorkflowId != binding.WorkflowId || oldDal.IsAsync != binding.IsAsync);
            var newDal = !needToPersist ? null : MapperFacade.ContentWorkflowBindMapper.GetDalObject(binding);
            if (persisted && changed || persisted && !needToPersist)
            {
                DefaultRepository.SimpleDelete(oldDal, context);
            }

            if (persisted && changed || !persisted && needToPersist)
            {
                DefaultRepository.SimpleSave(newDal);
            }
        }

        internal static Workflow GetById(int id)
        {
            var entities = QPContext.EFContext;
            return MapperFacade.WorkflowMapper.GetBizObject(entities.WorkflowSet.Include("LastModifiedByUser").Include("WorkflowRules.StatusType").SingleOrDefault(n => (int)n.Id == id));
        }

        internal static Workflow UpdateProperties(Workflow workflow, IEnumerable<int> activeStatuses)
        {
            var entities = QPContext.EFContext;
            UpdateRuleOrder(workflow, activeStatuses);

            var dalDb = entities.WorkflowSet.AsNoTracking().Include("WorkflowRules").Single(g => g.Id == workflow.Id);
            var indbRulesIDs = new HashSet<decimal>(dalDb.WorkflowRules.Select(x => x.Id));
            var indbRulesStatusIDs = new HashSet<decimal>(dalDb.WorkflowRules.Select(x => x.SuccessorStatusId));

            var rulesToAdd = workflow.WorkflowRules.Where(x => x.Id == 0 && !indbRulesStatusIDs.Contains(x.SuccessorStatusId ?? 0)).ToArray();
            workflow.WorkflowRules.RemoveAll(x => x.Id == 0);
            var inmemoryRulesIDs = new HashSet<decimal>(workflow.WorkflowRules
                .Where(n => activeStatuses.Contains(n.SuccessorStatusId ?? 0))
                .Select(x => Converter.ToDecimal(x.Id)));

            workflow.LastModifiedBy = QPContext.CurrentUserId;
            using (new QPConnectionScope())
            {
                workflow.Modified = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
            }

            var dal = MapperFacade.WorkflowMapper.GetDalObject(workflow);
            entities.Entry(dal).State = EntityState.Modified;

            var rulesToDelete = dalDb.WorkflowRules.Where(x => x.Id != 0 && !inmemoryRulesIDs.Contains(x.Id)).ToArray();
            var rulesToUpdate = workflow.WorkflowRules
                .Where(n => inmemoryRulesIDs.Contains(n.Id) && indbRulesIDs.Contains(n.Id))
                .ToArray();

            foreach (var rule in rulesToAdd)
            {
                rule.WorkflowId = workflow.Id;
                var dalRule = MapperFacade.WorkFlowRuleMapper.GetDalObject(rule);
                entities.Entry(dalRule).State = EntityState.Added;
            }

            foreach (var rule in rulesToDelete)
            {
                entities.Entry(rule).State = EntityState.Deleted;
            }

            foreach (var rule in rulesToUpdate)
            {
                rule.WorkflowId = workflow.Id;
                var dalRule = MapperFacade.WorkFlowRuleMapper.GetDalObject(rule);
                entities.Entry(dalRule).State = EntityState.Modified;
            }

            entities.SaveChanges();
            return MapperFacade.WorkflowMapper.GetBizObject(dal);
        }

        private static void UpdateRuleOrder(Workflow workflow, IEnumerable<int> activeStatuses = null)
        {
            var i = 1;
            var rules = workflow.WorkflowRules
                .Where(n => activeStatuses == null || activeStatuses.Contains(n.SuccessorStatusId ?? 0))
                .OrderBy(n => n.StatusType.Weight);
            foreach (var rule in rules)
            {
                rule.RuleOrder = i;
                i++;
            }
        }

        internal static IEnumerable<int> GetBindedContetnsIds(int workflowId)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetWorkflowContentBindedIds(scope.DbConnection, workflowId).Select(x => Converter.ToInt32(x.Field<decimal>("CONTENT_ID")));
            }
        }

        internal static void CleanWorkflowContentIds(int workflowId)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.CleanWorkflowContentBindedIds(scope.DbConnection, workflowId);
            }
        }

        internal static void SetWorkflowBindedContentId(int workflowId, int contentId)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.SetWorkflowBindedContentId(scope.DbConnection, workflowId, contentId);
            }
        }

        private static void ChangeInsertAccessTriggerState(bool enable)
        {
            Common.ChangeTriggerState(QPContext.CurrentConnectionScope.DbConnection, "ti_access_workflow", enable);
        }

        private static void ChangeDeleteBindingsTriggerState(bool enable)
        {
            Common.ChangeTriggerState(QPContext.CurrentConnectionScope.DbConnection, "td_content_and_article_workflow_bind", enable);
        }

        internal static Workflow SaveProperties(Workflow workflow)
        {
            using (new QPConnectionScope())
            {
                var entities = QPContext.EFContext;
                UpdateRuleOrder(workflow);

                var forceIds = workflow.ForceRulesIds == null ? null : new Queue<int>(workflow.ForceRulesIds);
                workflow.LastModifiedBy = QPContext.CurrentUserId;
                workflow.Created = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
                workflow.Modified = workflow.Created;
                if (workflow.ForceId > 0)
                {
                    workflow.Id = workflow.ForceId;
                }


                var dal = MapperFacade.WorkflowMapper.GetDalObject(workflow);
                entities.Entry(dal).State = EntityState.Added;

                if (QPContext.DatabaseType == DatabaseType.SqlServer)
                {
                    DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.Workflow, workflow);
                    ChangeInsertAccessTriggerState(false);
                }

                entities.SaveChanges();
                workflow.Id = (int)dal.Id;

                CommonSecurity.CreateWorkflowAccess(QPConnectionScope.Current.DbConnection, workflow.Id);

                if (QPContext.DatabaseType == DatabaseType.SqlServer)
                {
                    DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.Workflow);
                    ChangeInsertAccessTriggerState(true);
                }

                foreach (var rule in workflow.WorkflowRules)
                {
                    rule.WorkflowId = workflow.Id;
                    if (forceIds != null)
                    {
                        rule.Id = forceIds.Dequeue();
                    }

                    var dalRule = MapperFacade.WorkFlowRuleMapper.GetDalObject(rule);
                    entities.Entry(dalRule).State = EntityState.Added;
                }

                DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.WorkflowRule);
                entities.SaveChanges();
                DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.WorkflowRule);

                return MapperFacade.WorkflowMapper.GetBizObject(dal);
            }
        }

        internal static void Delete(int id)
        {
            using (var scope = new QPConnectionScope())
            {
                if (QPContext.DatabaseType == DatabaseType.SqlServer)
                {
                    ChangeDeleteBindingsTriggerState(false);
                }

                var bindings = QPContext.EFContext.ContentWorkflowBindSet.Where(n => n.WorkflowId == id).ToArray();
                DefaultRepository.SimpleDeleteBulk(bindings);

                var bindings2 = QPContext.EFContext.ArticleWorkflowBindSet.Where(n => n.WorkflowId == id).ToArray();
                DefaultRepository.SimpleDeleteBulk(bindings2);

                var waits = QPContext.EFContext.WaitingForApprovalSet.Include(n => n.Article.WorkflowBinding)
                    .Where(n => n.Article.WorkflowBinding.WorkflowId == id).ToArray();
                DefaultRepository.SimpleDeleteBulk(waits);

                DefaultRepository.Delete<WorkflowDAL>(id);

                if (QPContext.DatabaseType == DatabaseType.SqlServer)
                {
                    ChangeDeleteBindingsTriggerState(true);
                }
            }
        }

        internal static void SaveHistoryStatus(int id, int systemStatusTypeId, string comment, int currentUserId)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.AddHistoryStatus(scope.DbConnection, id, systemStatusTypeId, currentUserId, comment);
            }
        }

        internal static void CopyArticleWorkflowBind(int sourceSiteId, int destinationSiteId, string relationsBetweenItemsXml)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.CopyArticleWorkflowBind(scope.DbConnection, sourceSiteId, destinationSiteId, relationsBetweenItemsXml);
            }
        }
    }
}
