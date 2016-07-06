using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Quantumart.QP8;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Helpers;

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
			TreeNode node = GetChildNodeList(entityTypeCode, parentEntityId, isFolder, isGroup, groupItemCode, entityId).Single();
			if (node != null)
			{

				if (entityTypeCode == EntityTypeCode.SiteFolder || entityTypeCode == EntityTypeCode.ContentFolder && String.IsNullOrEmpty(node.Title))
					node.Title = LibraryStrings.RootFolder;

				if (loadChildNodes)
				{
					int? currentParentEntityId = (node.IsFolder) ? node.ParentId : node.Id;

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
			IEnumerable<TreeNode> nodesList = Enumerable.Empty<TreeNode>();
			int userId = QPContext.CurrentUserId;

			using (var scope = new QPConnectionScope())
			{
				nodesList = MappersRepository.TreeNodeMapper.GetBizList(
						Common.GetChildTreeNodeList(scope.DbConnection, userId, entityTypeCode, parentEntityId, isFolder, isGroup, groupItemCode, entityId).ToList());
			}


            if (entityTypeCode == EntityTypeCode.SiteFolder || entityTypeCode == EntityTypeCode.ContentFolder)
            {
                TreeNode rootFolderNode = nodesList.Where(n => String.IsNullOrEmpty(n.Title)).SingleOrDefault();
                if (rootFolderNode != null)
                    rootFolderNode.Title = LibraryStrings.RootFolder;
            }

			// Не показываем не администратору контенты для которых запрещены операции редактирования
			if (entityTypeCode == EntityTypeCode.Content || entityTypeCode == EntityTypeCode.VirtualContent)
			{
				if (!QPContext.IsAdmin)
				{
					IEnumerable<int> chdIDs = ContentRepository.GetChangeDisabledIDs();
					if (chdIDs.Any())
					{
						nodesList = nodesList.Where(c => !chdIDs.Contains(c.Id)).ToArray();
					}
				}

				if (nodesList.Any())
				{
					TreeNode firstNode = nodesList.First();
					if (firstNode.Code == EntityTypeCode.ContentGroup)
					{
						int siteId = firstNode.ParentId.Value;
						int defaultGroupId = ContentRepository.GetDefaultGroupId(siteId);
						TreeNode defaultNode = nodesList.Where(n => n.Id == defaultGroupId).SingleOrDefault();
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
				TreeNode rootNode = nodesList.Where(n => n.Code == Constants.EntityTypeCode.CustomerCode).SingleOrDefault();
				if (rootNode != null)
				{
					rootNode.Title = QPContext.CurrentCustomerCode;
				}
			}

			return nodesList;
		}
	}
}