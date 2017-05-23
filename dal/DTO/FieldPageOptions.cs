using Quantumart.QP8.Constants;

namespace Quantumart.QP8.DAL.DTO
{
    public class FieldPageOptions : PageOptionsBase
    {
        public int ContentId { get; set; }

        public FieldSelectMode Mode { get; set; }
    }
}
