using System;
using System.Collections.Generic;
using System.IO;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.WebMvc.ViewModels.Library
{
    /// <summary>
    /// View Model элемента для списков файлов
    /// </summary>
    public class FileListItem
    {
        private static readonly Dictionary<FolderFileType, string> FileTypeSmallIconDictionary = new Dictionary<FolderFileType, string>
        {
            { FolderFileType.CSS, "css_16.png" },
            { FolderFileType.Flash, "flash_16.png" },
            { FolderFileType.Image, "image_16.png" },
            { FolderFileType.Javascript, "javascript_16.png" },
            { FolderFileType.Media, "media_16.png" },
            { FolderFileType.Office, "office_16.png" },
            { FolderFileType.PDF, "pdf_16.png" },
            { FolderFileType.Unknown, "unknown_16.png" }
        };

        private static readonly Dictionary<FolderFileType, string> FileTypeBigIconDictionary = new Dictionary<FolderFileType, string>
        {
            { FolderFileType.CSS, "css_64.png" },
            { FolderFileType.Flash, "flash_64.png" },
            { FolderFileType.Image, "image_64.png" },
            { FolderFileType.Javascript, "javascript_64.png" },
            { FolderFileType.Media, "media_64.png" },
            { FolderFileType.Office, "office_64.png" },
            { FolderFileType.PDF, "pdf_64.png" },
            { FolderFileType.Unknown, "unknown_64.png" }
        };

        public static string GetDimensions(FolderFile file, PathHelper pathHelper)
        {
            var dimensions = "";
            try
            {
                var image = pathHelper.IdentifyImage(file.FullName);
                dimensions = $"{image.Width}x{image.Height}";
            }
            catch (Exception)
            {
                // ignored
            }

            return dimensions;
        }

        public static FileListItem Create(FolderFile file, int fileShortNameLength, PathHelper pathHelper, bool loadDimensions)
        {
            var dimensions = loadDimensions ? GetDimensions(file, pathHelper) : String.Empty;
            var item = new FileListItem
            {
                FullName = file.Name,
                Name = string.Concat(Typographer.CutShort(Path.GetFileNameWithoutExtension(file.Name), fileShortNameLength), Path.GetExtension(file.Name)),
                Size = file.Size,
                Dimensions = dimensions,
                Modified = file.Modified.ToLongTimeString(),
                FileType = file.FileType
            };

            return item;
        }

        /// <summary>
        /// Укороченное имя файла с расширением
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Полное имя файла с расширением
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Размер файла
        /// </summary>
        public string Size { get; set; }

        /// <summary>
        /// Разрешение изображения
        /// </summary>
        public string Dimensions { get; set; }

        /// <summary>
        /// Дата создания файла
        /// </summary>
        public string Created { get; set; }

        /// <summary>
        /// Дата модификации файла
        /// </summary>
        public string Modified { get; set; }

        /// <summary>
        /// Тип файла
        /// </summary>
        public FolderFileType FileType { get; set; }

        /// <summary>
        /// Ссылка на картинку с маленькой иконкой
        /// </summary>
        public string SmallIconLink => FileTypeSmallIconDictionary[FileType];

        /// <summary>
        /// Ссылка на картинку для preview
        /// </summary>
        public string BigIconLink => FileTypeBigIconDictionary[FileType];
    }
}
