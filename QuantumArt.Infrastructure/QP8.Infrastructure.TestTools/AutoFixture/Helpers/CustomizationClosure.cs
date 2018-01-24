using System;
using AutoFixture;
using AutoFixture.Kernel;

namespace QP8.Infrastructure.TestTools.AutoFixture.Helpers
{
    public class CustomizationClosure : IDisposable
    {
        private readonly IFixture _fixture;
        private readonly ISpecimenBuilder _specimenBuilder;

        public CustomizationClosure(IFixture fixture, ISpecimenBuilder specimenBuilder)
        {
            _fixture = fixture;
            _specimenBuilder = specimenBuilder;
            _fixture.Customizations.Add(_specimenBuilder);
        }

        public void Dispose()
        {
            _fixture.Customizations.Remove(_specimenBuilder);
        }
    }
}
