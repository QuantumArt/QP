using System;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Enums;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Helpers;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.ArgumentsProcessor;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Factories
{
    internal class ConsoleArgsProcessorFactory
    {
        internal static BaseConsoleArgsProcessor Create(ConsoleKey userSelectedMode)
        {
            BaseConsoleArgsProcessor processor = null;
            switch (userSelectedMode)
            {
                case ConsoleKey.D0:
                case ConsoleKey.NumPad0:
                    return Create(GetProcessorBasedOnInput());
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    processor = new XmlConsoleArgsProcessor();
                    ConsoleHelpers.ClearAndPrintHeader("Using XML import mode.");
                    break;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    processor = new CsvConsoleArgsProcessor();
                    ConsoleHelpers.ClearAndPrintHeader("Using CSV import mode.");
                    break;
                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    ConsoleHelpers.ExitProgram(ExitCode.Success);
                    break;
                default:
                    ConsoleHelpers.ClearAndPrintHeader("Unknown option. Try again...");
                    return Create(ConsoleHelpers.AskUserToSelectUtilityMode());
            }

            return processor;
        }

        private static ConsoleKey GetProcessorBasedOnInput()
        {
            if (Console.IsInputRedirected)
            {
                var normalizedInput = Program.StandardInputData.Trim().ToLower();
                return normalizedInput.StartsWith("<?xml") && normalizedInput.EndsWith("</actions>")
                    ? ConsoleKey.D1
                    : ConsoleKey.D2;
            }

            return ConsoleHelpers.AskUserToSelectUtilityMode();
        }
    }
}
