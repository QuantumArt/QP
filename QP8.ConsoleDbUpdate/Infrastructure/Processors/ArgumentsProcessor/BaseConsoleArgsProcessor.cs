using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Mono.Options;
using QP8.Infrastructure;
using QP8.Infrastructure.Helpers;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Enums;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Helpers;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.ArgumentsProcessor
{
    internal abstract class BaseConsoleArgsProcessor
    {
        protected internal string CustomerCode;

        protected internal DatabaseType DbType;

        protected internal IList<string> FilePathes;

        protected internal string ConfigPath;

        protected internal string QpConfigPath;

        protected internal string QpConfigUrl;

        protected internal string QpConfigToken;

        protected internal string ConnectionString;

        protected BaseConsoleArgsProcessor()
        {
            FilePathes = new List<string>();
        }

        protected internal abstract BaseSettingsModel CreateSettingsFromArguments();

        protected internal virtual OptionSet BuildOptionSet()
        {
            return new OptionSet();
        }

        protected internal virtual void PrintEnteredData()
        {
            Console.WriteLine($@"Connection String: {ConnectionString}");
            Console.WriteLine($@"Customer Code: {CustomerCode}");
            Console.WriteLine($@"File Pathes: {string.Join(", ", FilePathes)}");
            Console.WriteLine($@"Config: {(string.IsNullOrWhiteSpace(ConfigPath) ? "disabled" : ConfigPath)}");
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
                Console.WriteLine(@"Try `qpdbupdate --help' for more information.");
                Console.WriteLine(Environment.NewLine);
                throw;
            }

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
                _ = optionSet.Add("p|path=", "single or multiple <path> to file|directory with xml|csv record actions to replay", p => FilePathes.Add(p));
                _ = optionSet.Add("c|config=", "the <path> of xml|csv config file to apply", c => ConfigPath = c);
            }

            _ = optionSet.Add("t|type=", "database type, 0 (default) - SqlServer, 1 - postgres", c => { DbType = Enum.TryParse<DatabaseType>(c, out DatabaseType dbtype) ? dbtype : DatabaseType.SqlServer; });
            _ = optionSet.Add("f|file=", "full path to qp xml config file", f => QpConfigPath = f);
            _ = optionSet.Add("o|onlineStore=", "url to qp configuration service", u => QpConfigUrl = u);
            _ = optionSet.Add("j|jwt=", "jwt token for access qp configuration service", j => QpConfigToken = j);
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
                Ensure.That<OptionException>(FilePathes.Any(), "Should specify at least one xml file or folder path", "path");
            }

            string nonamedOption = noNamedOptions.Single();

            if (SqlHelpers.TryParseConnectionString(nonamedOption, (QP.ConfigurationService.Models.DatabaseType)(int)DbType, out DbConnectionStringBuilder _))
            {
                ConnectionString = nonamedOption;
            }
            else
            {
                CustomerCode = nonamedOption;
            }
        }

        private static void ShowCommandLineHelp(OptionSet optionsSet)
        {
            Console.WriteLine(@"USAGE: qpdbupdate [OPTIONS]+ <customer_code|connection_string>" + Environment.NewLine);
            Console.WriteLine(@"OPTIONS:");
            optionsSet.WriteOptionDescriptions(Console.Out);
            ConsoleHelpers.ExitProgram(ExitCode.Success);
        }
    }
}
