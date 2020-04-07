using Microsoft.AspNetCore.Mvc;

namespace Quantumart.QP8.BLL.Services.DTO
{
    public class PermissionTreeQuery
    {
        [BindProperty(Name = "userId")]
        public int? UserId { get; set; }

        [BindProperty(Name = "groupId")]
        public int? GroupId { get; set; }

        [BindProperty(Name = "entityTypeId")]
        public int? EntityTypeId { get; set; }

        [BindProperty(Name = "actionId")]
        public int? ActionId { get; set; }
    }
}
