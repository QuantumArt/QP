using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;
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
        /// <param name="isGroup"></param>
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
            var nodesList = GetNodesList(entityTypeCode, parentEntityId, isFolder, isGroup, groupItemCode, entityId);

            //pluralize
            foreach (var groupNode in nodesList.Where(x => x.IsFolder))
            {
                groupNode.Title = Translator.Translate(Pluralize(groupNode.Title));

            }

            if (entityTypeCode == EntityTypeCode.SiteFolder || entityTypeCode == EntityTypeCode.ContentFolder)
            {
                var rootFolderNode = nodesList.SingleOrDefault(n => string.IsNullOrEmpty(n.Title));
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
                        nodesList = nodesList.Where(c => !chdIDs.Contains(c.Id)).ToList();
                    }
                }

                if (nodesList.Any())
                {
                    var firstNode = nodesList.First();
                    if (firstNode.Code == EntityTypeCode.ContentGroup)
                    {
                        var siteId = firstNode.ParentId ?? 0;
                        var defaultGroupId = ContentRepository.GetDefaultGroupId(siteId);
                        var defaultNode = nodesList.SingleOrDefault(n => n.Id == defaultGroupId);
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
                var rootNode = nodesList.SingleOrDefault(n => n.Code == EntityTypeCode.CustomerCode);
                if (rootNode != null)
                {
                    rootNode.Title = QPContext.CurrentCustomerCode;
                }
            }

            return nodesList;
        }

        private static string Pluralize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            return input.EndsWith("s") || input.EndsWith("x") ? $"{input}es" : $"{input}s";
        }

        public static bool IsEntityTypeParent(string code)
        {
            return EntityTypeCache.IsParentTypeForTree(
                QPContext.EFContext, QPContext.CurrentCustomerCode, QPContext.CurrentLanguageId, code
            );
        }

        private static List<TreeNode> GetNodesList(string entityTypeCode, int? parentEntityId, bool isFolder, bool isGroup, string groupItemCode, int entityId)
        {
            using (var scope = new QPConnectionScope())
            {
                var ctx = QPContext.EFContext;
                var connection = scope.DbConnection;
                var user = ctx.UserSet.SingleOrDefault(x => x.Id == QPContext.CurrentUserId);
                var enableContentGrouping = (entityTypeCode != EntityTypeCode.Content && entityTypeCode != EntityTypeCode.VirtualContent)
                || user.EnableContentGroupingInTree;

                var areChildNodesParents = AreChildNodesParents(entityTypeCode, isFolder, isGroup, groupItemCode, enableContentGrouping);

                var dataRows = TreeMenu.GetTreeChildNodes(
                        ctx,
                        connection,
                        entityTypeCode,
                        parentEntityId,
                        isFolder,
                        isGroup,
                        groupItemCode,
                        entityId,
                        QPContext.CurrentUserId,
                        QPContext.IsAdmin,
                        QPContext.CurrentCustomerCode,
                        enableContentGrouping
                        )
                    .ToList();

                var nodesList = MapperFacade.TreeNodeMapper.GetBizList(dataRows);
                foreach (var node in nodesList)
                {
                    node.HasChildren = GetNodeHasChildren(node, ctx, connection, enableContentGrouping, areChildNodesParents);
                }

                return nodesList;
            }
        }

        private static bool AreChildNodesParents(string entityTypeCode, bool isFolder, bool isGroup, string groupItemCode, bool enableContentGrouping)
        {
            var result = false;
            result = isFolder && IsEntityTypeParent(entityTypeCode)
                || isGroup && IsEntityTypeParent(groupItemCode);

            if ((isFolder && entityTypeCode == EntityTypeCode.Content  || entityTypeCode == EntityTypeCode.VirtualContent) && enableContentGrouping)
            {
                result = false;
            }

            return result;
        }

        private static bool GetNodeHasChildren(TreeNode node, QPModelDataContext ctx, DbConnection connection, bool enableContentGrouping, bool areChildNodesParents)
        {
            bool result = false;
            if (areChildNodesParents)
            {
                result = true;
            }
            else if (node.Code == EntityTypeCode.CustomerCode || node.IsFolder || node.IsRecurring || node.IsGroup)
            {
                var count = TreeMenu.GetTreeChildNodesCount(
                    ctx,
                    connection,
                    node.Code,
                    node.IsFolder ? node.ParentId : node.Id,
                    node.IsFolder,
                    node.IsGroup,
                    node.GroupItemCode,
                    0,
                    QPContext.CurrentUserId,
                    QPContext.IsAdmin,
                    QPContext.CurrentCustomerCode,
                    enableContentGrouping);
                result = count > 0;
            }
            return result;
        }
    }
}
