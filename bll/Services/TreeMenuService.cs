using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Services
{
	public class TreeMenuService
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
		public static TreeNode GetNode(string entityTypeCode, int entityId, int? parentEntityId, bool isFolder, bool isGroup, string groupItemCode, bool loadChildNodes = false)
		{
			return TreeMenuRepository.GetNode(entityTypeCode, entityId, parentEntityId, isFolder, isGroup, groupItemCode, loadChildNodes);
		}

		/// <summary>
		/// Возвращает список дочерних узлов для указанного узла
		/// </summary>
		/// <param name="entityTypeCode">код типа сущности</param>
		/// <param name="parentEntityId">идентификатор родительской сущности</param>
		/// <param name="isFolder">признак является ли узел директорией</param>
		/// <returns>список дочерних узлов</returns>
		public static IEnumerable<TreeNode> GetChildNodeList(string entityTypeCode, int? parentEntityId, bool isFolder, bool isGroup, string groupItemCode)
		{
			return TreeMenuRepository.GetChildNodeList(entityTypeCode, parentEntityId, isFolder, isGroup, groupItemCode);
		}

        /// <summary>
        /// Возвращает поддерево меню от корня до ближайшего существующего нода для параметров
        /// </summary>
        /// <param name="entityTypeCode"></param>
        /// <param name="entityId"></param>
        /// <param name="parentEntityId"></param>
        /// <returns></returns>
        public static TreeNode GetSubTreeToEntity(string entityTypeCode, long entityId, long parentEntityId)
        {
            // положить в стек все ВОЗМОЖНЫЕ (Folder и Not Folder) tree menu nodes от корня до Entity
            Stack<TreeNode> stack = new Stack<TreeNode>();
            EntityObjectRepository.GetParentsChain(entityTypeCode, entityId, parentEntityId).ToList()
                .ForEach(e => 
                {

                    stack.Push(new TreeNode
                    {
                        Code = e.Code,
                        Id = Converter.ToInt32(e.Id),
                        ParentId = Converter.ToInt32(e.ParentId),
                        IsFolder = false,
                    });

                    if (StringComparer.InvariantCultureIgnoreCase.Equals(e.Code, Constants.EntityTypeCode.Content))
                    {
                        ContentGroup group = ContentRepository.GetContentGroup(Convert.ToInt32(e.Id));
                        if (group != null)
                        {
                            stack.Push(new TreeNode
                            {
                                Code = group.EntityTypeCode,
                                Id = Converter.ToInt32(group.Id),
                                ParentId = Converter.ToInt32(group.ParentEntityId),
                                GroupItemCode = Constants.GroupItemCodes.ContentGroup,
                                IsGroup = true,
                            });
                        }
                    }

                    stack.Push(new TreeNode 
                    {
                        Code = e.Code,
                        Id = Converter.ToInt32(e.ParentId),
                        IsFolder = true
                    });
                });

            // получить реальные nodes 
            Queue<TreeNode> nodeQueue = new Queue<TreeNode>(stack.Count);
            IEnumerable<TreeNode> nodes = TreeMenuRepository.GetChildNodeList(null, null, false, false, null);
            while (stack.Count > 0)
            {
                TreeNode paramNode = stack.Pop();
                TreeNode currentNode = null;
                if (paramNode.IsFolder)
                    currentNode = nodes.SingleOrDefault(n => n.IsFolder && n.Code == paramNode.Code && n.ParentId == paramNode.Id);
                else if (paramNode.IsGroup)
                    currentNode = nodes.SingleOrDefault(n => n.IsGroup && n.Code == paramNode.Code && n.Id == paramNode.Id);
                else
                    currentNode = nodes.SingleOrDefault(n => !(n.IsFolder || n.IsGroup) && n.Code == paramNode.Code && n.Id == paramNode.Id);
                if(currentNode != null)
                {
                    nodes = TreeMenuRepository.GetChildNodeList(paramNode.Code, paramNode.Id, paramNode.IsFolder, paramNode.IsGroup, paramNode.GroupItemCode);
                    currentNode.ChildNodes = nodes;
                    currentNode.HasChildren = nodes.Any();
                    nodeQueue.Enqueue(currentNode);
                }
            }

            return nodeQueue.Any() ? nodeQueue.Dequeue() : null;
        }
	}
}
