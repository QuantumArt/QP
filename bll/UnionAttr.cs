namespace Quantumart.QP8.BLL
{
    public class UnionAttr
    {
        public int VirtualFieldId { get; set; }
        public int BaseFieldId { get; set; }

        public Field VirtualField { get; set; }
        public Field BaseField { get; set; }
    }
}
