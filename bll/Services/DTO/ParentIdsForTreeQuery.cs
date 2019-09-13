using Microsoft.AspNetCore.Mvc;

namespace Quantumart.QP8.BLL.Services.DTO
{
    public class ParentIdsForTreeQuery
    {
        [BindProperty(Name = "entityTypeCode")]
        public string EntityTypeCode { get; set; }

        [BindProperty(Name = "ids")]
        public int[] Ids { get; set; }
    }
}
