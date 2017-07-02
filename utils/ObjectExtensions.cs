using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Quantumart.QP8.Utils
{
	public static class ObjectExtensions
	{
		public static void SetField<T>(this object source, string fieldName, T value)
		{
			var type = source.GetType();
			var info = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (info != null)
			{
				info.SetValue(source, value);
			}
		}

        public static IEnumerable<string> GetFields(this object source)
        {
            var type = source.GetType();
            var fields = type.GetFields();
            return fields.Select(f => f.GetValue(source).ToString()).ToArray();
        }

	}
}
