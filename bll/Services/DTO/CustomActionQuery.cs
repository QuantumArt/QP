using Microsoft.AspNetCore.Mvc;

namespace Quantumart.QP8.BLL.Services.DTO
{
    public class CustomActionQuery
    {
        [BindProperty(Name="IDs")]
        public int[] Ids { get; set; }

        [BindProperty(Name="actionCode")]
        public string ActionCode { get; set; }
    }
}
