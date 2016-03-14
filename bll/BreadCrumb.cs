using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
	public class EntityInfo
	{
		public long Id
		{
			get;
			set;
		}

		public string Code
		{
			get;
			set;
		}

		public long? ParentId
		{
			get;
			set;
		}

		public bool IsFolder
		{
			get;
			set;
		}

		public string Title
		{
			get;
			set;
		}

		public string ActionCode
		{
			get;
			set;
		}

		public string EntityTypeName
		{
			get;
			set;
		}

		public static EntityInfo Create(DataRow r)
		{
			EntityInfo info = new EntityInfo();
			info.Id = Converter.ToInt64(r["ID"]);
			info.Code = r["CODE"].ToString();
			info.ParentId = Converter.ToNullableInt64(r["PARENT_ID"]);
			info.IsFolder = Converter.ToBoolean(r["IS_FOLDER"]);
			info.Title = r["TITLE"].ToString();
			info.ActionCode = r["ACTION_CODE"].ToString();
			info.EntityTypeName = r["NAME"].ToString();
			return info;
		}
	}
}
