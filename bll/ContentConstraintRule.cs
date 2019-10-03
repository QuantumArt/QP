using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Newtonsoft.Json;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
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

        [JsonIgnore]
        [BindNever]
        [ValidateNever]
        public Field Field => _field.Value;
    }
}
