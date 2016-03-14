using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.IO;

namespace Quantumart.QP8.Deploy.RemoteBatch.Server.Controllers
{
    public class BatchController : Controller
    {
        //
        // GET: /Batch/
		[HttpGet]
		public ActionResult Execute()
		{
			try
			{
				string results = "";
				// Get the full file path
				string strFilePath = Properties.Settings.Default.CmdFilePath;

				// Create the ProcessInfo object
				System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("cmd.exe");
				psi.UseShellExecute = false;
				psi.RedirectStandardOutput = true;
				psi.RedirectStandardInput = true;
				psi.RedirectStandardError = true;
				psi.WorkingDirectory = Path.GetDirectoryName(strFilePath);

				// Start the process
				using (System.Diagnostics.Process proc = System.Diagnostics.Process.Start(psi))
				{
					// Attach the output for reading
					System.IO.StreamReader sOut = proc.StandardOutput;
					// Attach the in for writing
					System.IO.StreamWriter sIn = proc.StandardInput;

					// Open the batch file for reading
					using (System.IO.StreamReader strm = System.IO.File.OpenText(strFilePath))
					{
						// Write each line of the batch file to standard input
						while (strm.Peek() != -1)
						{
							sIn.WriteLine(strm.ReadLine());
						}


						strm.Close();
					}

					// Exit CMD.EXE
					string stEchoFmt = "# {0} run successfully. Exiting";

					sIn.WriteLine(String.Format(stEchoFmt, strFilePath));
					sIn.WriteLine("EXIT");

					// Close the process
					proc.Close();

					// Read the sOut to a string.
					results = sOut.ReadToEnd().Trim();

					// Close the io Streams;
					sIn.Close();
					sOut.Close();
				}

				return Content(results, "text/plain");
			}
			catch (Exception e)
			{
				return Content(e.Message, "text/plain");
			}
		}	

    }
}
