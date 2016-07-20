using System;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Helpers;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.ArgumentsProcessor;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Factories
{
    internal class ConsoleArgsProcessorFactory
    {
        internal static BaseConsoleArgsProcessor Create(ConsoleKey userSelectedMode)
        {
            BaseConsoleArgsProcessor processor = null;
            switch (userSelectedMode) {
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
                    Environment.Exit(0);
                    break;
                default:
                    ConsoleHelpers.ClearAndPrintHeader("Unknown option. Try again...");
                    return Create(ConsoleHelpers.AskUserToSelectUtilityMode());
            }


            return processor;
        }
    }
}
