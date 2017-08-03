using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QP8.Infrastructure.TestTools.Xunit.Attributes;

namespace QP8.WebMvc.Tests.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class XmlDbUpdateDataReader : DataReaderAttribute
    {
        private readonly string _hash;

        public XmlDbUpdateDataReader(string fileName)
            : base(fileName)
        {
        }

        public XmlDbUpdateDataReader(string fileName, string hash)
            : base(fileName)
        {
            _hash = hash;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var result = base.GetData(testMethod).Single().ToList();
            if (!string.IsNullOrWhiteSpace(_hash))
            {
                result.Add(_hash);
            }

            return new[]
            {
                result.ToArray()
            };
        }
    }
}
