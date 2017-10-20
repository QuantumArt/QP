using System.Linq;

namespace Quantumart.QP8.BLL.Repository.ActiveDirectory
{
	internal static class LdapQueryBuilder
	{
		#region Constants
		private const string AndFilter = "(&{0})";
		private const string OrFilter = "(|{0})";
		private const string PersonFilter = "(objectCategory=person)";
		private const string UserFilter = "(objectClass=user)";
		private const string GroupFilter = "(objectCategory=group)";
		private const string MemberOfFilter = "(memberOf={0})";
		private const string CnFilter = "(cn={0})";
		#endregion

		public static string Join(params string[] values) => string.Join(string.Empty, values);

	    public static string Apply(string filter, string value) => string.Format(filter, value);

	    public static string JoinAndApply(string filter, params string[] values)
		{
			var valuesToBeApplied = values.Where(v => !string.IsNullOrEmpty(v)).ToArray();

			if (valuesToBeApplied.Length > 1)
			{
				return string.Format(filter, Join(valuesToBeApplied));
			}

		    return Join(valuesToBeApplied);
		}

		public static string[] ApplyForEach(string filter, params string[] values)
		{
			return values.Where(v => !string.IsNullOrEmpty(v)).Select(v => Apply(filter, v)).ToArray();
		}

		public static string And(params string[] values) => JoinAndApply(AndFilter, values);

	    public static string Or(params string[] values) => JoinAndApply(OrFilter, values);

	    public static string MemberOf(params string[] groups) => Or(ApplyForEach(MemberOfFilter, groups));

	    public static string UserOf(params string[] groups) => And(PersonFilter, UserFilter, MemberOf(groups));

	    public static string GroupOf(string[] names ,params string[] groups) => And(GroupFilter, Or(ApplyForEach(CnFilter, names)), MemberOf(groups));
	}
}
