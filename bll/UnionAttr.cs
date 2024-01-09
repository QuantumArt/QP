using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Quantumart.QP8.BLL
{
    public class UnionAttr
    {
        public int VirtualFieldId { get; set; }
        public int BaseFieldId { get; set; }

        [ValidateNever]
        [BindNever]
        public Field VirtualField { get; set; }

        [ValidateNever]
        [BindNever]
        public Field BaseField { get; set; }
    }
}
