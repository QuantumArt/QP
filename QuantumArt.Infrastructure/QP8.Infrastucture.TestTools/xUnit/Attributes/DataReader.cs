using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit.Sdk;

namespace QP8.Infrastucture.TestTools.xUnit.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class DataReaderAttribute : DataAttribute
    {
        private readonly string _fileName;

        public DataReaderAttribute(string fileName)
        {
            _fileName = fileName;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (testMethod == null)
            {
                throw new ArgumentNullException(nameof(testMethod));
            }

            var result = new List<object>
            {
                File.ReadAllText(GetFullFilename(_fileName))
            };

            return new[]
            {
                result.ToArray()
            };
        }

        private static string GetFullFilename(string filename)
        {
            var executable = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            var filePath = Path.Combine(Path.GetDirectoryName(executable) ?? string.Empty, filename);
            return Path.GetFullPath(filePath);
        }
    }
}
