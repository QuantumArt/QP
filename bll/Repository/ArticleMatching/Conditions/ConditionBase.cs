using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Quantumart.QP8.BLL.Repository.ArticleMatching.Conditions
{
	public abstract class ConditionBase : IEnumerable<ConditionBase>, ICloneable
	{
		public ConditionBase()
		{
			Conditions = new ConditionBase[0];
		}

		public ConditionBase[] Conditions { get; set; }
		public abstract string GetCurrentExpression();

		public string[] GetChildExpressions()
		{
			return Conditions.Select(c => c.GetCurrentExpression()).ToArray();
		}

		#region IEnumerable<ConditionBase> implementation
		public IEnumerator<ConditionBase> GetEnumerator()
		{
			yield return this;

			if (Conditions != null)
			{
				foreach (var condition in Conditions)
				{
					foreach (var child in condition)
					{
						yield return child;
					}
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			foreach (var child in this)
			{
				yield return child;
			}
		}
		#endregion

		#region ICloneable implementation
		public ConditionBase Clone()
		{
			var clone = (ConditionBase)MemberwiseClone();
			clone.Conditions = clone.Conditions.Select(c => c.Clone()).ToArray();
			return clone;
		}

		object ICloneable.Clone() => Clone();

	    #endregion
	}
}
