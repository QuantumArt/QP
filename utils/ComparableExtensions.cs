using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.Utils
{
	public static class ComparableExtensions
	{
		public static bool IsInRange<T>(this T value, T left, T right, bool includeBounds = true) where T: IComparable<T>
		{
			if (includeBounds)
				return left.CompareTo(value) <= 0 && right.CompareTo(value) >= 0;
			else
				return left.CompareTo(value) < 0 && right.CompareTo(value) > 0;
		}
	}
}
