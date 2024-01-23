namespace Quantumart.QP8.BLL.Services.DTO
{
    public class CustomActionPrepareResult
    {
        public CustomAction CustomAction { get; set; }

        public bool IsActionAccessible { get; set; }

        public string SecurityErrorMessage { get; set; }

        public string ClientSecurityErrorMessage { get; set; }
    }
}
