using Quantumart.QP8.BLL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL
{
	public class ObjectType : EntityObject
	{
		private const string _generic = "Generic";
		private const string _container = "Publishing Container";
		private const string _form = "Publishing Form";
		private const string _javaScript = "JavaScript";
		private const string _css = "Style Sheet (CSS)";

		public static ObjectType GetGeneric()
		{
			return ObjectTypeRepository.GetByName(_generic);
		}

		public static ObjectType GetContainer()
		{
			return ObjectTypeRepository.GetByName(_container);
		}

		public static ObjectType GetForm()
		{
			return ObjectTypeRepository.GetByName(_form);
		}

		public static ObjectType GetJavaScript()
		{
			return ObjectTypeRepository.GetByName(_javaScript);
		}

		public static ObjectType GetCss()
		{
			return ObjectTypeRepository.GetByName(_css);
		}
	}
}
