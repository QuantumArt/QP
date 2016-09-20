using System;
using Mono.Options;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Enums;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Helpers
{
    internal static class ConsoleHelpers
    {
        internal static void PrintHelloMessage()
        {
            WriteLineDebug();
            Console.WriteLine("QuantumArt DBUpdate for QP8 version 6.0.");
            Console.WriteLine($"Assembly version {typeof(Program).Assembly.GetName().Version}.");
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

        internal static ConsoleKey AskUserToSelectUtilityMode()
        {
            Console.WriteLine("Please choose option to process:");
            Console.WriteLine("1. Continue with XmlImport");
            Console.WriteLine("2. Continue with CsvImport");
            Console.WriteLine("3. Quit");

            return Console.ReadKey().Key;
        }

        internal static ConsoleKey GetUtilityMode(string[] args)
        {
            var utilityMode = "ask";
            var options = new OptionSet
            {
                { "m|mode=", m => { utilityMode = m; } },
                { "s|silent", "enable silent mode for automatization", s => Program.IsSilentModeEnabled = s != null },
                { "v|verbose", "increase debug message verbosity. [v|vv|vvv]:[error|warning|info]", v => ++Program.VerboseLevel }
            };

            options.Parse(args);
            switch (utilityMode.ToLower())
            {
                case "xml":
                    return ConsoleKey.NumPad1;
                case "csv":
                    return ConsoleKey.NumPad2;
                case "ask":
                    return AskUserToSelectUtilityMode();
            }

            throw new OptionException("Unexpected utility mode was specified", "m|mode=");
        }

        internal static void ExitProgram(ExitCode exitCode)
        {
            Console.WriteLine(exitCode == ExitCode.Success
                ? "Processing successfuly finished."
                : ">>> There was an exception in xml db updater <<<");

            Program.Logger.Flush();
            Environment.Exit((int)exitCode);
        }
    }
}
