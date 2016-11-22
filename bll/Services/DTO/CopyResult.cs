using System;

namespace Quantumart.QP8.BLL.Services.DTO
{
    [Serializable]
    public class CopyResult
    {
        public MessageResult Message { get; set; }

        public int Id { get; set; }

        public Guid UniqueId { get; set; }
    }
}
