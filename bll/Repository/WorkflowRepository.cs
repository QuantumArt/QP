using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;
using EntityState = Microsoft.EntityFrameworkCore.EntityState;

namespace Quantumart.QP8.BLL.Repository
{
    internal class WorkflowRepository
    {
        internal static ContentWorkflowBindDAL GetContentWorkflowDal(int contentId, QP8Entities context = null)
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

        internal static Workflow UpdateProperties(Workflow workflow)
        {
            var entities = QPContext.EFContext;
            UpdateRuleOrder(workflow);

            var newWorkflowRules = workflow.WorkflowRules.Where(x => x.Id == 0).ToArray();
            workflow.WorkflowRules.RemoveAll(x => x.Id == 0);

            var dal = MapperFacade.WorkflowMapper.GetDalObject(workflow);
            dal.LastModifiedBy = QPContext.CurrentUserId;

            using (new QPConnectionScope())
            {
                dal.Modified = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
            }

            entities.Entry(dal).State = EntityState.Modified;

            var dalDb = entities.WorkflowSet.Include("WorkflowRules").Single(g => g.Id == dal.Id);
            foreach (var rule in newWorkflowRules)
            {
                var dalRule = MapperFacade.WorkFlowRuleMapper.GetDalObject(rule);
                dalRule.WorkflowId = workflow.Id;
                dalRule.Description = rule.Description;
                dalRule.RuleOrder = rule.RuleOrder;
                entities.Entry(dalRule).State = EntityState.Added;
            }

            var inmemoryRulesIDs = new HashSet<decimal>(workflow.WorkflowRules.Select(x => Converter.ToDecimal(x.Id)));
            var indbRulesIDs = new HashSet<decimal>(dalDb.WorkflowRules.Select(x => x.Id));
            foreach (var rule in dalDb.WorkflowRules.ToArray().Where(x => x.Id != 0))
            {
                if (!inmemoryRulesIDs.Contains(rule.Id))
                {
                    entities.WorkflowRulesSet.Attach(rule);
                    dalDb.WorkflowRules.Remove(rule);
                    entities.Entry(rule).State = EntityState.Deleted;
                }
            }

            foreach (var rule in workflow.WorkflowRules)
            {
                if (indbRulesIDs.Contains(rule.Id))
                {
                    var existingRule = entities.WorkflowRulesSet.Single(x => x.Id == rule.Id);
                    existingRule.UserId = rule.UserId;
                    existingRule.GroupId = rule.GroupId;
                    existingRule.Description = rule.Description;
                    existingRule.RuleOrder = rule.RuleOrder;
                }
            }

            entities.SaveChanges();
            return MapperFacade.WorkflowMapper.GetBizObject(dal);
        }

        private static void UpdateRuleOrder(Workflow workflow)
        {
            var i = 1;
            foreach (var rule in workflow.WorkflowRules.OrderBy(n => n.StatusType.Weight))
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

        internal static Workflow SaveProperties(Workflow workflow)
        {
            var entities = QPContext.EFContext;
            UpdateRuleOrder(workflow);

            var forceIds = workflow.ForceRulesIds == null ? null : new Queue<int>(workflow.ForceRulesIds);
            var dal = MapperFacade.WorkflowMapper.GetDalObject(workflow);

            dal.LastModifiedBy = QPContext.CurrentUserId;
            using (new QPConnectionScope())
            {
                dal.Created = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
                dal.Modified = dal.Created;
            }

            entities.Entry(dal).State = EntityState.Added;

            DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.Workflow, workflow);
            if (workflow.ForceId > 0)
            {
                dal.Id = workflow.ForceId;
            }

            entities.SaveChanges();
            DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.Workflow);
            foreach (var rule in workflow.WorkflowRules)
            {
                var dalRule = MapperFacade.WorkFlowRuleMapper.GetDalObject(rule);
                if (forceIds != null)
                {
                    dalRule.Id = forceIds.Dequeue();
                }
                dalRule.WorkflowId = dal.Id;
                entities.Entry(dalRule).State = EntityState.Added;
            }

            DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.WorkflowRule);
            entities.SaveChanges();
            DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.WorkflowRule);

            return MapperFacade.WorkflowMapper.GetBizObject(dal);
        }

        internal static void DeleteVeStyle(int id)
        {
            DefaultRepository.Delete<WorkflowDAL>(id);
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
