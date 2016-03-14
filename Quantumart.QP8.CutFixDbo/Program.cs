using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Quantumart.QP8.CutFixDbo
{
	class Program
	{
		static int Main(string[] args)
		{
			try
			{
				if (args.Length < 2 || String.IsNullOrWhiteSpace(args[0]) || String.IsNullOrWhiteSpace(args[1]))
				{
					Console.Out.WriteLine("Parameter Error. Correct is: CutFixDbo.exe {current db version} {fix.dbo.sql path} Example: CutFixDbo.exe 7.8.0.2 c:\temp\fix_dbo.sql");
					return 1;
				}

				string searchingString = String.Concat("PRINT '", args[0]);
				using (var reader = new StreamReader(args[1], Encoding.GetEncoding(1251)))
				{
					while (reader.Peek() >= 0)
					{
						if (reader.ReadLine().Contains(searchingString))
						{
							Console.OutputEncoding = Encoding.GetEncoding(1251);
							Console.Out.Write(reader.ReadToEnd());
							return 0;
						}
					}
				}

				return 0;
			}
			catch (Exception e)
			{
				Console.Out.WriteLine(String.Format("Error: {0}", e.Message));
				return 1;
			}
		}
	}
}
