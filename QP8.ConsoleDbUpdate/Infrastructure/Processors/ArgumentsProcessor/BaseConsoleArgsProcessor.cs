using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using Mono.Options;
using QP8.Infrastructure;
using QP8.Infrastructure.Helpers;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Enums;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Factories;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Helpers;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Providers.ConfigurationProvider;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.ArgumentsProcessor
{
    internal abstract class BaseConsoleArgsProcessor
    {
        internal ApplicationSettings Settings { get; set; }

        internal ConfigurationSettings Config { get; }

        protected BaseConsoleArgsProcessor()
        {
            Settings = new();
            Config = new();
        }

        protected internal abstract BaseSettingsModel CreateSettingsFromArguments();

        protected internal virtual OptionSet BuildOptionSet()
        {
            return new OptionSet();
        }

        protected internal virtual void PrintEnteredData()
        {
            Console.WriteLine($@"Connection String: {Settings.ConnectionString}");
            Console.WriteLine($@"Customer Code: {Settings.CustomerCode}");
            Console.WriteLine($@"Files: {string.Join(", ", Settings.FilePathes)}");
            Console.WriteLine($@"Verbosity Level: {Program.VerboseLevel}");
            Console.WriteLine($@"Is silent mode enabled: {Program.IsSilentModeEnabled}");
            Console.WriteLine();

            if (!Program.IsSilentModeEnabled)
            {
                ConsoleHelpers.MakeSureUserWantToContinue();
            }
        }

        public BaseSettingsModel ParseConsoleArguments(string[] args)
        {
            Program.Logger.Debug("Parse utility settings for selected processor..");

            OptionSet optionSet = BuildOptionSet();
            optionSet = AddCommonOptions(optionSet);

            try
            {
                List<string> noNamedOptions = optionSet.Parse(args);
                if (Program.ShouldShowHelp)
                {
                    ShowCommandLineHelp(optionSet);
                }

                ValidateInputData(noNamedOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + Environment.NewLine);
                Console.WriteLine(@"Try `consoledbupdate --help' for more information.");
                Console.WriteLine(Environment.NewLine);
                throw;
            }

            Settings.FilePathes = GenerateFileListFromPathes().ToList();

            IConfigurationProvider provider = new ConfigurationProviderFactory().Create(Config, Settings.ConnectionString);
            Settings = provider.UpdateSettings(Settings);

            Console.WriteLine();
            PrintEnteredData();

            return CreateSettingsFromArguments();
        }

        protected internal OptionSet AddCommonOptions(OptionSet optionSet)
        {
            // TODO: REMOVE AFTER RESHARPER FIX BUG https://youtrack.jetbrains.com/issue/RSRP-466882
            _ = optionSet.Add("disablePipedInput", "internal temp mode for internal use only", i => { });

            if (!(Console.IsInputRedirected && !Program.DisablePipedInput))
            {
                _ = optionSet.Add("p|path=", "single or multiple <path> to file|directory with xml|csv record actions to replay", p => Settings.FilePathes.Add(p));
            }

            _ = optionSet.Add("t|type=", "database type, 0 (default) - SqlServer, 1 - postgres", c => { Settings.DbType = Enum.TryParse<DatabaseType>(c, out DatabaseType dbtype) ? dbtype : DatabaseType.SqlServer; });
            _ = optionSet.Add("c|config=", "full path to qp xml config file", c => Config.QpConfigurationPath = c);
            _ = optionSet.Add("r|remoteConfig=", "url to qp configuration service", r => Config.QpConfigurationServiceUrl = r);
            _ = optionSet.Add("j|jwt=", "jwt token for access qp configuration service", j => Config.QpConfigurationServiceToken = j);
            _ = optionSet.Add("v|verbose", "increase debug message verbosity [v|vv|vvv]:[error|warning|info].", v => { });
            _ = optionSet.Add("s|silent", "enable silent mode for automatization.", s => { });
            _ = optionSet.Add("m|mode=", "single value which represents utility mode [xml|csv]", m => { });
            _ = optionSet.Add("h|help", "show this message and exit", h => Program.ShouldShowHelp = h != null);
            return optionSet;
        }

        private void ValidateInputData(ICollection<string> noNamedOptions)
        {
            Ensure.That<OptionException>(noNamedOptions.Count == 1, $"Should specify single not named parameter <customer_code|connection_string>, but was {string.Join(",", noNamedOptions)}", "customer_code|connection_string");
            if (!(Console.IsInputRedirected && !Program.DisablePipedInput))
            {
                Ensure.That<OptionException>(Settings.FilePathes.Any(), "Should specify at least one xml file or folder path", "path");
            }

            string nonamedOption = noNamedOptions.Single();

            if (SqlHelpers.TryParseConnectionString(nonamedOption, (QP.ConfigurationService.Models.DatabaseType)(int)Settings.DbType, out DbConnectionStringBuilder _))
            {
                Settings.ConnectionString = nonamedOption;
            }
            else
            {
                Settings.CustomerCode = nonamedOption;
            }
        }

        private static void ShowCommandLineHelp(OptionSet optionsSet)
        {
            Console.WriteLine(@"USAGE: consoledbupdate [OPTIONS]+ <customer_code|connection_string>" + Environment.NewLine);
            Console.WriteLine(@"OPTIONS:");
            optionsSet.WriteOptionDescriptions(Console.Out);
            ConsoleHelpers.ExitProgram(ExitCode.Success);
        }

        private IEnumerable<string> GenerateFileListFromPathes()
        {
            List<string> unorderedFilePathes = new();
            string extension = GetExpectedFileExtension();

            foreach (string path in Settings.FilePathes)
            {
                if (!File.Exists(path) && !Directory.Exists(path))
                {
                    throw new FileNotFoundException($"There is no such file or directory on file system: {path}");
                }

                if ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    unorderedFilePathes.AddRange(GetFilePathesFromDirectory(path, extension));
                }
                else
                {
                    if (Path.GetExtension(path) != extension)
                    {
                        Console.WriteLine($"Skipping file {path} because expecting files only with extension {extension}");
                    }
                    else
                    {
                        unorderedFilePathes.Add(path);
                    }
                }
            }

            return unorderedFilePathes.Distinct().OrderBy(x => Path.GetFileName(x));

            IEnumerable<string> GetFilePathesFromDirectory(string path, string extension)
            {
                return Directory.EnumerateFiles(path, $"*{extension}", SearchOption.TopDirectoryOnly).ToList();
            }

            string GetExpectedFileExtension()
            {
                return GetType() switch
                {
                    Type xmlType when xmlType == typeof(XmlConsoleArgsProcessor) => ".xml",
                    Type csvType when csvType == typeof(CsvConsoleArgsProcessor) => ".csv",
                    _ => throw new InvalidOperationException("Unknown args processor type"),
                };
            }
        }
    }
}
