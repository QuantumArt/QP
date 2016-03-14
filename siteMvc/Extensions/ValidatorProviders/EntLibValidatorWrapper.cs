using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Practices.EnterpriseLibrary.Validation;

namespace Quantumart.QP8.WebMvc.Extensions.ValidatorProviders
{
	public class EntLibValidatorWrapper : ModelValidator 
	{
		private Validator _validator; 
		public EntLibValidatorWrapper(ModelMetadata metadata, ControllerContext context, Validator validator) : base(metadata, context) 
		{ 
			_validator = validator; 
		} 
		
		public override IEnumerable<ModelValidationResult> Validate(object container) 
		{
			return ConvertResults(_validator.Validate(Metadata.Model)); 
		} 
		
		private IEnumerable<ModelValidationResult> ConvertResults(IEnumerable<ValidationResult> validationResults) 
		{ 
			if (validationResults != null) 
			{ 
				foreach (ValidationResult validationResult in validationResults) 
				{ 
					if (validationResult.NestedValidationResults != null) 
					{ 
						foreach (ModelValidationResult result in ConvertResults(validationResult.NestedValidationResults)) 
						{ 
							yield return result; 
						} 
					}

                    yield return new ModelValidationResult { Message = validationResult.Message, MemberName = validationResult.Key }; 
				} 
			} 
		} 
	}
}
