using System;
using System.Xaml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Validation.Xaml;
using System.Collections.Generic;
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
            var old_value = "old_value";
            var new_value = "new_value";

            var article = new Article();
            article.FieldValues = new List<FieldValue>();

            var field = GenerateField(_fixture, FieldExactTypes.String);
            var fv = new FieldValue() { Field = field, Value = old_value };
            article.FieldValues.Add(fv);

            var valuesState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Value);
            var new_model = CreateValidatorAndRun<string>(field.FormName, new_value, valuesState);

            var currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Value);

            Assert.AreNotEqual(currentState, new_model);

            article.CheckChangesValues(currentState, new_model);
            currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Value);

            CollectionAssert.AreEqual(currentState, new_model);
        }

        [TestMethod]
        public void Check_That_Validator_Change_Bool_Field_Test()
        {
            var old_value = false;
            var new_value = true;

            var article = new Article();
            article.FieldValues = new List<FieldValue>();

            var field = GenerateField(_fixture, FieldExactTypes.Boolean);
            var fv = new FieldValue() { Field = field, Value = old_value.ToString() };
            article.FieldValues.Add(fv);

            var valuesState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                               v => v.Field.ExactType == FieldExactTypes.Boolean
                               ? Converter.ToBoolean(v.Value).ToString()
                               : v.Value);
            var new_model = CreateValidatorAndRun<string>(field.FormName, new_value, valuesState);

            var currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Field.ExactType == FieldExactTypes.Boolean
                               ? Converter.ToBoolean(v.Value).ToString()
                               : v.Value);

            Assert.AreNotEqual(currentState, new_model);

            article.CheckChangesValues(currentState, new_model);
            currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Field.ExactType == FieldExactTypes.Boolean
                               ? Converter.ToBoolean(v.Value).ToString()
                               : v.Value);

            CollectionAssert.AreEqual(currentState, new_model);
        }

        [TestMethod]
        public void Check_That_Validator_Change_Numeric_Field_Test()
        {
            var old_value = 10;
            var new_value = 15;

            var article = new Article();
            article.FieldValues = new List<FieldValue>();

            var field = GenerateField(_fixture, FieldExactTypes.Numeric);
            var fv = new FieldValue() { Field = field, Value = old_value.ToString() };
            article.FieldValues.Add(fv);

            var valuesState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                               v => v.Value);

            var new_model = CreateValidatorAndRun<string>(field.FormName, new_value, valuesState);

            var currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Value);

            Assert.AreNotEqual(currentState, new_model);

            article.CheckChangesValues(currentState, new_model);
            currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Value);

            CollectionAssert.AreEqual(currentState, new_model);
        }

        [TestMethod]
        public void Check_That_Validator_Change_NumericField_WithNonFormat_Value_Test()
        {
            var old_value = 10;
            var new_value = "value";

            var article = new Article();
            article.FieldValues = new List<FieldValue>();

            var field = GenerateField(_fixture, FieldExactTypes.Numeric);
            var fv = new FieldValue() { Field = field, Value = old_value.ToString() };
            article.FieldValues.Add(fv);

            var valuesState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                               v => v.Value);

            var new_model = CreateValidatorAndRun<string>(field.FormName, new_value, valuesState);

            var currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Value);

            Assert.AreNotEqual(currentState, new_model);

            article.CheckChangesValues(currentState, new_model);
            currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Value);

            CollectionAssert.AreEqual(currentState, new_model);
        }

        [TestMethod]
        public void Check_That_Validator_Change_DateTimeField_WithNonFormat_Value_Test()
        {
            var old_value = DateTime.Now;
            var new_value = "value";

            var article = new Article();
            article.FieldValues = new List<FieldValue>();

            var field = GenerateField(_fixture, FieldExactTypes.DateTime);
            var fv = new FieldValue() { Field = field, Value = old_value.ToString() };
            article.FieldValues.Add(fv);

            var valuesState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                               v => v.Value);

            var new_model = CreateValidatorAndRun<string>(field.FormName, new_value, valuesState);

            var currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Value);

            Assert.AreNotEqual(currentState, new_model);

            article.CheckChangesValues(currentState, new_model);
            currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Value);

            CollectionAssert.AreEqual(currentState, new_model);
        }

        [TestMethod]
        public void Check_That_Validator_Change_BoolField_WithNonFormat_Value_Test()
        {
            var old_value = true;
            var new_value = "value";

            var article = new Article();
            article.FieldValues = new List<FieldValue>();

            var field = GenerateField(_fixture, FieldExactTypes.Boolean);
            var fv = new FieldValue() { Field = field, Value = old_value.ToString() };
            article.FieldValues.Add(fv);

            var valuesState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                               v => v.Field.ExactType == FieldExactTypes.Boolean
                               ? Converter.ToBoolean(v.Value).ToString()
                               : v.Value);

            var new_model = CreateValidatorAndRun<string>(field.FormName, new_value, valuesState);

            var currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Field.ExactType == FieldExactTypes.Boolean
                               ? Converter.ToBoolean(v.Value).ToString()
                               : v.Value);

            Assert.AreNotEqual(currentState, new_model);

            article.CheckChangesValues(currentState, new_model);
            currentState = article.FieldValues
                .ToDictionary(v => v.Field.FormName,
                              v => v.Field.ExactType == FieldExactTypes.Boolean
                               ? Converter.ToBoolean(v.Value).ToString()
                               : v.Value);

           Assert.AreEqual(Converter.ToBoolean(currentState.First().Value), false);
        }

        private static string CreateSimpleValidatorText<T>(string fieldName, object fieldValue)
        {
            var emptyValidatorString = ValidationServices.GenerateXamlValidatorText(new PropertyDefinition[] { new PropertyDefinition
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

        private static Dictionary<string, string> CreateValidatorAndRun<T>(string fieldName, object valueToSet, Dictionary<string, string> model)
        {
            // создадим пустой валидатор c одним полем
            string newValidatorText = CreateSimpleValidatorText<T>(fieldName, valueToSet);

            var result = ValidationServices.ValidateModel(model, newValidatorText, null);

            Assert.IsTrue(result.IsValid);

            // после запуска валидатора модель должна поменяться
            return model;
        }
    }
}
