using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Quantumart.QP8.Deploy.RemoteBatch.Client
{
	class Program
	{
		static int Main(string[] args)
		{
			try
			{
				Console.Out.WriteLine("Remote Deployment");
				Console.Out.WriteLine("Start: " + DateTime.Now.ToShortTimeString());

				if (args.Length < 1 || String.IsNullOrWhiteSpace(args[0]))
				{
					Console.Out.WriteLine(@"Parameter Error. Correct is: RemoteBatchServer.exe {Remote Batch Server URL} Example: RemoteBatchServer.exe http://updateadmin8.qpublishing.ru");
					return 1;
				}

				var cancellationTokenSource = new CancellationTokenSource();
				var task = new Task(() =>
				{

					do
					{
						Console.Write(".");
					}
					while (!cancellationTokenSource.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(0.3)));
				}, cancellationTokenSource.Token);
				task.Start();

				WebClient client = new WebClient();
				string reply = client.DownloadString(args[0]);				
				
				cancellationTokenSource.Cancel();
				task.Wait(TimeSpan.FromSeconds(1));

				Console.WriteLine(reply);
				
				Console.Out.WriteLine("Remote Deployment Complete: " + DateTime.Now.ToShortTimeString());				
				return 0;
			}
			catch (Exception e)
			{
				Console.Out.WriteLine(String.Format("Remote Deployment Error: {0}", e.Message));
				Console.Out.WriteLine(DateTime.Now.ToShortTimeString());
				return 1;
			}
		}
		
	}
}
