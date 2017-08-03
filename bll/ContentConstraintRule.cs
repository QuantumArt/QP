using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
    public class ContentConstraintRule : BizObject
    {
        private readonly InitPropertyValue<Field> _field;

        public ContentConstraintRule()
        {
            _field = new InitPropertyValue<Field>(() => FieldRepository.GetById(FieldId));
        }

        public int ConstraintId { get; set; }

        public int FieldId { get; set; }

        public Field Field => _field.Value;
    }
}
