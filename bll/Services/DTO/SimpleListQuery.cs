using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.Constants;
using System;

namespace Quantumart.QP8.BLL.Services.DTO
{
    public class SimpleListQuery
    {
        [BindProperty(Name="entityTypeCode")]
        public string EntityTypeCode { get; set; }

        [BindProperty(Name="parentEntityId")]
        public int ParentEntityId { get; set; }

        [BindProperty(Name="entityId")]
        public string EntityId { get; set; }

        [BindProperty(Name="listId")]
        public int ListId { get; set; }

        [BindProperty(Name="selectionMode")]
        public ListSelectionMode SelectionMode { get; set; }

        [BindProperty(Name="selectedEntitiesIDs")]
        public int[] SelectedEntitiesIds { get; set; }

        [BindProperty(Name="filter")]
        public CustomFilterItem[] Filter { get; set; }

        [BindProperty(Name="testEntityId")]
        public int TestEntityId { get; set; }

        public int? ActualEntityId => String.IsNullOrEmpty(EntityId) ? (int?)null : int.Parse(EntityId);

        public int? ActualListId => ListId == 0 ? (int?)null : ListId;

    }
}
