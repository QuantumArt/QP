using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quantumart.QP8.BLL.Processors.TreeProcessors
{
    internal class UserGroupProcessor : ITreeProcessor
    {
        private readonly string _entityTypeCode;
        private readonly int? _entityId;

        public UserGroupProcessor(string entityTypeCode, int? entityId)
        {
            _entityTypeCode = entityTypeCode;
            _entityId = entityId;
        }

        public IList<EntityTreeItem> Process()
        {
            Func<IEnumerable<TreeNode>, IEnumerable<EntityTreeItem>> map = null;
            map = input =>
            {
                if (input != null)
                {
                    return input.Select(n => new EntityTreeItem
                    {
                        Id = n.Id.ToString(),
                        Alias = n.Title,
                        IconUrl = n.Icon,
                        Enabled = true,
                        HasChildren = n.HasChildren,
                        Children = n.HasChildren ? map(n.ChildNodes) : Enumerable.Empty<EntityTreeItem>()
                    }).ToArray();
                }

                return Enumerable.Empty<EntityTreeItem>();
            };

            return map(TreeMenuRepository.GetChildNodeList(_entityTypeCode, _entityId, !_entityId.HasValue, false, null)).ToList();
        }
    }
}
