using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services
{
    public static class VirtualContentService
    {
        /// <summary>
        /// Инициализация списка контентов
        /// </summary>
        public static ContentInitListResult InitUnionSourceList(int siteId)
        {
            var site = SiteRepository.GetById(siteId);
            if (site == null)
            {
                throw new Exception(string.Format(SiteStrings.SiteNotFound, siteId));
            }

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
            {
                throw new ArgumentException(string.Format(SiteStrings.SiteNotFound, siteId));
            }

            return VirtualContentRepository.GetAcceptableContentForVirtualJoin(siteId);
        }

        /// <summary>
        /// Возвращает контент для добавления
        /// </summary>
        public static Content New(int siteId, int? groupId) => ContentService.InternalNew(siteId, groupId);

        public static Content NewForSave(int siteId) => New(siteId, null);

        public static Content Save(Content content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            if (content.VirtualType == VirtualType.None)
            {
                throw new ApplicationException("Content virtual type is undefined.");
            }

            // Сохранить контент
            var helper = new VirtualContentHelper(content.ForceVirtualFieldIds.ToList());
            var newContent = VirtualContentRepository.Save(content);

            if (content.VirtualType == VirtualType.Join)
            {
                newContent = helper.SaveJoinContent(content, newContent);
            }
            else if (content.VirtualType == VirtualType.Union)
            {
                newContent = helper.SaveUnionContent(content, newContent);
            }
            else if (content.VirtualType == VirtualType.UserQuery)
            {
                newContent = helper.SaveUserQueryContent(content, newContent);
            }

            newContent.NewVirtualFieldIds = helper.NewFieldIds;

            return newContent;
        }

        /// <summary>
        /// Возвращает контент для редактирования или просмотра
        /// </summary>
        /// <param name="id">идентификатор контента</param>
        /// <returns>контент</returns>
        public static Content Read(int id) => ContentService.InternalRead(id);

        public static Content ReadForUpdate(int id) => Read(id);

        /// <summary>
        /// Обновляет виртуальный контент
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static Content Update(Content content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var helper = new VirtualContentHelper(content.ForceVirtualFieldIds.ToList());
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
                {
                    dbContent = helper.UpdateJoinContent(content, dbContent);
                }
                else if (content.VirtualType == VirtualType.Union)
                {
                    dbContent = helper.UpdateUnionContent(content, dbContent);
                }
                else if (content.VirtualType == VirtualType.UserQuery)
                {
                    dbContent = helper.UpdateUserQueryContent(content, dbContent);
                }

                dbContent.NewVirtualFieldIds = helper.NewFieldIds;
                return dbContent;
            }
        }

        public static MessageResult Remove(int id)
        {
            var content = ContentRepository.GetById(id);
            if (content == null)
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, id));
            }

            if (!content.IsAccessible(ActionTypeCode.Remove))
            {
                return MessageResult.Error(ArticleStrings.CannotRemoveBecauseOfSecurity);
            }

            var violationMessages = content.Die().ToList();
            return violationMessages.Any() ? MessageResult.Error(string.Join(Environment.NewLine, violationMessages), new[] { id }) : null;
        }

        public static IEnumerable<EntityTreeItem> GetChildFieldList(int virtualContentId, int? joinedContentId, string entityId, string selectItemIDs, string parentAlias)
        {
            var helper = new VirtualContentHelper();

            // Дочерние поля выбранного поля
            if (!string.IsNullOrWhiteSpace(entityId))
            {
                return helper.GetChildFieldList(entityId, parentAlias, (f, eid, alias) => Enumerable.Empty<EntityTreeItem>());
            }

            // рутовые поля
            if (virtualContentId > 0 || joinedContentId.HasValue)
            {
                return helper.GetRootFieldList(virtualContentId, joinedContentId, selectItemIDs);
            }

            return null;
        }

        public static IEnumerable<DataRow> CopyVirtualContents(int sourceSiteId, int destinationSiteId)
        {
            var newContents = ContentRepository.CopyVirtualContents(sourceSiteId, destinationSiteId).ToList();
            var newContentIds = string.Join(",", newContents.Select(r => r.Field<int>("content_id_new")));
            FieldRepository.CopyContentsAttributes(sourceSiteId, destinationSiteId, newContentIds, true);

            var relBetweenAttributes = FieldRepository.GetRelationsBetweenAttributesXml(sourceSiteId, destinationSiteId, string.Empty, null, true);
            if (string.IsNullOrEmpty(newContentIds))
            {
                newContentIds = "0";
            }

            FieldRepository.UpdateAttributes(sourceSiteId, destinationSiteId, relBetweenAttributes, newContentIds);
            ContentRepository.CopyUnionContents(sourceSiteId, destinationSiteId, newContentIds);
            ContentRepository.UpdateVirtualContentAttributes(sourceSiteId, destinationSiteId);

            FieldRepository.UpdateAttributesOrder(destinationSiteId, relBetweenAttributes, newContentIds);

            ContentRepository.CopyContentsGroups(sourceSiteId, destinationSiteId);
            ContentRepository.UpdateContentGroupIds(sourceSiteId, destinationSiteId);

            var relBetweenContents = ContentRepository.GetRelationsBetweenContentsXml(sourceSiteId, destinationSiteId, string.Empty);
            ContentRepository.CopyUserQueryContents(relBetweenContents);

            ContentRepository.CopyUserQueryAttributes(relBetweenContents, relBetweenAttributes);

            return newContents;
        }

        public static string UpdateVirtualContents(int oldSiteId, int newSiteId, IEnumerable<DataRow> rows)
        {
            var contentsWithErrors = new StringBuilder();
            if (rows == null)
            {
                return string.Empty;
            }

            foreach (var row in rows)
            {
                var virtualTypeId = int.Parse(row["virtual_type"].ToString());
                var newContentId = int.Parse(row["content_id_new"].ToString());
                var helper = new VirtualContentHelper();
                var newContent = ContentRepository.GetById(newContentId);
                if (virtualTypeId == VirtualType.UserQuery)
                {
                    var contentsRelations = ContentRepository.GetRelationsBetweenContents(oldSiteId, newSiteId, string.Empty).ToList();
                    var newSqlQuery = ReplaceOldContentIdsFromQuery(contentsRelations, row["sqlquery"].ToString());
                    ContentRepository.UpdateVirtualContent(newSqlQuery, newContentId);
                    newContent.UserQuery = newSqlQuery;
                }
                try
                {
                    helper.CreateContentViews(newContent);
                }
                catch (Exception)
                {
                    contentsWithErrors.Append($"{newContent.Name}, ");
                }
            }

            return contentsWithErrors.ToString();
        }

        private static string ReplaceOldContentIdsFromQuery(IList<DataRow> contentsRelations, string sqlQuery)
        {
            var matchValues = Regex.Matches(sqlQuery, @"content_([\d]*)", RegexOptions.IgnoreCase);
            foreach (Match match in matchValues)
            {
                var row = contentsRelations.FirstOrDefault(r => r["source_content_id"].ToString() == match.Groups[1].Value);
                if (row != null)
                {
                    var newContentId = row["destination_content_id"].ToString();
                    sqlQuery = sqlQuery.Replace(match.Groups[1].Value, newContentId);
                }
            }

            return sqlQuery;
        }
    }
}
