using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
	public class VisualEditorConfig
	{
		private const int CKEDITOR_ENTER_P	= 1;
		private const int CKEDITOR_ENTER_BR = 2;

		private readonly int ToolbarsHeight = 110;
				

		internal VisualEditorConfig(Field field)
		{
			Field = field;
		}

		private Field Field { get; set; }

		public int Height
		{
			get
			{
				return Field.VisualEditorHeight - ToolbarsHeight;
			}
		}
		

		public string DocType
		{
			get
			{
				if (!String.IsNullOrWhiteSpace(Field.DocType))
					return Field.DocType;
				else
					return @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">";
			}
		}


		public int EnterMode
		{
			get
			{
				if (Field.PEnterMode)
					return CKEDITOR_ENTER_P;
				else
					return CKEDITOR_ENTER_BR;
			}
		}

		public int ShiftEnterMode
		{
			get
			{
				if (Field.PEnterMode)
					return CKEDITOR_ENTER_BR;
				else
					return CKEDITOR_ENTER_P;
			}
		}

		public bool UseEnglishQuotes
		{
			get
			{
				return Field.UseEnglishQuotes;					
			}
		}

		public string Language
		{
			get
			{
				return QPContext.CurrentUserIdentity.CultureName;
			}
		}

		public bool FullPage
		{
			get
			{
				return Field.FullPage;
			}
		}		
	}
}
