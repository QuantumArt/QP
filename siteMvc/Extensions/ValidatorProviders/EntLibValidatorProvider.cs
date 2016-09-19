using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.Extensions.ValidatorProviders
{
    public class EntLibValidatorProvider : ModelValidatorProvider
    {
        public override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context)
        {
            if (metadata.PropertyName == null && !metadata.ModelType.IsSubclassOf(typeof(EntityObject)))
            {
                // если вызвана валидация для модели целиком;
                // для EntityObject валидация вызывается вручную
                var validator = ValidationFactory.CreateValidator(metadata.ModelType);
                if (validator != null)
                {
                    yield return new EntLibValidatorWrapper(metadata, context, validator);
                }
            }
        }
    }
}
