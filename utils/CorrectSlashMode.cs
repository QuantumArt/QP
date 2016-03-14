using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.Utils
{
	[Flags]
	public enum CorrectSlashMode
	{
		ConvertSlashesToBackSlashes = 1,
		ConvertBackSlashesToSlashes = 2,
		RemoveFirstSlash = 4,
		RemoveLastSlash = 8,
		WrapToSlashes = 16,
		ReplaceDoubleSlashes = 32
	}
}
