using System;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Helpers
{
    internal static class ConsoleHelpers
    {
        internal static void PrintHelloMessage()
        {
            Console.WriteLine("QuantumArt DBUpdate for QP8 version 6.0.");
            Console.WriteLine($"Assembly version {typeof(Program).Assembly.GetName().Version}.");
        }

        internal static void ClearAndPrintHeader(string headerMessage)
        {
            Console.Clear();
            Console.WriteLine(headerMessage);
            Console.WriteLine();
        }

        internal static void MakeSureUserWantToContinue()
        {
            Console.Write("Continue processing (y/n): ");
            if (Console.ReadKey().Key != ConsoleKey.Y)
            {
                Environment.Exit(0);
            }

            Console.WriteLine(Environment.NewLine);
        }

        internal static ConsoleKey AskUserToSelectUtilityMode()
        {
            Console.WriteLine("Please choose option to process:");
            Console.WriteLine("1. Continue with XmlImport");
            Console.WriteLine("2. Continue with CsvImport");
            Console.WriteLine("3. Quit");

            return Console.ReadKey().Key;
        }
    }
}
