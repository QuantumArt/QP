using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.WebMvc.Extensions.ValidatorProviders
{
	public class EntLibValidatorProvider : ModelValidatorProvider
	{
		public override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context)
		{
            
            if (
                metadata.PropertyName == null // если вызвана валидация для модели целиком;
                && !metadata.ModelType.IsSubclassOf(typeof(EntityObject)) // для EntityObject валидация вызывается вручную
                )
            {
                Validator validator = ValidationFactory.CreateValidator(metadata.ModelType);
			    if (validator != null)
			    {
				    yield return new EntLibValidatorWrapper(metadata, context, validator);
			    }
            }
		}
	}
}