using System;
using Mono.Options;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Helpers
{
    internal static class ConsoleHelpers
    {
        internal static void PrintHelloMessage()
        {
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
            var utilityMode = "ask";
            var options = new OptionSet
            {
                { "m|mode=", m => { utilityMode = m; } },
                { "s|silent", "enable silent mode for automatization", s => Program.IsSilentModeEnabled = s != null },
                { "v|verbose", "increase debug message verbosity. [v|vv|vvv]:[error|warning|info]", v => ++Program.VerboseLevel },
                { "h|help", "show this message and exit", h => utilityMode = h != null ? "help" : utilityMode }
            };

            options.Parse(args);
            switch (utilityMode.ToLower())
            {
                case "xml":
                    return ConsoleKey.NumPad1;
                case "csv":
                    return ConsoleKey.NumPad2;
                case "help":
                    return AskUserToSelectHelpMode();
                case "ask":
                    return AskUserToSelectUtilityMode();
            }

            throw new OptionException("Unexpected utility mode was specified", "m|mode=");
        }

        internal static ConsoleKey AskUserToSelectHelpMode()
        {
            Console.WriteLine(@"1. Show help for XmlImport");
            Console.WriteLine(@"2. Show help for CsvImport");
            Console.WriteLine(@"3. Quit");

            return Console.ReadKey().Key;
        }

        internal static ConsoleKey AskUserToSelectUtilityMode()
        {
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
                    Console.WriteLine(warningMsg);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(successMsg);
                    break;
                case ExitCode.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(errorMsg);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(exitCode), exitCode, "Unknown exit code");
            }

            Console.ResetColor();
            Program.Logger.Dispose();
            Environment.Exit((int)exitCode);
        }
    }
}
