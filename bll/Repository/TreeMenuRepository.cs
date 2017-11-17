using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Repository
{
    internal class TreeMenuRepository
    {
        /// <summary>
        /// Возвращает узел дерева
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <param name="parentEntityId">идентификатор родительской сущности</param>
        /// <param name="isFolder">признак является ли узел директорией</param>
        /// <param name="loadChildNodes">признак, разрешающий предварительную загрузку первого уровня дочерних узлов</param>
        /// <returns>узел дерева</returns>
        internal static TreeNode GetNode(string entityTypeCode, int entityId, int? parentEntityId, bool isFolder, bool isGroup = false, string groupItemCode = null, bool loadChildNodes = false)
        {
            var node = GetChildNodeList(entityTypeCode, parentEntityId, isFolder, isGroup, groupItemCode, entityId).Single();
            if (node != null)
            {
                if (entityTypeCode == EntityTypeCode.SiteFolder || entityTypeCode == EntityTypeCode.ContentFolder && string.IsNullOrEmpty(node.Title))
                {
                    node.Title = LibraryStrings.RootFolder;
                }

                if (loadChildNodes)
                {
                    var currentParentEntityId = node.IsFolder ? node.ParentId : node.Id;

                    node.ChildNodes = GetChildNodeList(node.Code, currentParentEntityId, node.IsFolder, node.IsGroup, node.GroupItemCode);
                }
            }

            return node;
        }

        /// <summary>
        /// Возвращает список дочерних узлов для указанного узла
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="parentEntityId">идентификатор родительской сущности</param>
        /// <param name="isFolder">признак является ли узел директорией</param>
        /// <returns>список дочерних узлов</returns>
        internal static IEnumerable<TreeNode> GetChildNodeList(string entityTypeCode, int? parentEntityId, bool isFolder, bool isGroup, string groupItemCode, int entityId = 0)
        {
            var nodesList = Enumerable.Empty<TreeNode>();
            var userId = QPContext.CurrentUserId;

            using (var scope = new QPConnectionScope())
            {
                nodesList = MapperFacade.TreeNodeMapper.GetBizList(
                    Common.GetChildTreeNodeList(scope.DbConnection, userId, entityTypeCode, parentEntityId, isFolder, isGroup, groupItemCode, entityId).ToList());
            }

            if (entityTypeCode == EntityTypeCode.SiteFolder || entityTypeCode == EntityTypeCode.ContentFolder)
            {
                var rootFolderNode = nodesList.Where(n => string.IsNullOrEmpty(n.Title)).SingleOrDefault();
                if (rootFolderNode != null)
                {
                    rootFolderNode.Title = LibraryStrings.RootFolder;
                }
            }

            // Не показываем не администратору контенты для которых запрещены операции редактирования
            if (entityTypeCode == EntityTypeCode.Content || entityTypeCode == EntityTypeCode.VirtualContent)
            {
                if (!QPContext.IsAdmin)
                {
                    var chdIDs = ContentRepository.GetChangeDisabledIDs();
                    if (chdIDs.Any())
                    {
                        nodesList = nodesList.Where(c => !chdIDs.Contains(c.Id)).ToArray();
                    }
                }

                if (nodesList.Any())
                {
                    var firstNode = nodesList.First();
                    if (firstNode.Code == EntityTypeCode.ContentGroup)
                    {
                        var siteId = firstNode.ParentId.Value;
                        var defaultGroupId = ContentRepository.GetDefaultGroupId(siteId);
                        var defaultNode = nodesList.Where(n => n.Id == defaultGroupId).SingleOrDefault();
                        if (defaultNode != null)
                        {
                            defaultNode.Title = Translator.Translate(defaultNode.Title);
                        }

                        nodesList = nodesList.Where(n => n.HasChildren).OrderBy(n => n.Title).ToList();
                    }
                }
            }

            if (entityTypeCode == null)
            {
                var rootNode = nodesList.Where(n => n.Code == EntityTypeCode.CustomerCode).SingleOrDefault();
                if (rootNode != null)
                {
                    rootNode.Title = QPContext.CurrentCustomerCode;
                }
            }

            return nodesList;
        }
    }
}
