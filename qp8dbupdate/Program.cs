using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using qp8dbupdate.Versioning;
using System.Diagnostics;
using Quantumart.QP8.Constants;

namespace qp8dbupdate
{
    class Program
    {
        static Program()
        {
            try
            {
                // MVC
                EmbeddedAssembly.Load("qp8dbupdate.References.System.Web.Helpers.dll", "System.Web.Helpers.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.System.Web.Razor.dll", "System.Web.Helpers.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.System.Web.WebPages.dll", "System.Web.WebPages.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.System.Web.WebPages.Razor.dll", "System.Web.WebPages.Razor.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.System.Web.WebPages.Deployment.dll", "System.Web.WebPages.Deployment.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.System.Web.Mvc.dll", "System.Web.Helpers.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Microsoft.Owin.dll", "Microsoft.Owin.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Microsoft.AspNet.SignalR.Core.dll", "Microsoft.AspNet.SignalR.Core.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Microsoft.Practices.EnterpriseLibrary.Logging.dll", "Microsoft.Practices.EnterpriseLibrary.Logging.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Newtonsoft.Json.dll", "Newtonsoft.Json.dll");

                // QP8
                EmbeddedAssembly.Load("qp8dbupdate.References.Microsoft.Practices.Unity.dll", "Microsoft.Practices.Unity.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Microsoft.Practices.Unity.Configuration.dll", "Microsoft.Practices.Unity.Configuration.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Microsoft.Practices.ServiceLocation.dll", "Microsoft.Practices.ServiceLocation.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Microsoft.Practices.Unity.Interception.dll", "Microsoft.Practices.Unity.Interception.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Microsoft.Practices.EnterpriseLibrary.Common.dll", "Microsoft.Practices.EnterpriseLibrary.Common.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Microsoft.Practices.EnterpriseLibrary.Validation.dll", "Microsoft.Practices.EnterpriseLibrary.Validation.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.dll", "Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.AutoMapper.dll", "AutoMapper.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.EFExtensions.dll", "EFExtensions.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Irony.dll", "Irony.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Moq.dll", "Moq.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.RazorGenerator.Mvc.dll", "RazorGenerator.Mvc.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Telerik.Web.Mvc.dll", "Telerik.Web.Mvc.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.WebActivatorEx.dll", "WebActivatorEx.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Quantumart.QP8.Assembling.dll", "Quantumart.QP8.Assembling.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Quantumart.dll", "Quantumart.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.QA.Configuration.dll", "QA.Configuration.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.QA.Validation.Xaml.dll", "QA.Validation.Xaml.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.QA.Validation.Xaml.Extensions.dll", "QA.Validation.Xaml.Extensions.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Quantumart.QP8.DAL.dll", "Quantumart.QP8.DAL.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Quantumart.QP8.Constants.dll", "Quantumart.QP8.Constants.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Quantumart.QP8.Resources.dll", "Quantumart.QP8.Resources.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Quantumart.QP8.Utils.dll", "Quantumart.QP8.Utils.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Quantumart.QP8.Validators.dll", "Quantumart.QP8.Validators.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Quantumart.QP8.Security.dll", "Quantumart.QP8.Security.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Quantumart.QP8.BLL.dll", "Quantumart.QP8.BLL.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Quantumart.QP8.Configuration.dll", "Quantumart.QP8.Configuration.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Quantumart.QP8.Logging.Loggers.dll", "Quantumart.QP8.Logging.Loggers.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Quantumart.QP8.Logging.Web.dll", "Quantumart.QP8.Logging.Web.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Quantumart.QP8.Logging.dll", "Quantumart.QP8.Logging.dll");
                EmbeddedAssembly.Load("qp8dbupdate.References.Quantumart.QP8.WebMvc.dll", "Quantumart.QP8.WebMvc.dll");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.QAErrorMessage());
            }

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler((obj, args) =>
            {
                return EmbeddedAssembly.Get(args.Name);
            });
        }

