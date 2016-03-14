using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.DTO
{
	public class UserListItem
	{
		public int Id { get; set; }

		public bool Disabled { get; set; }

		public string DisabledIcon 
		{ 
			get 
			{
				return Disabled ? "user1.gif" : "user0.gif";
			} 
		}
		public string DisabledText
		{
			get
			{
				return Disabled ? UserStrings.Disabled : UserStrings.Active;
			}
		}

		public string Login { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string Email { get; set; }

		public int LanguageId { get; set; }

		public string Language { get; set; }

		public DateTime? LastLogOn { get; set; }

		public DateTime Created { get; set; }

		public DateTime Modified { get; set; }

		public int LastModifiedByUserId { get; set; }

		public string LastModifiedByUser { get; set; }

		public static string TranslateSortExpression(string sortExpression)
		{
			Dictionary<string, string> replaces = new Dictionary<string, string>() { 
				{"LastModifiedByUserId", "LAST_MODIFIED_BY"},
				{"LastModifiedByUser", "LAST_MODIFIED_BY_LOGIN"},
				{"FirstName", "FIRST_NAME"},
				{"LastName", "LAST_NAME"},
				{"LanguageId", "LANGUAGE_ID"},
				{"Language", "LANGUAGE_NAME"},
				{"LastLogOn", "LAST_LOGIN"},

			};
			return TranslateHelper.TranslateSortExpression(sortExpression, replaces);
		}
	}
}
