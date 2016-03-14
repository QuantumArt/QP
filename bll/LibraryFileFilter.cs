using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL
{
	/// <summary>
	/// Фильтр файлов в библиотеке
	/// </summary>
	public class LibraryFileFilter
	{
		public FolderFileType? FileType { get; set; }
		public string FileNameFilter { get; set; }

		public string Mask
		{
			get
			{
				return String.IsNullOrEmpty(FileNameFilter) ? "*" : String.Format("*{0}*", FileNameFilter);
			}
		}
	}
}
