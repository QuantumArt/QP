using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL.Repository.Results;
using Quantumart.QP8.Constants;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Helpers;
using System.Data;

namespace Quantumart.QP8.BLL.Services
{
    public static class VirtualContentService
    {

        #region Getting Lists
        /// <summary>
        /// Инициализация списка контентов
        /// </summary>
        public static ContentInitListResult InitUnionSourceList(int siteId)
        {
            Site site = SiteRepository.GetById(siteId);
            if (site == null)
                throw new Exception(String.Format(SiteStrings.SiteNotFound, siteId));
            return new ContentInitListResult { ParentName = site.Name };
        }

        public static ListResult<ContentListItem> List(ContentListFilter filter, ListCommand cmd)
        {
            filter.IsVirtual = true;
            return ContentRepository.GetList(filter, cmd);
        }

        /// <summary>
        /// Возвращает список контентов на основе которых можно строить виртуальные контетны типа join
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public static IEnumerable<ListItem> GetAcceptableContentForVirtualJoin(int siteId)
        {
            if (!SiteRepository.Exists(siteId))
                throw new ArgumentException(String.Format(SiteStrings.SiteNotFound, siteId));
            return VirtualContentRepository.GetAcceptableContentForVirtualJoin(siteId);
        }

        #endregion

        #region Save
        /// <summary>
        /// Возвращает контент для добавления
        /// </summary>
        /// <param name="siteId">идентификатор сайта</param>
        /// <returns>контент</returns>
        public static Content New(int siteId, int? groupId)
        {
            return ContentService.InternalNew(siteId, groupId);
        }

        public static Content NewForSave(int siteId)
        {
            return New(siteId, null);
        }

        public static Content Save(Content content)
        {
            if (content == null)
                throw new ArgumentNullException("content");
            if (content.VirtualType == VirtualType.None)
                throw new ApplicationException("Content virtual type is undefined.");

            // Сохранить контент
            var helper = new VirtualContentHelper(content.ForceVirtualFieldIds);
            Content newContent = VirtualContentRepository.Save(content);

            if (content.VirtualType == VirtualType.Join)
                newContent = helper.SaveJoinContent(content, newContent);
            else if (content.VirtualType == VirtualType.Union)
                newContent = helper.SaveUnionContent(content, newContent);
            else if (content.VirtualType == VirtualType.UserQuery)
                newContent = helper.SaveUserQueryContent(content, newContent);

            newContent.NewVirtualFieldIds = helper.NewFieldIds;
            return newContent;
        }

        #endregion

        #region Read
        /// <summary>
        /// Возвращает контент для редактирования или просмотра
        /// </summary>
        /// <param name="id">идентификатор контента</param>
        /// <returns>контент</returns>
        public static Content Read(int id)
        {
            return ContentService.InternalRead(id);
        }

        public static Content ReadForUpdate(int id)
        {
            return Read(id);
        }
        #endregion

        #region Update
        /// <summary>
        /// Обновляет виртуальный контент
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static Content Update(Content content)
        {
            if (content == null)
                throw new ArgumentNullException("content");

            var helper = new VirtualContentHelper(content.ForceVirtualFieldIds);
            using (VirtualFieldRepository.LoadVirtualFieldsRelationsToMemory(content.Id))
            {
                // Если тип контента изменился
                // то удалить связанную с контентом информацию
                if (content.StoredVirtualType != content.VirtualType)
                {
                    helper.RemoveContentData(content);
                }

                // Обновить контент
                var dbContent = VirtualContentRepository.Update(content);

                // Спициальное обновления для конкретного типа контента
                if (content.VirtualType == VirtualType.Join)
                    dbContent = helper.UpdateJoinContent(content, dbContent);
                else if (content.VirtualType == VirtualType.Union)
                    dbContent = helper.UpdateUnionContent(content, dbContent);
                else if (content.VirtualType == VirtualType.UserQuery)
                    dbContent = helper.UpdateUserQueryContent(content, dbContent);

                dbContent.NewVirtualFieldIds = helper.NewFieldIds;
                return dbContent;
            }
        }

        #endregion

        #region Remove
        public static MessageResult Remove(int id)
        {
            Content content = ContentRepository.GetById(id);
            if (content == null)
                throw new Exception(String.Format(ContentStrings.ContentNotFound, id));

            if (!content.IsAccessible(ActionTypeCode.Remove))
                return MessageResult.Error(ArticleStrings.CannotRemoveBecauseOfSecurity);

            var violationMessages = content.Die();

            if (violationMessages.Any())
            {
                return MessageResult.Error(String.Join(Environment.NewLine, violationMessages), new[] { id });
            }
            else
            {
                return null;
            }


        }
        #endregion

