using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Services.DTO
{
    public class FieldInitListResult : InitListResult
    {
        public FieldInitListResult()
        {
            IsVirtual = false;
        }

        public List<Field> Data { get; set; }

        public string ParentName { get; set; }

        public bool IsVirtual { get; set; }
    }
}
