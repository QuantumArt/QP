using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL
{
	/// <summary>
	/// Типы файлов
	/// </summary>
	public enum FolderFileType : int
	{
		Unknown = 0,
		Image = 1,
		CSS = 2,
		Javascript = 3,
		Flash = 4,
		Media = 5,
		PDF = 6,
		Office = 7
	}
}