        #region Helpers

        #region Join Virtual Content Fied List operations
        public static IEnumerable<EntityTreeItem> GetChildFieldList(int virtualContentId, int? joinedContentId, string entityId, string selectItemIDs, string parentAlias)
        {
            var helper = new VirtualContentHelper();
            // Дочерние поля выбранного поля
            if (!String.IsNullOrWhiteSpace(entityId))
                return helper.GetChildFieldList(entityId, parentAlias, (f, eid, alias) => Enumerable.Empty<EntityTreeItem>());
            // рутовые поля
            else if (virtualContentId > 0 || joinedContentId.HasValue)
                return helper.GetRootFieldList(virtualContentId, joinedContentId, selectItemIDs);
            else
                return null;
        }


        #endregion

        #endregion

        #region Copy

        public static IEnumerable<DataRow> CopyVirtualContents(int sourceSiteId, int destinationSiteId)
        {
            IEnumerable<DataRow> newContents = ContentRepository.CopyVirtualContents(sourceSiteId, destinationSiteId);
            string newContentIds = String.Join(",", newContents.Select(r => r.Field<int>("content_id_new")));

            FieldRepository.CopyContentsAttributes(sourceSiteId, destinationSiteId, newContentIds, isContentsVirtual: true);

            string relBetweenAttributes = FieldRepository.GetRelationsBetweenAttributesXML(sourceSiteId, destinationSiteId, String.Empty, forVirtualContents: null, byNewContents: true);

            if (String.IsNullOrEmpty(newContentIds))
                newContentIds = "0";
            FieldRepository.UpdateAttributes(sourceSiteId, destinationSiteId, relBetweenAttributes, newContentIds);
            ContentRepository.CopyUnionContents(sourceSiteId, destinationSiteId, newContentIds);
            ContentRepository.UpdateVirtualContentAttributes(sourceSiteId, destinationSiteId);

            FieldRepository.UpdateAttributesOrder(destinationSiteId, relBetweenAttributes, newContentIds);

            ContentRepository.CopyContentsGroups(sourceSiteId, destinationSiteId);
            ContentRepository.UpdateContentGroupIds(sourceSiteId, destinationSiteId);

            string relBetweenContents = ContentRepository.GetRelationsBetweenContentsXML(sourceSiteId, destinationSiteId, String.Empty);
            ContentRepository.CopyUserQueryContents(relBetweenContents);

            ContentRepository.CopyUserQueryAttributes(relBetweenContents, relBetweenAttributes);

            return newContents;
        }

        public static string UpdateVirtualContents(int oldSiteId, int newSiteId, IEnumerable<DataRow> rows)
        {
            StringBuilder contentsWithErrors = new StringBuilder();
            if (rows == null)
                return String.Empty;
            foreach (var row in rows)
            {
                int virtualTypeId = Int32.Parse(row["virtual_type"].ToString());
                int newContentId = Int32.Parse(row["content_id_new"].ToString());
                VirtualContentHelper helper = new VirtualContentHelper();
                Content newContent = ContentRepository.GetById(newContentId);
                if (virtualTypeId == VirtualType.UserQuery)
                {
                    IEnumerable<DataRow> contentsRelations = ContentRepository.GetRelationsBetweenContents(oldSiteId, newSiteId, String.Empty);
                    string newSqlQuery = ReplaceOldContentIdsFromQuery(contentsRelations, row["sqlquery"].ToString());
                    ContentRepository.UpdateVirtualContent(newSqlQuery, newContentId);
                    newContent.UserQuery = newSqlQuery;
                }
                try
                {
                    helper.CreateContentViews(newContent);
                }
                catch (Exception) {
                    contentsWithErrors.Append(String.Format("{0}, ", newContent.Name));
                }
            }
            return contentsWithErrors.ToString();
        }

        private static string ReplaceOldContentIdsFromQuery(IEnumerable<DataRow> contentsRelations, string sqlQuery)
        {
            MatchCollection matchValues = Regex.Matches(sqlQuery, @"content_([\d]*)", RegexOptions.IgnoreCase);

            foreach (Match match in matchValues)
            {
                DataRow row = contentsRelations.Where(r => r["source_content_id"].ToString() == match.Groups[1].Value).FirstOrDefault();
                if(row != null){
                    string newContentId = row["destination_content_id"].ToString();
                    sqlQuery = sqlQuery.Replace(match.Groups[1].Value, newContentId);
                }
            }
            return sqlQuery;
        }

        #endregion

    }
}
