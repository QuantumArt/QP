using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Repository
{
    internal class WorkflowRepository
    {

        internal static ContentWorkflowBindDAL GetContentWorkflowDAL(int contentId)
        {
            return QPContext.EFContext.ContentWorkflowBindSet.Where(s => s.ContentId == (decimal)contentId).SingleOrDefault();
        }

        internal static ContentWorkflowBind GetContentWorkflow(int contentId)
        {
            return MappersRepository.ContentWorkflowBindMapper.GetBizObject(GetContentWorkflowDAL(contentId));
        }

        internal static ContentWorkflowBind GetContentWorkflow(Content content)
        {
            ContentWorkflowBind binding = WorkflowRepository.GetContentWorkflow(content.Id);
            if (binding != null)
                binding.Content = content;
            else
                binding = new ContentWorkflowBind(content);
            return binding;
        }

        internal static ArticleWorkflowBindDAL GetArticleWorkflowDAL(int articleId)
        {
            return QPContext.EFContext.ArticleWorkflowBindSet.Where(s => s.ArticleId == (decimal)articleId).SingleOrDefault();
        }

        internal static ArticleWorkflowBind GetArticleWorkflow(int articleId)
        {
            return MappersRepository.ArticleWorkflowBindMapper.GetBizObject(GetArticleWorkflowDAL(articleId));
        }

        internal static ArticleWorkflowBind GetArticleWorkflow(Article article)
        {
            ArticleWorkflowBind binding = WorkflowRepository.GetArticleWorkflow(article.Id);
            if (binding != null)
                binding.Article = article;
            else
                binding = new ArticleWorkflowBind(article);
            return binding;
        }

        internal static List<StatusType> GetStatuses(int workflowId)
        {
            List<StatusType> result = new List<StatusType>();
            List<StatusType> workflowResults = MappersRepository.StatusTypeMapper.GetBizList(QPContext.EFContext.WorkflowRulesSet.Where(s => s.WorkflowId == (decimal)workflowId).Select(s => s.StatusType).OrderBy(n => n.Weight).ToList());
            result.Add(StatusTypeRepository.GetByName("None", workflowResults[0].SiteId));
            result.AddRange(workflowResults);
            return result;
        }

        internal static StatusType GetMaxStatus(int workflowId)
        {
            return MappersRepository.StatusTypeMapper.GetBizObject(QPContext.EFContext.WorkflowRulesSet.Where(s => s.WorkflowId == (decimal)workflowId).Select(s => s.StatusType).OrderByDescending(n => n.Weight).First());
        }

        internal static bool DoesWorkflowUseStatus(int workflowId, int statusTypeId)
        {
            return QPContext.EFContext.WorkflowRulesSet
                .Where(s => s.WorkflowId == (decimal)workflowId)
                .Select(s => s.StatusType)
                .Where(s => s.Id == (decimal)statusTypeId)
                .Any();
        }

        internal static StatusType GetClosestStatus(int workflowId, int statusWeight)
        {
            return
                MappersRepository.StatusTypeMapper.GetBizObject(
                    QPContext.EFContext.WorkflowRulesSet
                    .Where(s => s.WorkflowId == (decimal)workflowId)
                    .Select(s => s.StatusType)
                    .Where(s => s.Weight >= (decimal)statusWeight)
                    .FirstOrDefault() ?? QPContext.EFContext.WorkflowRulesSet
                    .Where(s => s.WorkflowId == (decimal)workflowId)
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
            return MappersRepository.WorkflowMapper.GetBizList(QPContext.EFContext.WorkflowSet.Where(s => s.SiteId == siteId).OrderBy(n => n.Id).ToList());
        }

        internal static IEnumerable<WorkflowListItem> GetSiteWorkflowsPage(ListCommand cmd, int siteId, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                IEnumerable<DataRow> rows = Common.GetWorkflowsPage(scope.DbConnection, siteId, null, out totalRecords, cmd.StartRecord, cmd.PageSize);
                return MappersRepository.WorkflowListItemRowMapper.GetBizList(rows.ToList());
            }
        }

        internal static IEnumerable<Workflow> GetUserWorkflows(int userId)
        {
            var efc = QPContext.EFContext;
            var q = (from w in efc.WorkflowSet
                     join r in efc.WorkflowRulesSet on w.Id equals r.Workflow.Id
                     where r.User.Id == userId
                     select w).Distinct();
            return MappersRepository.WorkflowMapper.GetBizList(q.ToList());
        }

        internal static IEnumerable<Workflow> GetUserGroupWorkflows(int groupId)
        {
            var efc = QPContext.EFContext;
            var q = (from w in efc.WorkflowSet
                     join r in efc.WorkflowRulesSet on w.Id equals r.Workflow.Id
                     where r.UserGroup.Id == groupId
                     select w).Distinct();
            return MappersRepository.WorkflowMapper.GetBizList(q.ToList());
        }

        /// <summary>
        /// Возвращает список по ids
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<Workflow> GetList(IEnumerable<int> IDs)
        {
            IEnumerable<decimal> decIDs = Converter.ToDecimalCollection(IDs).Distinct().ToArray();
            return MappersRepository.WorkflowMapper
                .GetBizList(QPContext.EFContext.WorkflowSet
                    .Where(f => decIDs.Contains(f.Id))
                    .ToList()
                );
        }

        internal static void UpdateContentWorkflowBind(ContentWorkflowBind binding)
        {
            var oldDal = GetContentWorkflowDAL(binding.Content.Id);
            bool persisted = oldDal != null;
            bool needToPersist = binding.WorkflowId != WorkflowBind.UnassignedId;
            bool changed = persisted && needToPersist && (oldDal.WorkflowId != binding.WorkflowId || oldDal.IsAsync != binding.IsAsync);
            var newDal = (!needToPersist) ? null : MappersRepository.ContentWorkflowBindMapper.GetDalObject(binding);

            if (persisted && changed || persisted && !needToPersist)
            {
                DefaultRepository.SimpleDelete(oldDal.EntityKey);
            }

            if (persisted && changed || !persisted && needToPersist)
            {
                DefaultRepository.SimpleSave(newDal);
            }
        }

        internal static Workflow GetById(int id)
        {
            QP8Entities entities = QPContext.EFContext;
            return MappersRepository.WorkflowMapper.GetBizObject(entities.WorkflowSet.Include("LastModifiedByUser").Include("WorkflowRules.StatusType").SingleOrDefault(n => (int)n.Id == id));
        }

        internal static Workflow UpdateProperties(Workflow workflow)
        {
            QP8Entities entities = QPContext.EFContext;

            UpdateRuleOrder(workflow);

            //workflowRules
            var newWorkflowRules = workflow.WorkflowRules.Where(x => x.Id == 0).ToArray();
            workflow.WorkflowRules.RemoveAll(x => x.Id == 0);

            WorkflowDAL dal = MappersRepository.WorkflowMapper.GetDalObject(workflow);
            dal.LastModifiedBy = QPContext.CurrentUserId;
            using (new QPConnectionScope())
            {
                dal.Modified = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
            }

            entities.WorkflowSet.Attach(dal);
            entities.ObjectStateManager.ChangeObjectState(dal, EntityState.Modified);

            WorkflowDAL dalDb = entities.WorkflowSet
                .Include("WorkflowRules")
                .Single(g => g.Id == dal.Id);

            foreach (var rule in newWorkflowRules)
            {
                WorkflowRulesDAL dalRule = MappersRepository.WorkFlowRuleMapper.GetDalObject(rule);
                dalRule.WorkflowId = workflow.Id;
                dalRule.Description = rule.Description;
                dalRule.RuleOrder = rule.RuleOrder;
                entities.WorkflowRulesSet.AddObject(dalRule);
            }

            HashSet<decimal> inmemoryRulesIDs = new HashSet<decimal>(workflow.WorkflowRules.Select(x => Converter.ToDecimal(x.Id)));
            HashSet<decimal> indbRulesIDs = new HashSet<decimal>(dalDb.WorkflowRules.Select(x => x.Id));
            foreach (var rule in dalDb.WorkflowRules.ToArray().Where(x => x.Id != 0))
            {
                ///удаление удаленного
                if (!inmemoryRulesIDs.Contains(rule.Id))
                {
                    entities.WorkflowRulesSet.Attach(rule);
                    dalDb.WorkflowRules.Remove(rule);
                    entities.WorkflowRulesSet.DeleteObject(rule);
                }
            }

            foreach (var rule in workflow.WorkflowRules)//изменение существовавших wf
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

            return MappersRepository.WorkflowMapper.GetBizObject(dal);
        }

        private static void UpdateRuleOrder(Workflow workflow)
        {
            int i = 1;
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
            QP8Entities entities = QPContext.EFContext;

            UpdateRuleOrder(workflow);

            Queue<int> forceIds = (workflow.ForceRulesIds == null) ? null : new Queue<int>(workflow.ForceRulesIds);

            WorkflowDAL dal = MappersRepository.WorkflowMapper.GetDalObject(workflow);
            dal.LastModifiedBy = QPContext.CurrentUserId;
            using (new QPConnectionScope())
            {
                dal.Created = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
                dal.Modified = dal.Created;
            }

            entities.WorkflowSet.AddObject(dal);

            DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.Workflow, workflow);
            if (workflow.ForceId > 0)
                dal.Id = workflow.ForceId;
            entities.SaveChanges();
            DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.Workflow);

            foreach (var rule in workflow.WorkflowRules)
            {

                WorkflowRulesDAL dalRule = MappersRepository.WorkFlowRuleMapper.GetDalObject(rule);
                if (forceIds != null)
                    dalRule.Id = forceIds.Dequeue();
                dalRule.WorkflowId = dal.Id;
                entities.WorkflowRulesSet.AddObject(dalRule);
            }

            DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.WorkflowRule);
            entities.SaveChanges();
            DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.WorkflowRule);

            return MappersRepository.WorkflowMapper.GetBizObject(dal);
        }

        internal static void DeleteVeStyle(int id)
        {
            DefaultRepository.Delete<WorkflowDAL>(id);
        }

        internal static void SaveHistoryStatus(int Id, int systemStatusTypeId, string comment, int currentUserId)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.AddHistoryStatus(scope.DbConnection, Id, systemStatusTypeId, currentUserId, comment);
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
