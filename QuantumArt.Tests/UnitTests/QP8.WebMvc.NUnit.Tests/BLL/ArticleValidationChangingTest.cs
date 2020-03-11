using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xaml;
using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Portable.Xaml;
using QA.Validation.Xaml;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;

namespace QP8.WebMvc.NUnit.Tests.BLL
{
    [TestFixture]
    public class ArticleValidationChangingTest
    {
        public static string ConnectionString => $"Initial Catalog=qp8_test_{Environment.MachineName.ToLowerInvariant()};Data Source=mscsql01;Integrated Security=True;Application Name=UnitTest";
        private readonly IFixture _fixture;

        public ArticleValidationChangingTest()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization(){ ConfigureMembers = true});
        }

        public static Field GenerateField(IFixture fixture, FieldExactTypes type)
        {
            return fixture.Build<Field>()
                .OmitAutoProperties()
                .Do(c => c.Init())
                .With(f => f.Id)
                .With(f => f.Name)
                .With(f => f.IsClassifier, false)
                .With(f => f.ExactType, type)
                .Create();
        }

        [Test]
        public void Check_That_Validator_Change_String_Field_Test()
        {
            const string oldValue = "old_value";
            const string newValue = "new_value";

            var article = new Article { FieldValues = new List<FieldValue>() };
            var field = GenerateField(_fixture, FieldExactTypes.String);
            var fv = new FieldValue { Field = field, Value = oldValue };
            article.FieldValues.Add(fv);

            var valuesState = article.FieldValues.ToDictionary(v => v.Field.FormName, v => v.Value);
            var newModel = CreateValidatorAndRun(field.FormName, newValue, valuesState);
            var currentState = article.FieldValues.ToDictionary(v => v.Field.FormName, v => v.Value);

            Assert.AreNotEqual(currentState, newModel);
            article.CheckChangesValues(currentState, newModel);

            currentState = article.FieldValues.ToDictionary(v => v.Field.FormName, v => v.Value);
            CollectionAssert.AreEqual(currentState, newModel);
        }

        [Test]
        public void Check_That_Validator_Change_Bool_Field_Test()
        {
            var article = new Article { FieldValues = new List<FieldValue>() };
            var field = GenerateField(_fixture, FieldExactTypes.Boolean);
            var fv = new FieldValue { Field = field, Value = false.ToString() };
            article.FieldValues.Add(fv);

            var valuesState = article.FieldValues.ToDictionary(v => v.Field.FormName, v => v.Field.ExactType == FieldExactTypes.Boolean ? Converter.ToBoolean(v.Value).ToString() : v.Value);
            var newModel = CreateValidatorAndRun(field.FormName, true, valuesState);
            var currentState = article.FieldValues.ToDictionary(v => v.Field.FormName, v => v.Field.ExactType == FieldExactTypes.Boolean ? Converter.ToBoolean(v.Value).ToString() : v.Value);

            Assert.AreNotEqual(currentState, newModel);
            article.CheckChangesValues(currentState, newModel);

            currentState = article.FieldValues.ToDictionary(v => v.Field.FormName, v => v.Field.ExactType == FieldExactTypes.Boolean ? Converter.ToBoolean(v.Value).ToString() : v.Value);
            CollectionAssert.AreEqual(currentState, newModel);
        }

        [Test]
        public void Check_That_Validator_Change_Numeric_Field_Test()
        {
            const int oldValue = 10;
            const int newValue = 15;

            var article = new Article { FieldValues = new List<FieldValue>() };
            var field = GenerateField(_fixture, FieldExactTypes.Numeric);
            var fv = new FieldValue { Field = field, Value = oldValue.ToString() };
            article.FieldValues.Add(fv);

            var valuesState = article.FieldValues.ToDictionary(v => v.Field.FormName, v => v.Value);
            var newModel = CreateValidatorAndRun(field.FormName, newValue, valuesState);
            var currentState = article.FieldValues.ToDictionary(v => v.Field.FormName, v => v.Value);
            Assert.AreNotEqual(currentState, newModel);

            article.CheckChangesValues(currentState, newModel);
            currentState = article.FieldValues.ToDictionary(v => v.Field.FormName, v => v.Value);
            CollectionAssert.AreEqual(currentState, newModel);
        }

        [Test]
        public void Check_That_Validator_Change_NumericField_WithNonFormat_Value_Test()
        {
            const int oldValue = 10;
            const string newValue = "value";

            var article = new Article { FieldValues = new List<FieldValue>() };
            var field = GenerateField(_fixture, FieldExactTypes.Numeric);
            var fv = new FieldValue { Field = field, Value = oldValue.ToString() };
            article.FieldValues.Add(fv);

            var valuesState = article.FieldValues.ToDictionary(v => v.Field.FormName, v => v.Value);
            var newModel = CreateValidatorAndRun(field.FormName, newValue, valuesState);
            var currentState = article.FieldValues.ToDictionary(v => v.Field.FormName, v => v.Value);

            Assert.AreNotEqual(currentState, newModel);
            article.CheckChangesValues(currentState, newModel);
            currentState = article.FieldValues.ToDictionary(v => v.Field.FormName, v => v.Value);

            CollectionAssert.AreEqual(currentState, newModel);
        }

        [Test]
        public void Check_That_Validator_Change_DateTimeField_WithNonFormat_Value_Test()
        {
            const string newValue = "value";
            var oldValue = DateTime.Now;

            var article = new Article { FieldValues = new List<FieldValue>() };
            var field = GenerateField(_fixture, FieldExactTypes.DateTime);
            var fv = new FieldValue { Field = field, Value = oldValue.ToString(CultureInfo.InvariantCulture) };
            article.FieldValues.Add(fv);

            var valuesState = article.FieldValues.ToDictionary(v => v.Field.FormName, v => v.Value);
            var newModel = CreateValidatorAndRun(field.FormName, newValue, valuesState);
            var currentState = article.FieldValues.ToDictionary(v => v.Field.FormName, v => v.Value);
            Assert.AreNotEqual(currentState, newModel);

            article.CheckChangesValues(currentState, newModel);
            currentState = article.FieldValues.ToDictionary(v => v.Field.FormName, v => v.Value);
            CollectionAssert.AreEqual(currentState, newModel);
        }

        [Test]
        public void Check_That_Validator_Change_BoolField_WithNonFormat_Value_Test()
        {
            const string newValue = "value";
            var article = new Article { FieldValues = new List<FieldValue>() };

            var field = GenerateField(_fixture, FieldExactTypes.Boolean);
            var fv = new FieldValue { Field = field, Value = true.ToString() };
            article.FieldValues.Add(fv);

            var valuesState = article.FieldValues.ToDictionary(v => v.Field.FormName, v => v.Field.ExactType == FieldExactTypes.Boolean ? Converter.ToBoolean(v.Value).ToString() : v.Value);
            var newModel = CreateValidatorAndRun(field.FormName, newValue, valuesState);
            var currentState = article.FieldValues.ToDictionary(v => v.Field.FormName, v => v.Field.ExactType == FieldExactTypes.Boolean ? Converter.ToBoolean(v.Value).ToString() : v.Value);

            Assert.AreNotEqual(currentState, newModel);
            article.CheckChangesValues(currentState, newModel);
            currentState = article.FieldValues.ToDictionary(v => v.Field.FormName, v => v.Field.ExactType == FieldExactTypes.Boolean ? Converter.ToBoolean(v.Value).ToString() : v.Value);

            Assert.AreEqual(Converter.ToBoolean(currentState.First().Value), false);
        }

        private static string CreateSimpleValidatorText(string fieldName, object fieldValue)
        {
            var emptyValidatorString = ValidationServices.GenerateXamlValidatorText(new[]
            {
                new PropertyDefinition
                {
                    PropertyName = fieldName,
                    PropertyType = typeof(string),
                    Alias = fieldName
                }
            });

            var validator = (XamlValidator)XamlServices.Parse(emptyValidatorString);
            validator.ValidationRules.Add(new ForMember
            {
                Definition = validator.Definitions[fieldName],
                Condition = new ApplyValue { Value = fieldValue }
            });

            return XamlServices.Save(validator);
        }

        private static Dictionary<string, string> CreateValidatorAndRun(string fieldName, object valueToSet, Dictionary<string, string> model)
        {
            var obj = new ValidationParamObject
            {
                Validator = CreateSimpleValidatorText(fieldName, valueToSet),
                Model = model
            };

            var result = ValidationServices.ValidateModel(obj);
            Assert.IsTrue(result.IsValid);
            return model;
        }
    }
}
