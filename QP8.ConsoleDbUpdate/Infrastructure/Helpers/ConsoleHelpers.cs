using System;
using System.IO;
using System.Text;
using Mono.Options;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Helpers
{
    internal static class ConsoleHelpers
    {
        internal static void SetupConsole()
        {
            Program.Logger.Debug("Setup initial console settings..");
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
        }

        internal static void ReadFromStandardInput()
        {
            if (Console.IsInputRedirected && !Program.DisablePipedInput)
            {
                Program.Logger.Debug("Read from redirected input..");
                using (var inputStream = Console.OpenStandardInput())
                using (var streamReader = new StreamReader(inputStream, Console.InputEncoding))
                {
                    Program.StandardInputData = streamReader.ReadToEnd();
                }
            }
        }

        internal static void PrintHelloMessage()
        {
            Program.Logger.Debug("Print hello message..");

            WriteLineDebug();
            Console.WriteLine(@"QuantumArt DbUpdate for QP8 version 6.0.");
            Console.WriteLine($@"Assembly version {CommonHelpers.GetAssemblyVersion()}.");
        }

        internal static void WriteLineDebug(string message = null)
        {
            WriteDebug($"{message}{Environment.NewLine}");
        }

        internal static void WriteDebug(string message = null)
        {
#if DEBUG
            Console.Write(message ?? string.Empty);
#endif
        }

        internal static void ClearAndPrintHeader(string headerMessage)
        {
            if (!Program.IsSilentModeEnabled)
            {
                Console.Clear();
                Console.WriteLine(headerMessage);
                Console.WriteLine();
            }
        }

        internal static void MakeSureUserWantToContinue()
        {
            Console.Write("Continue processing (y/n): ");
            var enteredKey = Console.ReadKey().Key;
            Console.WriteLine();

            switch (enteredKey)
            {
                case ConsoleKey.Y:
                    Console.WriteLine();
                    break;
                case ConsoleKey.Escape:
                case ConsoleKey.N:
                    ExitProgram(ExitCode.Success);
                    break;
                default:
                    MakeSureUserWantToContinue();
                    break;
            }
        }

        internal static ConsoleKey GetUtilityMode(string[] args)
        {
            Program.Logger.Debug("Parse initial utility settings..");

            var utilityMode = string.Empty;
            var options = new OptionSet
            {
                { "m|mode=", m => { utilityMode = m?.Trim().ToLower(); } },
                { "v|verbose", "increase debug message verbosity. [v|vv|vvv]:[error|warning|info]", v => ++Program.VerboseLevel },
                { "s|silent", "enable silent mode for automatization", s => Program.IsSilentModeEnabled = s != null }
            };

            options.Parse(args);
            if (Program.IsSilentModeEnabled)
            {
                if (Console.IsInputRedirected && !Program.DisablePipedInput)
                {
                    utilityMode = string.IsNullOrWhiteSpace(utilityMode) ? "auto" : utilityMode;
                }
                else if (string.Equals("auto", utilityMode))
                {
                    throw new OptionException(@"You cannot choose ""auto"" mode (m|mode=) with ""-p"" flag (p|path=)", "m|mode=");
                }

                if (string.Equals("ask", utilityMode))
                {
                    throw new OptionException(@"You cannot choose ""ask"" mode (m|mode=) with ""-s"" flag (s|silent)", "m|mode=");
                }
            }
            else
            {
                utilityMode = string.IsNullOrWhiteSpace(utilityMode) ? "ask" : utilityMode;
            }

            switch (utilityMode.ToLower())
            {
                case "auto":
                    return ConsoleKey.NumPad0;
                case "xml":
                    return ConsoleKey.NumPad1;
                case "csv":
                    return ConsoleKey.NumPad2;
                case "ask":
                    return AskUserToSelectUtilityMode();
            }

            throw new OptionException("Unexpected utility mode was specified", "m|mode=");
        }

        internal static ConsoleKey AskUserToSelectUtilityMode()
        {
            if (Program.IsSilentModeEnabled && !(Console.IsInputRedirected && !Program.DisablePipedInput))
            {
                throw new OptionException("You should specify mode for silent work or use piped input", "m|mode=");
            }

            Console.WriteLine(@"Please choose option to process:");
            Console.WriteLine(@"1. Continue with XmlImport");
            Console.WriteLine(@"2. Continue with CsvImport");
            Console.WriteLine(@"3. Quit");

            return Console.ReadKey().Key;
        }

        internal static void ExitProgram(ExitCode exitCode)
        {
            const string successMsg = "Processing successfuly finished...";
            const string warningMsg = "Some data already exists at database. Check logs for detailed information...";
            const string errorMsg = "There was an exception in xml db updater. Check logs for detailed information...";
            Console.WriteLine();

            switch (exitCode)
            {
                case ExitCode.Success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(successMsg);
                    break;
                case ExitCode.DbUpdateError:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Error.WriteLine(warningMsg);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(successMsg);
                    break;
                case ExitCode.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine(errorMsg);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(exitCode), exitCode, "Unknown exit code");
            }

            Console.ResetColor();
            Console.Out.Flush();
            Console.Error.Flush();
            Program.Logger.Dispose();

            Environment.Exit((int)exitCode);
        }
    }
}
