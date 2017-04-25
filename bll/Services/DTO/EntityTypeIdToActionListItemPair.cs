using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Services.DTO
{
	public class EntityTypeIdToActionListItemPair
	{
		public int EntityTypeId { get; set; }
		public IEnumerable<SimpleListItem> ActionItems { get; set; }
	}
}
