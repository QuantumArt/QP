using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using qp8dbupdate.Infrastructure;
using qp8dbupdate.Infrastructure.Exceptions;
using qp8dbupdate.Infrastructure.Extensions;
using qp8dbupdate.Infrastructure.Versioning;

namespace qp8dbupdate
{
    internal class Program
    {
        static Program()
        {
            try
            {
                EmbeddedAssembly.LoadMvcAssemblies();
                EmbeddedAssembly.LoadQpAssemblies();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.QaErrorMessage());
            }

            AppDomain.CurrentDomain.AssemblyResolve += (obj, args) => EmbeddedAssembly.Get(args.Name);
        }

        private static void Main(string[] args)
        {
            try
            {
                var qpReplaySettings = GetQpReplaySettingsFromArguments(args);
                var xmlString = XmlReaderProcessor.GetNodesToReplay(qpReplaySettings.Xmlpath, qpReplaySettings.ConfigPath);

                try
                {
                    using (var service = new QpReplayService(qpReplaySettings.CustomerCode, 1, qpReplaySettings.ReplaySettings, qpReplaySettings.IdentityTypes))
                    {
                        if (qpReplaySettings.SkipLogging)
                        {
                            // TODO: exception is here
                            service.ReplayXml(xmlString);
                        }
                        else
                        {
                            service.ReplayWithLogging(new DbUpdateLogEntry { Applied = DateTime.Now, Body = xmlString, FileName = qpReplaySettings.Xmlpath }, qpReplaySettings.CreateTable);
                        }
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine(Environment.NewLine);
                    Console.WriteLine("При обновлении произошла ошибка.");
                    Console.WriteLine(dbEx.Message);
                    Console.WriteLine("Обновление было произведено {0} пользователем #{1} с помощью файла {2}", dbEx.Entry.Applied, dbEx.Entry.UserId, dbEx.Entry.FileName);
                    Environment.Exit(1);
                }

                Console.WriteLine("Структура базы была успешно обновлена");
            }
            catch (Exception ex)
            {
                var filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LastException.txt");
                Console.WriteLine("При выполнении возникла ошибка: {0} \n\r {1}", ex.Message, ex.StackTrace);
                Console.WriteLine("Подробное описание ошибки в файле {0}", filename);

                try
                {
                    using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                    {
                        var data = Encoding.UTF8.GetBytes(ex.QaErrorMessage());
                        fs.Write(data, 0, data.Length);
                    }
                }
                catch (Exception exc)
                {
                    Console.WriteLine("При записи файла с ошибкой возникла ошибка: {0} \n\r {1}", exc.Message, exc.StackTrace);
                }

                Environment.Exit(1);
            }

            Environment.Exit(0);
        }

        private static QpReplaySettings GetQpReplaySettingsFromArguments(string[] args)
        {
            var arguments = args.ToList();
            var disableContentIdentity = false;
            var disableFieldIdentity = false;
            var skipLogging = false;
            var createTable = false;
            var debug = false;

            foreach (var item in args)
            {
                if (item == "-getScript")
                {
                    Console.WriteLine(UpdateManager.TableQuery);
                    Environment.Exit(0);
                }

                if (item == "-disableContentIdentity")
                {
                    disableContentIdentity = true;
                    arguments.Remove(item);
                }

                if (item == "-disableFieldIdentity")
                {
                    disableFieldIdentity = true;
                    arguments.Remove(item);
                }

                if (item == "-skipLogging")
                {
                    skipLogging = true;
                    arguments.Remove(item);
                }

                if (item == "-createTable")
                {
                    createTable = true;
                    arguments.Remove(item);
                }

                if (item == "-debug")
                {
                    debug = true;
                    arguments.Remove(item);
                }
            }

            if (debug)
            {
                Console.WriteLine("Запуск произведен с возможностью отладки. Нажмите любую клавишу для продолжения...");
                Console.ReadKey(true);
                Debugger.Break();
            }

            Console.WriteLine("DB update for QP8 version 6.0");
            Console.WriteLine("Assembly version {0}", typeof(Program).Assembly.GetName().Version);
            Console.WriteLine((char)64 + " Quantumart " + DateTime.Now.Year);
            Console.WriteLine("ContentIdentity: " + !disableContentIdentity);
            Console.WriteLine("FieldIdentity: " + !disableFieldIdentity);

            if (arguments.Count < 2)
            {
                Console.WriteLine("Указаны не все параметры или их слишком много. Формат использования: ");
                Console.WriteLine("qp8dbupdate.exe [-disableContentIdentity] [-disableFieldIdentity] [-createTable] <XML FILE PATH> <CUSTOMER CODE>");
                Console.WriteLine("\t[-disableContentIdentity] отключить сохранение id контентов");
                Console.WriteLine("\t[-disableFieldIdentity]  отключить сохранение id полей");
                Console.WriteLine("\t[-createTable] создать таблицу XML_DB_UPDATE во время обновления");
                Console.WriteLine("\t<XML FILE/DIRECTORY PATH> путь к xml с действиями");
                Console.WriteLine("\t<CUSTOMER CODE> customer code, для которого требуется выполнять действия");
                Console.WriteLine("\t<CONFIG FILE> config файл, в котором указаны относительные пути к файлам xml c действиями и их настройки");

                Console.WriteLine("Для получения скрипта: ");
                Console.WriteLine("qp8dbupdate.exe -getScript");
                Environment.Exit(1);
            }

            var xmlpath = arguments[0];
            var customerCode = arguments[1];
            var configPath = arguments.Count == 3 ? arguments[2] : null;

            return new QpReplaySettings(disableContentIdentity, disableFieldIdentity, xmlpath, customerCode, configPath, skipLogging, createTable);
        }
    }
}
