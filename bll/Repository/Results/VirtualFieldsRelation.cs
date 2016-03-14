using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Quantumart.QP8.BLL.Repository.Results
{
	/// <summary>
	/// Связь между виртуальными полями
	/// </summary>
	internal class VirtualFieldsRelation : IEqualityComparer, IEqualityComparer<VirtualFieldsRelation>
	{
		public int BaseFieldId { get; set; }
		public int BaseFieldContentId { get; set; }

		public int VirtualFieldId { get; set; }
		public int VirtualFieldContentId { get; set; }

		#region IEqualityComparer Members

		bool IEqualityComparer.Equals(object x, object y)
		{
			return Equals(x as VirtualFieldsRelation, y as VirtualFieldsRelation);
		}

		public int GetHashCode(object obj)
		{
			return GetHashCode(obj as VirtualFieldsRelation);
		}

		#endregion

		#region IEqualityComparer<VirtualFieldsRelation> Members

		public bool Equals(VirtualFieldsRelation x, VirtualFieldsRelation y)
		{
			if (x == null && y == null)
				return true;
			else if ((x != null && y == null) || (x == null && y != null))
				return false;
			else
				return x.BaseFieldId.Equals(y.BaseFieldId) && x.VirtualFieldId.Equals(y.VirtualFieldId);
		}

		public int GetHashCode(VirtualFieldsRelation obj)
		{
			if (obj == null)
				return -1;
			else
				return obj.GetHashCode();
		}

		#endregion

		public override int GetHashCode()
		{
			return 31 * (BaseFieldId + VirtualFieldId);
		}

		public override bool Equals(object obj)
		{
			return Equals(this, obj as VirtualFieldsRelation);
		}
	}
}
