using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{
    public class FolderFile
    {
        #region creation

        static FolderFile()
        {
            Func<FolderFileType, string> createFileExtension = type => string.Join(";", fileTypeDictionary.Where(i => i.Value == type).Select(i => i.Key));
            fileExtensionsDictionary = new Dictionary<FolderFileType, string>
            {
                { FolderFileType.Unknown, "" },
                { FolderFileType.Image, createFileExtension(FolderFileType.Image) },
                { FolderFileType.CSS, createFileExtension(FolderFileType.CSS) },
                { FolderFileType.Javascript, createFileExtension(FolderFileType.Javascript) },
                { FolderFileType.Flash, createFileExtension(FolderFileType.Flash) },
                { FolderFileType.Media, createFileExtension(FolderFileType.Media) },
                { FolderFileType.PDF, createFileExtension(FolderFileType.PDF) },
                { FolderFileType.Office, createFileExtension(FolderFileType.Office) }
            };
        }

        public FolderFile()
        {
        }

        public FolderFile(FileInfo info)
        {
            OldName = info.Name;
            Name = info.Name;
            Path = info.FullName.Replace(info.Name, string.Empty);
            Extension = info.Extension;
            Created = info.CreationTime;
            Modified = info.LastWriteTime;
            Length = info.Length;
        }

        #endregion

        #region private

        private string _Dimensions;

        /// <summary>
        /// Словарь типов файлов по расширениям
        /// </summary>
        private static readonly Dictionary<string, FolderFileType> fileTypeDictionary = new Dictionary<string, FolderFileType>(StringComparer.OrdinalIgnoreCase)
        {
            #region словарь

            { ".gif", FolderFileType.Image },
            { ".jpg", FolderFileType.Image },
            { ".jpeg", FolderFileType.Image },
            { ".png", FolderFileType.Image },
            { ".bmp", FolderFileType.Image },
            { ".svg", FolderFileType.Image },
            { ".webp", FolderFileType.Image},

            { ".css", FolderFileType.CSS },

            { ".js", FolderFileType.Javascript },

            { ".flv", FolderFileType.Flash },
            { ".swf", FolderFileType.Flash },

            { ".avi", FolderFileType.Media },
            { ".mid", FolderFileType.Media },
            { ".mov", FolderFileType.Media },
            { ".mp3", FolderFileType.Media },
            { ".mp4", FolderFileType.Media },
            { ".mpc", FolderFileType.Media },
            { ".mpeg", FolderFileType.Media },
            { ".mpg", FolderFileType.Media },
            { ".ram", FolderFileType.Media },
            { ".rm", FolderFileType.Media },
            { ".rmi", FolderFileType.Media },
            { ".rmvb", FolderFileType.Media },
            { ".wav", FolderFileType.Media },
            { ".wma", FolderFileType.Media },
            { ".wmv", FolderFileType.Media },

            { ".pdf", FolderFileType.PDF },

            { ".doc", FolderFileType.Office },
            { ".rtf", FolderFileType.Office },
            { ".docx", FolderFileType.Office },
            { ".xlsx", FolderFileType.Office },
            { ".xls", FolderFileType.Office },
            { ".ppt", FolderFileType.Office },
            { ".pptx", FolderFileType.Office },

            #endregion
        };

        /// <summary>
        /// Словарь имен типов файлов по типу
        /// </summary>
        private static readonly Dictionary<FolderFileType, string> fileTypeNameDictionary = new Dictionary<FolderFileType, string>
        {
            #region словарь

            { FolderFileType.Unknown, LibraryStrings.Unknown },
            { FolderFileType.Image, LibraryStrings.Image },
            { FolderFileType.CSS, LibraryStrings.CSS },
            { FolderFileType.Javascript, LibraryStrings.Javascript },
            { FolderFileType.Flash, LibraryStrings.Flash },
            { FolderFileType.Media, LibraryStrings.Media },
            { FolderFileType.PDF, LibraryStrings.PDF },
            { FolderFileType.Office, LibraryStrings.Office }

            #endregion
        };

        private static readonly Dictionary<FolderFileType, string> fileExtensionsDictionary;

        #endregion

        /// <summary>
        /// исходное имя файла с расширением
        /// </summary>
        internal string OldName { get; set; }

        /// <summary>
        /// имя файла с расширением
        /// </summary>
        [LocalizedDisplayName("FileName", NameResourceType = typeof(LibraryStrings))]
        public string Name { get; set; }

        /// <summary>
        /// расширение файла (с точкой)
        /// </summary>
        [LocalizedDisplayName("Extension", NameResourceType = typeof(LibraryStrings))]
        public string Extension { get; set; }

        /// <summary>
        /// путь к файлу с завершающим слэшом
        /// </summary>
        internal string Path { get; set; }

        /// <summary>
        /// размер файла (в байтах)
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// Размеры изображения (только для картинок)
        /// </summary>
        [LocalizedDisplayName("Dimensions", NameResourceType = typeof(LibraryStrings))]
        public string Dimensions
        {
            get
            {
                if (_Dimensions == null)
                {
                    _Dimensions = string.Empty;
                    if (FileType == FolderFileType.Image)
                    {
                        try
                        {
                            using (var image = Image.FromFile(FullName))
                            {
                                _Dimensions = string.Format("{0}x{1}", image.Width, image.Height);
                            }
                        }
                        catch (OutOfMemoryException) // sic!!!
                        {
                            _Dimensions = string.Empty;
                        }
                    }
                }
                return _Dimensions;
            }
        }

        /// <summary>
        /// Дата создания файла
        /// </summary>
        [LocalizedDisplayName("Created", NameResourceType = typeof(EntityObjectStrings))]
        public DateTime Created { get; set; }

        /// <summary>
        /// Дата модификации файла
        /// </summary>
        [LocalizedDisplayName("Modified", NameResourceType = typeof(EntityObjectStrings))]
        public DateTime Modified { get; set; }

        /// <summary>
        /// Тип файла
        /// </summary>
        public FolderFileType FileType
        {
            get
            {
                if (!fileTypeDictionary.ContainsKey(Extension))
                {
                    return FolderFileType.Unknown;
                }

                return fileTypeDictionary[Extension];
            }
        }

        /// <summary>
        /// Имя типа файла
        /// </summary>
        [LocalizedDisplayName("FileType", NameResourceType = typeof(LibraryStrings))]
        public string FileTypeName => GetTypeName(FileType);

        /// <summary>
        /// Возвращает имя типа файла по типу
        /// </summary>
        /// <param name="type">тип файла</param>
        /// <returns>имя типа</returns>
        public static string GetTypeName(FolderFileType type) => fileTypeNameDictionary[type];

        /// <summary>
        /// Возвращает строку для фильтра по расширениям для конкретного типа
        /// </summary>
        /// <returns></returns>
        public static string GetTypeExtensions(FolderFileType type) => fileExtensionsDictionary[type];

        [LocalizedDisplayName("Size", NameResourceType = typeof(LibraryStrings))]
        public string Size
        {
            get
            {
                var m = 1024;
                var kb = m;
                var mb = m * m;
                var gb = m * m * m;
                var formatString = "{0:f2} {1}";
                if (Length > gb)
                {
                    return string.Format(formatString, Length / gb, LibraryStrings.GB);
                }

                if (Length > mb)
                {
                    return string.Format(formatString, Length / mb, LibraryStrings.MB);
                }

                if (Length > kb)
                {
                    return string.Format(formatString, Length / kb, LibraryStrings.KB);
                }

                return string.Format("{0} {1}", Length, LibraryStrings.Bytes);
            }
        }

        internal bool NameChanged => OldName != Name;

        internal string FullName => Path + Name;

        internal string OldFullName => Path + OldName;

        public void Validate()
        {
            var errors = new RulesException<FolderFile>();

            if (NameChanged && File.Exists(FullName))
            {
                errors.ErrorFor(s => s.Name, string.Format(LibraryStrings.FileExists, FullName));
            }

            if (!errors.IsEmpty)
            {
                throw errors;
            }
        }

        internal void Rename()
        {
            if (NameChanged)
            {
                File.SetAttributes(OldFullName, FileAttributes.Normal);
                File.Move(OldFullName, FullName);
            }
        }

        internal void Remove()
        {
            if (File.Exists(FullName))
            {
                File.SetAttributes(FullName, FileAttributes.Normal);
                File.Delete(FullName);
            }
        }
    }
}
