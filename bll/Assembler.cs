using System;
using System.Text;

namespace Quantumart.QP8.BLL
{
	public class Assembler
	{
		const int MAXIMUM_STACK_DEPTH = 20;
		const int MAX_PAGE_ASSEMBLING_CYCLES = 10;
		const bool UNICODE = false;
		const string ASSEMBLE_ERROR_INFO = String.Empty;
		const int BLOCK_SIZE = 5;

		public static void AssembleSite(int siteId, int startFrom)
		{
			Assemble(siteId, 0, startFrom);

			//assembleSite = assemble(SiteId, 0, StartFrom)
		}

		public static void Assemble(int siteId, int templateId, int startFrom)
		{
			// SetSiteBorderModes cnn, CLng(siteId)
		}
	}
}
