using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Xml;
using Quantumart.QPublishing.Database;

namespace Quantumart.QPublishing.Helpers
{
    public class Dump
    {
        private static readonly object DataLocker = new object();
        private static readonly object TextLocker = new object();
        private static readonly object HashLocker = new object();
        private static string _tempDirectory;
        private static readonly string FileName = $@"{TempDirectory}\text.txt";

        public static string TempDirectory
        {
            get
            {
                if (_tempDirectory == null)
                {
                    try
                    {
                        _tempDirectory = DBConnector.GetQpTempDirectory();
                    }
                    catch
                    {
                        _tempDirectory = @"c:\temp";
                    }
                }
                return _tempDirectory;
            }
        }

        public static void DumpHashTable(Hashtable ht, string inKey)
        {
            string fileName = $@"{TempDirectory}\ht{DateTime.Now.Ticks}.txt";
            if (Directory.Exists(TempDirectory))
            {
                lock (HashLocker)
                {
                    var sw = File.AppendText(fileName);

                    sw.WriteLine($"key: {inKey}");
                    foreach (string key in ht.Keys)
                    {
                        sw.WriteLine($"{key} - {ht[key]}");
                    }

                    sw.Close();
                }
            }

        }

        public static void DumpDataTable(DataTable dt, string name)
        {
            string fileName = $@"{TempDirectory}\db{DateTime.Now.Ticks}.txt";
            if (Directory.Exists(TempDirectory) && !File.Exists(fileName))
            {
                lock (DataLocker)
                {
                    var settings = new XmlWriterSettings
                    {
                        Indent = true,
                        IndentChars = " "
                    };
                    using (var writer = XmlWriter.Create(fileName, settings))
                    {
                        if (dt != null)
                        {
                            // Write XML data.
                            dt.TableName = name;
                            dt.WriteXml(writer);
                        }
                        writer.Close();
                    }
                }
            }

        }

        public static void DumpDataTable(DataTable dt)
        {
            DumpDataTable(dt, "DefaultName");
        }

        public static void DumpStr(string str)
        {
            if (Directory.Exists(TempDirectory))
            {
                lock (TextLocker)
                {
                    var sw = File.AppendText(FileName);
                    sw.WriteLine(str);
                    sw.Close();
                }
            }
        }
    }
}
