using System.Reflection;
using Ploeh.AutoFixture.Kernel;

namespace QP8.Infrastructure.Tests.Infrastructure.Specimens
{
    internal class PrtgMonitoringServiceSpecimenBuilder : ISpecimenBuilder
    {
        private const string Host = "http://mscmonitor01:5050/";
        private const string IdentificationToken = "qp.articlescheduler.mscdev02";

        public object Create(object request, ISpecimenContext context)
        {
            var pi = request as ParameterInfo;
            if (pi == null)
            {
                return new NoSpecimen();
            }

            switch (pi.Name)
            {
                case "host":
                    return Host;
                case "identificationToken":
                    return IdentificationToken;
                default:
                    return new NoSpecimen();
            }
        }
    }
}
