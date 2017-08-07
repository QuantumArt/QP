using System;
using System.Xaml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Validation.Xaml;
using System.Collections.Generic;
using System.Globalization;
using Quantumart.QP8.BLL;
using System.Linq;
using Quantumart.QP8.Constants;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Quantumart.QP8.Utils;

namespace QP8.WebMvc.NUnit.Tests.BLL
{
    [TestClass]
    public class ArticleValidationChangingTest
    {
        public static string ConnectionString => $"Initial Catalog=qp8_test_{Environment.MachineName.ToLowerInvariant()};Data Source=mscsql01;Integrated Security=True;Application Name=UnitTest";
        private readonly IFixture _fixture;
        public ArticleValidationChangingTest()
        {
            _fixture = new Fixture().Customize(new AutoConfiguredMoqCustomization());
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

        [TestMethod]
        public void Check_That_Validator_Change_String_Field_Test()
        {
            var oldValue = "old_value";
            var newValue = "new_value";

            var article = new Article {FieldValues = new List<FieldValue>()};

            var field = GenerateField(_fixture, FieldExactTypes.String);
            var fv = new FieldValue() { Field = field, Value = oldValue };
            article.FieldValues.Add(fv);

            var valuesState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Value);
            var newModel = CreateValidatorAndRun(field.FormName, newValue, valuesState);

            var currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Value);

            Assert.AreNotEqual(currentState, newModel);

            article.CheckChangesValues(currentState, newModel);
            currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Value);

            CollectionAssert.AreEqual(currentState, newModel);
        }

        [TestMethod]
        public void Check_That_Validator_Change_Bool_Field_Test()
        {
            var article = new Article {FieldValues = new List<FieldValue>()};

            var field = GenerateField(_fixture, FieldExactTypes.Boolean);
            var fv = new FieldValue() { Field = field, Value = false.ToString() };
            article.FieldValues.Add(fv);

            var valuesState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                               v => v.Field.ExactType == FieldExactTypes.Boolean
                               ? Converter.ToBoolean(v.Value).ToString()
                               : v.Value);
            var newModel = CreateValidatorAndRun(field.FormName, true, valuesState);

            var currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Field.ExactType == FieldExactTypes.Boolean
                               ? Converter.ToBoolean(v.Value).ToString()
                               : v.Value);

            Assert.AreNotEqual(currentState, newModel);

            article.CheckChangesValues(currentState, newModel);
            currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Field.ExactType == FieldExactTypes.Boolean
                               ? Converter.ToBoolean(v.Value).ToString()
                               : v.Value);

            CollectionAssert.AreEqual(currentState, newModel);
        }

        [TestMethod]
        public void Check_That_Validator_Change_Numeric_Field_Test()
        {
            var oldValue = 10;
            var newValue = 15;

            var article = new Article {FieldValues = new List<FieldValue>()};

            var field = GenerateField(_fixture, FieldExactTypes.Numeric);
            var fv = new FieldValue() { Field = field, Value = oldValue.ToString() };
            article.FieldValues.Add(fv);

            var valuesState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                               v => v.Value);

            var newModel = CreateValidatorAndRun(field.FormName, newValue, valuesState);

            var currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Value);

            Assert.AreNotEqual(currentState, newModel);

            article.CheckChangesValues(currentState, newModel);
            currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Value);

            CollectionAssert.AreEqual(currentState, newModel);
        }

        [TestMethod]
        public void Check_That_Validator_Change_NumericField_WithNonFormat_Value_Test()
        {
            var oldValue = 10;
            var newValue = "value";

            var article = new Article {FieldValues = new List<FieldValue>()};

            var field = GenerateField(_fixture, FieldExactTypes.Numeric);
            var fv = new FieldValue() { Field = field, Value = oldValue.ToString() };
            article.FieldValues.Add(fv);

            var valuesState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                               v => v.Value);

            var newModel = CreateValidatorAndRun(field.FormName, newValue, valuesState);

            var currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Value);

            Assert.AreNotEqual(currentState, newModel);

            article.CheckChangesValues(currentState, newModel);
            currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Value);

            CollectionAssert.AreEqual(currentState, newModel);
        }

        [TestMethod]
        public void Check_That_Validator_Change_DateTimeField_WithNonFormat_Value_Test()
        {
            var oldValue = DateTime.Now;
            var newValue = "value";

            var article = new Article {FieldValues = new List<FieldValue>()};

            var field = GenerateField(_fixture, FieldExactTypes.DateTime);
            var fv = new FieldValue() { Field = field, Value = oldValue.ToString(CultureInfo.InvariantCulture) };
            article.FieldValues.Add(fv);

            var valuesState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                               v => v.Value);

            var newModel = CreateValidatorAndRun(field.FormName, newValue, valuesState);

            var currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Value);

            Assert.AreNotEqual(currentState, newModel);

            article.CheckChangesValues(currentState, newModel);
            currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Value);

            CollectionAssert.AreEqual(currentState, newModel);
        }

        [TestMethod]
        public void Check_That_Validator_Change_BoolField_WithNonFormat_Value_Test()
        {
            var newValue = "value";

            var article = new Article {FieldValues = new List<FieldValue>()};

            var field = GenerateField(_fixture, FieldExactTypes.Boolean);
            var fv = new FieldValue() { Field = field, Value = true.ToString() };
            article.FieldValues.Add(fv);

            var valuesState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                               v => v.Field.ExactType == FieldExactTypes.Boolean
                               ? Converter.ToBoolean(v.Value).ToString()
                               : v.Value);

            var newModel = CreateValidatorAndRun(field.FormName, newValue, valuesState);

            var currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Field.ExactType == FieldExactTypes.Boolean
                               ? Converter.ToBoolean(v.Value).ToString()
                               : v.Value);

            Assert.AreNotEqual(currentState, newModel);

            article.CheckChangesValues(currentState, newModel);
            currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Field.ExactType == FieldExactTypes.Boolean
                               ? Converter.ToBoolean(v.Value).ToString()
                               : v.Value);

           Assert.AreEqual(Converter.ToBoolean(currentState.First().Value), false);
        }

        private static string CreateSimpleValidatorText(string fieldName, object fieldValue)
        {
            var emptyValidatorString = ValidationServices.GenerateXamlValidatorText(new[] { new PropertyDefinition
            {
                PropertyName = fieldName,
                PropertyType = typeof(string),
                Alias = fieldName }
            });

            var validator = (XamlValidator)XamlServices.Parse(emptyValidatorString);

            // добавим в валидатор логику выставления значения для этого поля
            validator.ValidationRules.Add(new ForMember
            {
                Definition = validator.Definitions[fieldName],
                Condition = new ApplyValue { Value = fieldValue }
            });

            var newValidatorText = XamlServices.Save(validator);
            return newValidatorText;
        }

        private static Dictionary<string, string> CreateValidatorAndRun(string fieldName, object valueToSet, Dictionary<string, string> model)
        {
            // создадим пустой валидатор c одним полем
            var obj = new ValidationParamObject()
            {
                Validator = CreateSimpleValidatorText(fieldName, valueToSet),
                Model = model
            };

            var result = ValidationServices.ValidateModel(obj);

            Assert.IsTrue(result.IsValid);

            // после запуска валидатора модель должна поменяться
            return model;
        }
    }
}
