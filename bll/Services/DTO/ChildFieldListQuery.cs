using Microsoft.AspNetCore.Mvc;

namespace Quantumart.QP8.BLL.Services.DTO
{
    public class ChildFieldListQuery
    {
        [BindProperty(Name="virtualContentId")]
        public int VirtualContentId { get; set; }

        [BindProperty(Name="joinedContentId")]
        public int? JoinedContentId { get; set; }

        [BindProperty(Name="entityId")]
        public string EntityId { get; set; }

        [BindProperty(Name="selectItemIDs")]
        public string Ids { get; set; }

        [BindProperty(Name="parentAlias")]
        public string ParentAlias { get; set; }
    }
}
