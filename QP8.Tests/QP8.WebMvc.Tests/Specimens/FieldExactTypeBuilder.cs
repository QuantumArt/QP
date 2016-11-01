using System.Linq;
using System.Reflection;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;
using Quantumart.QP8.Constants;

namespace WebMvc.Tests.Specimens
{
    internal class FieldWithRelationTypeBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            var type = request as PropertyInfo;
            if (type == null || type.Name != "ExactType" || type.PropertyType != typeof(FieldExactTypes))
            {
                return new NoSpecimen();
            }

            var fieldRelationType = context.Create<Generator<FieldExactTypes>>().First(fet => new[]
            {
                FieldExactTypes.O2MRelation,
                FieldExactTypes.M2ORelation,
                FieldExactTypes.M2MRelation
            }.Contains(fet));

            return fieldRelationType;
        }
    }
}
