using System;
using System.Collections.Generic;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL
{
    public class ContextMenu
    {
        public ContextMenu()
        {
            entityType = new Lazy<EntityType>(() => ContextMenuRepository.GetEntityType(Id));
        }

        public int Id { get; set; }

        public string Code { get; set; }

        public IEnumerable<ContextMenuItem> Items { get; set; }

        private readonly Lazy<EntityType> entityType;
        public EntityType EntityType => entityType.Value;
    }
}
