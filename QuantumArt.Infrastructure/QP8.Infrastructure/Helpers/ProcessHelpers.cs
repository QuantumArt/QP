using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace QP8.Infrastructure.Helpers
{
    public class ProcessHelpers
    {
        public static int ExecuteFileAndReadOutput(string fileName, string arguments) => ExecuteFileAndReadOutput(fileName, arguments, out var _, out var _);

        public static int ExecuteFileAndReadOutput(string fileName, string arguments, out string standardOutput, out string standardError, string standardInput = null)
        {
            using (var outputWaitHandle = new AutoResetEvent(false))
            using (var errorWaitHandle = new AutoResetEvent(false))
            {
                using (var dbUpdateProcess = new Process())
                {
                    dbUpdateProcess.StartInfo.FileName = fileName;
                    dbUpdateProcess.StartInfo.Arguments = arguments;

                    dbUpdateProcess.StartInfo.CreateNoWindow = true;
                    dbUpdateProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    dbUpdateProcess.EnableRaisingEvents = true;
                    dbUpdateProcess.StartInfo.UseShellExecute = false;
                    dbUpdateProcess.StartInfo.RedirectStandardError = true;
                    dbUpdateProcess.StartInfo.RedirectStandardOutput = true;
                    dbUpdateProcess.StartInfo.StandardErrorEncoding = Encoding.UTF8;
                    dbUpdateProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                    if (!string.IsNullOrWhiteSpace(standardInput))
                    {
                        dbUpdateProcess.StartInfo.RedirectStandardInput = true;
                    }

                    var outputBuilder = new StringBuilder();
                    var errorBuilder = new StringBuilder();

                    int exitCode;
                    try
                    {
                        dbUpdateProcess.ErrorDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                // ReSharper disable once AccessToDisposedClosure
                                errorWaitHandle.Set();
                            }
                            else
                            {
                                errorBuilder.AppendLine(e.Data);
                            }
                        };

                        dbUpdateProcess.OutputDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                // ReSharper disable once AccessToDisposedClosure
                                outputWaitHandle.Set();
                            }
                            else
                            {
                                outputBuilder.AppendLine(e.Data);
                            }
                        };

                        dbUpdateProcess.Start();
                        dbUpdateProcess.BeginErrorReadLine();
                        dbUpdateProcess.BeginOutputReadLine();
                        dbUpdateProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                        if (!string.IsNullOrWhiteSpace(standardInput))
                        {
                            using (var sw = dbUpdateProcess.StandardInput)
                            {
                                sw.Write(standardInput.Trim());
                            }
                        }

                        const int processTimeout = 60000;
                        if (dbUpdateProcess.WaitForExit(processTimeout))
                        {
                            exitCode = dbUpdateProcess.ExitCode;
                        }
                        else
                        {
                            throw new Exception("There was an timeout exception while processing console db update task");
                        }

                        standardOutput = outputBuilder.ToString();
                        standardError = errorBuilder.ToString();
                    }
                    finally
                    {
                        const int waitForOutputTimeout = 300;
                        outputWaitHandle.WaitOne(waitForOutputTimeout);
                        errorWaitHandle.WaitOne(waitForOutputTimeout);
                        if (!dbUpdateProcess.HasExited)
                        {
                            dbUpdateProcess.Kill();
                        }
                    }

                    return exitCode;
                }
            }
        }
    }
}
