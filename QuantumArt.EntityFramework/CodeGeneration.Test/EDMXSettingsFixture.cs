using NUnit.Framework;
using Quantumart.QP8.CodeGeneration.Services;
using System.IO;

namespace CodeGeneration.Test
{
    [TestFixture]
    public class EDMXSettingsFixture
    {
        [Test]
        public void EDMXSettings_Read_AllPropertiesFilled()
        {
            var settings = GetSettings();

            Assert.That(settings, Is.Not.Null, "settings");
            Assert.That(settings.QPContextMappingResultPath, Is.EqualTo("ModelMappingResult.xml"), "QPContextMappingResultPath");
            Assert.That(settings.GenerateModel, Is.True, "GenerateModel");
            Assert.That(settings.GenerateClasses, Is.True, "GenerateClasses");
            Assert.That(settings.GenerateExtensions, Is.True, "GenerateExtensions");
            Assert.That(settings.GenerateInterface, Is.True, "GenerateInterface");
            Assert.That(settings.GenerateMappings, Is.True, "GenerateMappings");
            Assert.That(settings.GenerateMappingInterface, Is.True, "GenerateMappingInterface");

            Assert.That(settings.UseContextNameAsConnectionString, Is.True, "UseContextNameAsConnectionString");
            Assert.That(settings.UseReversedAssociations, Is.True, "UseReversedAssociations");
            Assert.That(settings.ProxyCreationEnabled, Is.False, "ProxyCreationEnabled");
            Assert.That(settings.LazyLoadingEnabled, Is.True, "LazyLoadingEnabled");
            Assert.That(settings.PlaceContentsInSeparateFiles, Is.True, "PlaceContentsInSeparateFiles");

            Assert.That(settings.Localization, Is.Not.Null, "Localization");

            Assert.That(settings.Localization.UseSelectiveMappings, Is.True, "Localization.UseSelectiveMappings");
            //Assert.That(settings.Localization.GenerateMappingsRuntime, Is.True, "Localization.GenerateMappingsRuntime");
            Assert.That(settings.Localization.CaseSensitive, Is.True, "Localization.CaseSensitive");
            Assert.That(settings.Localization.Pattern, Is.EqualTo("{fieldName}_{cultureAlias}"), "Localization.Pattern");

            Assert.That(settings.Localization.CultureMappings, Is.Not.Null.And.Not.Empty, "Localization.CultureMappings");
    }     

        private EDMXSettings GetSettings()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\EDMXSettings.xml");
            return EDMXSettings.Parse(path);
        }
    }
}
