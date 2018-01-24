using System;
using System.Linq;
using Mono.Options;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Adapters;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Enums;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Factories;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.Exceptions;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers;

namespace Quantumart.QP8.ConsoleDbUpdate
{
    internal class Program
    {
        internal static int VerboseLevel;
        internal static bool ShouldShowHelp;
        internal static bool IsSilentModeEnabled;
        internal static string StandardInputData;
        internal static QpUpdateLoggingWrapper Logger;

        // TODO: REMOVE AFTER RESHARPER FIX BUG https://youtrack.jetbrains.com/issue/RSRP-466882
        internal static bool DisablePipedInput;

        static Program()
        {
            try
            {
                IsSilentModeEnabled = Console.IsInputRedirected;
                EmbeddedAssemblyManager.LoadAssembliesAndAttachEvents();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }
        }

        public static void Main(string[] args)
        {
            // TODO: REMOVE AFTER RESHARPER FIX BUG https://youtrack.jetbrains.com/issue/RSRP-466882
            if (args.Contains("--disablePipedInput"))
            {
                DisablePipedInput = true;
                IsSilentModeEnabled = false;
            }

            // TODO: REMOVE AFTER RESHARPER FIX BUG https://youtrack.jetbrains.com/issue/RSRP-466882

            Logger = new QpUpdateLoggingWrapper();
            Logger.Info($"QuantumArt DbUpdate for QP8 version 6.0. Version: {CommonHelpers.GetAssemblyVersion()}. Args: {args.ToJsonLog()}");

            try
            {
                ConsoleHelpers.SetupConsole();
                ConsoleHelpers.PrintHelloMessage();
                ConsoleHelpers.ReadFromStandardInput();

                var selectedMode = ConsoleHelpers.GetUtilityMode(args);
                Logger.SetLogLevel(VerboseLevel);

                var argsParser = ConsoleArgsProcessorFactory.Create(selectedMode);
                var settings = argsParser.ParseConsoleArguments(args);
                Logger.Debug($"Parsed settings: {settings.ToJsonLog()}");

                var dataProcessor = DataProcessorFactory.Create(settings);
                if (Console.IsInputRedirected && !DisablePipedInput)
                {
                    dataProcessor.Process(StandardInputData);
                }
                else
                {
                    dataProcessor.Process();
                }

                Logger.Debug("Processing successfuly finished..");
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