        static void Main(string[] args)
        {
            var arguments = args.ToList();

            bool disableContentIdentity = false,
                 disableFieldIdentity = false,
                 skipLogging = false,
                 createTable = false,
                 debug = false;

            try
            {
                // 1 - fingerprint
                // 2  - xml файл с обновлением
                // 3 - customer code
                

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
                    Console.WriteLine("Запуск произведен с возможностью отладки. Нажмите любою клавишу для продолжения...");
                    Console.ReadKey(true);

                    Debugger.Break();
                }

                Console.WriteLine("DB update for QP8 version 6.0");
                Console.WriteLine("Assembly version {0}", typeof(Program).Assembly.GetName().Version);
                Console.WriteLine(((char)64) + " Quantumart " + DateTime.Now.Year);
                Console.WriteLine("ContentIdentity: " + !disableContentIdentity);
                Console.WriteLine("FieldIdentity: " + !disableFieldIdentity);

                if (arguments.Count != 2)
                {
                    Console.WriteLine("Указаны не все параметры или их слишком много. Формат использования: ");
                    Console.WriteLine("qp8dbupdate.exe [-disableContentIdentity] [-disableFieldIdentity] [-createTable] <XML FILE PATH> <CUSTOMER CODE>");
                    Console.WriteLine("\t[-disableContentIdentity] отключить сохранение id контентов");
                    Console.WriteLine("\t[-disableFieldIdentity]  отключить сохранение id полей");
                    Console.WriteLine("\t[-createTable] создать таблицу XML_DB_UPDATE во время обновления");
                    Console.WriteLine("\t<XML FILE PATH> путь к xml с действиями");
                    Console.WriteLine("\t<CUSTOMER CODE> customer code, для которого требуется выполнять действия");

                    Console.WriteLine("Для получения скрипта: ");
                    Console.WriteLine("qp8dbupdate.exe -getScript");
                    Environment.Exit(1);
                    return;
                }

                string xmlpath = arguments[0];
                string customerCode = arguments[1];
                string xml = File.ReadAllText(xmlpath, UTF8Encoding.UTF8);

                Console.WriteLine("Processing {0} for customer code {1}", xmlpath, customerCode);

                var identityTypes = new HashSet<string>();

                if (!disableContentIdentity)
                    identityTypes.Add(EntityTypeCode.Content);

                if (!disableContentIdentity)
                    identityTypes.Add(EntityTypeCode.ContentGroup);

                if (!disableFieldIdentity)
                    identityTypes.Add(EntityTypeCode.Field);

                if (!disableContentIdentity)
                    identityTypes.Add(EntityTypeCode.ContentLink);

                using (QP8ReplayService service = new QP8ReplayService(customerCode, 1,
                     GetReplaySettings(disableContentIdentity, disableFieldIdentity), identityTypes))
                {

                    //string oldprint = service.ComputeFingerPrint();

                    ////if (!oldprint.Equals(fingerprint, StringComparison.InvariantCultureIgnoreCase))
                    ////{
                    ////    Console.WriteLine("Данное обновление невозможно применить, так как во ");
                    ////    return;
                    ////}
                    Console.WriteLine(xmlpath);

                    try
                    {
                        if (skipLogging)
                        {
                            service.ReplayXml(xml);
                        }
                        else
                        {
                            service.ReplayWithLogging(new DBUpdateLogEntry
                            {
                                Applied = DateTime.Now,
                                Body = xml,
                                FileName = Path.GetFileName(xmlpath)
                            }, createTable);
                        }
                    }
                    catch (DbUpdateException dbEx)
                    {
                        Console.WriteLine(Environment.NewLine);
                        Console.WriteLine("При обновлении произошла ошибка.");
                        Console.WriteLine(dbEx.Message);
                        Console.WriteLine("Обновление было произведено {0} пользователем #{1} с помощью файла {2}",
                            dbEx.Entry.Applied,
                            dbEx.Entry.UserId,
                            dbEx.Entry.FileName);

                        Environment.Exit(1);
                    }

                    Console.WriteLine("Структура базы была успешно обновлена");

                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                var filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LastException.txt");
                Console.WriteLine("При выполнении возникла ошибка: {0} \n\r {1}", ex.Message, ex.StackTrace);
                Console.WriteLine("Подробное описание ошибки в файле {0}", filename);

                try
                {
                    using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                    {
                        byte[] data = Encoding.UTF8.GetBytes(ex.QAErrorMessage());
                        fs.Write(data, 0, data.Length);
                    }
                }
                catch (Exception exc)
                {
                    Console.WriteLine("При записи файла с ошибкой возникла ошибка: {0} \n\r {1}", exc.Message, exc.StackTrace);
                }

                Environment.Exit(1);
            }
        }

        private static XDocument GetReplaySettings(bool disableContentIdentity,
            bool disableFieldIdentity)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<fingerprint>");

            if (!disableContentIdentity)
                sb.AppendLine(@"<entityType code=""content"" />");

            if (!disableFieldIdentity)
                sb.AppendLine(@"<entityType code=""field"" />");

            sb.AppendLine(@"</fingerprint>");

            return XDocument.Load(new StringReader(sb.ToString()));  // <entityType code=""content_link"" />	

        }
    }
}
