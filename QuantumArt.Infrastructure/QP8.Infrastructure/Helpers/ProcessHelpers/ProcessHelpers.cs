using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace QP8.Infrastructure.Helpers.ProcessHelpers
{
    public class ProcessHelpers
    {
        public static int ExecuteFileAndReadOutput(ProcessExecutionSettings processExecutionSettings) => ExecuteFileAndReadOutput(processExecutionSettings, out var _, out var _);

        public static int ExecuteFileAndReadOutput(ProcessExecutionSettings processExecutionSettings, out string standardOutput, out string standardError)
        {
            Ensure.NotNullOrWhiteSpace(processExecutionSettings.ProcessExePath, "Should specify process executable file path");

            using (var outputWaitHandle = new AutoResetEvent(false))
            using (var errorWaitHandle = new AutoResetEvent(false))
            {
                using (var dbUpdateProcess = new Process())
                {
                    dbUpdateProcess.StartInfo.FileName = processExecutionSettings.ProcessExePath;
                    if (!string.IsNullOrWhiteSpace(processExecutionSettings.Arguments))
                    {
                        dbUpdateProcess.StartInfo.Arguments = processExecutionSettings.Arguments;
                    }

                    dbUpdateProcess.StartInfo.CreateNoWindow = true;
                    dbUpdateProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    dbUpdateProcess.EnableRaisingEvents = true;
                    dbUpdateProcess.StartInfo.UseShellExecute = false;
                    dbUpdateProcess.StartInfo.RedirectStandardError = true;
                    dbUpdateProcess.StartInfo.RedirectStandardOutput = true;
                    dbUpdateProcess.StartInfo.StandardErrorEncoding = processExecutionSettings.OutputEncoding;
                    dbUpdateProcess.StartInfo.StandardOutputEncoding = processExecutionSettings.OutputEncoding;
                    if (!string.IsNullOrWhiteSpace(processExecutionSettings.StandardInputData))
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
                        if (!string.IsNullOrWhiteSpace(processExecutionSettings.StandardInputData))
                        {
                            using (var sw = dbUpdateProcess.StandardInput)
                            {
                                sw.Write(processExecutionSettings.StandardInputData.Trim());
                            }
                        }

                        if (dbUpdateProcess.WaitForExit(processExecutionSettings.ProcessTimeout))
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
                        outputWaitHandle.WaitOne(processExecutionSettings.WaitForOutputTimeout);
                        errorWaitHandle.WaitOne(processExecutionSettings.WaitForOutputTimeout);
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
