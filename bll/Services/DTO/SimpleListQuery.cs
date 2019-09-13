using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services.DTO
{
    public class SimpleListQuery
    {
        [BindProperty(Name="entityTypeCode")]
        public string EntityTypeCode { get; set; }

        [BindProperty(Name="parentEntityId")]
        public int ParentEntityId { get; set; }

        [BindProperty(Name="entityId")]
        public int EntityId { get; set; }

        [BindProperty(Name="listId")]
        public int ListId { get; set; }

        [BindProperty(Name="selectionMode")]
        public ListSelectionMode SelectionMode { get; set; }

        [BindProperty(Name="selectedEntitiesIDs")]
        public int[] SelectedEntitiesIds { get; set; }

        [BindProperty(Name="filter")]
        public string Filter { get; set; }

        [BindProperty(Name="testEntityId")]
        public int TestEntityId { get; set; }

        public int? ActualEntityId => EntityId == 0 ? (int?)null : EntityId;

        public int? ActualListId => ListId == 0 ? (int?)null : ListId;

    }
}
