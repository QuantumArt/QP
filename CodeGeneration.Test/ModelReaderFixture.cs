using System;
using NUnit.Framework;
using Quantumart.QP8.CodeGeneration.Services;
using System.IO;

namespace CodeGeneration.Test
{
    [TestFixture]
    public class ModelReaderFixture
    {
        [Test]
        public void ModelReader_Read()
        {
            var model = GetModel();

            Assert.That(model, Is.Not.Null, "model");

            Assert.That(model.Schema, Is.Not.Null, "Schema");
            Assert.That(model.Schema.ClassName, Is.EqualTo("EF6Model"), "Schema.ClassName");

            Assert.That(model.Contents, Is.Not.Null.And.Not.Empty, "Contents");
            Assert.That(model.Attributes, Is.Not.Null.And.Not.Empty, "Attributes");
            Assert.That(model.Links, Is.Not.Null.And.Not.Empty, "Links");
        }

        private ModelReader GetModel()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\ModelMapping.xml");
            return new ModelReader(path, _ => { }, true);
        }
    }
}
