using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8;
using Quantumart.QP8.DAL;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL
{
    public class ContextMenu
    {
		public ContextMenu()
		{
			entityType = new Lazy<EntityType>(() => ContextMenuRepository.GetEntityType(Id));
		}
		
		public int Id
        {
            get;
            set;
        }
        
        public string Code
        {
            get;
            set;
        }


        
        public IEnumerable<ContextMenuItem> Items
        {
            get;
            set;
        }

		private Lazy<EntityType> entityType;
		public EntityType EntityType { get { return entityType.Value; } }
		
    }
}
