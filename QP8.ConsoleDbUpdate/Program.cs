using System;
using Mono.Options;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Adapters;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Enums;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Extensions;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Factories;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.Exceptions;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;

namespace Quantumart.QP8.ConsoleDbUpdate
{
    internal class Program
    {
        internal static int VerboseLevel;
        internal static bool IsSilentModeEnabled;
        internal static QpUpdateLoggingWrapper Logger;

        static Program()
        {
            try
            {
                EmbeddedAssemblyManager.LoadAssembliesAndAttachEvents();
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was an exception in xml db updater");
                Console.WriteLine(ex.Dump());
                Environment.Exit(1);
            }
        }

        public static void Main(string[] args)
        {
            Logger = new QpUpdateLoggingWrapper();
            Logger.Debug($"Console db updater is started. Args: {args.ToJsonLog()}");

            try
            {
                ConsoleHelpers.PrintHelloMessage();
                var selectedMode = ConsoleHelpers.GetUtilityMode(args);
                Logger.SetLogLevel(VerboseLevel);

                var argsParser = ConsoleArgsProcessorFactory.Create(selectedMode);
                var settings = argsParser.ParseConsoleArguments(args);
                Logger.Debug($"Parsed settings: {settings.ToJsonLog()}");

                var dataProcessor = DataProcessorFactory.Create(settings);
                dataProcessor.Process();

                Console.WriteLine("Processing successfuly finished...");
                Logger.Debug("Processing successfuly finished...");
                ConsoleHelpers.ExitProgram(ExitCode.Success);
            }
            catch (OptionException ex)
            {
                Logger.Error($"Some params to qpupdate utility was wrong. Option name: \"{ex.OptionName}\"", ex);
                ConsoleHelpers.ExitProgram(ExitCode.Error);
            }
            catch (XmlDbUpdateLoggingException ex)
            {
                Logger.Warn("Some problems were countered while updating database", ex);
                ConsoleHelpers.ExitProgram(ExitCode.DbUpdateError);
            }
            catch (XmlDbUpdateReplayActionException ex)
            {
                Logger.Error("There was a problem while replaying xml db update process", ex);
                ConsoleHelpers.ExitProgram(ExitCode.Error);
            }
            catch (Exception ex)
            {
                Logger.Fatal("There was an unexpected exception in xml db updater", ex);
                ConsoleHelpers.ExitProgram(ExitCode.Error);
            }
        }
    }
}
