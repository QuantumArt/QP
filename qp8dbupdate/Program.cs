using System;
using Quantumart.QP8.BLL.Factories;
using Quantumart.QP8.BLL.Interfaces.Logging;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Factories;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;

namespace Quantumart.QP8.ConsoleDbUpdate
{
    internal class Program
    {
        internal static readonly ILog Logger;

        static Program()
        {
            LogManager.LogFactory = new NLogFactory();
            Logger = LogManager.GetLogger("QP8Update");

            try
            {
                //EmbeddedAssemblyManager.LoadAssembliesAndAttachEvents();
            }
            catch (Exception ex)
            {
                Logger.Fatal("There was an exception while loading assemblies", ex);
                Console.WriteLine("There was an exception in xml db updater");

                Logger.Flush();
                Environment.Exit(1);
            }
        }


        public static void Main(string[] args)
        {
            Logger.Debug($"Console db updater is started. Args: {args.ToJsonLog()}");

            try
            {
                ConsoleHelpers.PrintHelloMessage();
                var selectedMode = ConsoleHelpers.AskUserToSelectUtilityMode();
                var argsParser = ConsoleArgsProcessorFactory.Create(selectedMode);
                var settings = argsParser.ParseConsoleArguments(args);
                Logger.Debug($"Parsed settings: {settings.ToJsonLog()}");

                var dataProcessor = DataProcessorFactory.Create(settings);
                dataProcessor.Process();
            }
            catch (Exception ex)
            {
                Logger.Fatal("There was an exception in xml db updater", ex);
                Console.WriteLine("There was an exception in xml db updater");

                Logger.Flush();
                Environment.Exit(1);
            }

            Console.WriteLine("Processing successfuly finished...");

            Logger.Flush();
            Environment.Exit(0);
        }
    }
}
