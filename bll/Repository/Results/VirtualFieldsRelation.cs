using System.Collections;
using System.Collections.Generic;

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

		bool IEqualityComparer.Equals(object x, object y) => Equals(x as VirtualFieldsRelation, y as VirtualFieldsRelation);

	    public int GetHashCode(object obj) => GetHashCode(obj as VirtualFieldsRelation);

	    #endregion

		#region IEqualityComparer<VirtualFieldsRelation> Members

		public bool Equals(VirtualFieldsRelation x, VirtualFieldsRelation y)
		{
		    if (x == null && y == null)
			{
			    return true;
			}

		    if ((x != null && y == null) || (x == null && y != null))
		    {
		        return false;
		    }

		    return x.BaseFieldId.Equals(y.BaseFieldId) && x.VirtualFieldId.Equals(y.VirtualFieldId);
		}

		public int GetHashCode(VirtualFieldsRelation obj)
		{
		    if (obj == null)
			{
			    return -1;
			}

		    return obj.GetHashCode();
		}

		#endregion

		public override int GetHashCode() => 31 * (BaseFieldId + VirtualFieldId);

	    public override bool Equals(object obj) => Equals(this, obj as VirtualFieldsRelation);
	}
}
