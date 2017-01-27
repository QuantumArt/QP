using System;
using System.Drawing;
using System.IO;
using Quantumart.QPublishing.Info;

namespace Quantumart.QPublishing.Resizer
{
    public class DynamicImage : IDynamicImage
    {
        public void CreateDynamicImage(DynamicImageInfo image)
        {
            var curWidth = GetValidValue(image.Width);
            var curHeight = GetValidValue(image.Height);
            var curQuality = GetValidValue(image.Quality);
            var path = (image.ImagePath + "\\" + image.ImageName).Replace("/", "\\");
            if (File.Exists(path))
            {
                Bitmap img = null;
                Bitmap img2 = null;
                try
                {
                    img = new Bitmap(path);
                    img2 = curWidth != 0 || curHeight != 0
                        ? ResizeImage(img, curWidth, curHeight, image.MaxSize)
                        : new Bitmap(path);

                    var filePath = image.ContentLibraryPath + "\\" + GetDynamicImageRelPath(image.ImageName, image.AttrId, image.FileType);
                    CreateFolderForFile(filePath);
                    Resizer.SaveImage(img2, image.FileType, filePath, curQuality);
                }
                finally
                {
                    img?.Dispose();
                    img2?.Dispose();
                }
            }
        }

        public Bitmap ResizeImage(Bitmap img, int width, int height, bool fit)
        {
            var factor = GetResizeFactor(img.Width, img.Height, width, height, fit);
            var resizeWidth = (int)factor == 0 ? width : Convert.ToInt32(img.Width * factor);
            var resizeHeight = (int)factor == 0 ? height : Convert.ToInt32(img.Height * factor);
            return Resizer.Resize(img, resizeWidth, resizeHeight);
        }

        private static double GetResizeFactor(int width, int height, int targetWidth, int targetHeight, bool fit)
        {
            var heightFactor = Convert.ToDouble(targetHeight) / Convert.ToDouble(height);
            var widthFactor = Convert.ToDouble(targetWidth) / Convert.ToDouble(width);
            if (fit)
            {
                var result = heightFactor <= widthFactor ? heightFactor : widthFactor;
                result = result > 1 ? 1 : result;
                return result;
            }

            if ((int)heightFactor == 0)
            {
                return widthFactor;
            }

            return (int)widthFactor == 0 ? heightFactor : 0;
        }

        private static int GetValidValue(object value)
        {
            var res = 0;
            if (value != null)
            {
                var strValue = Convert.ToString(value);
                if (strValue != string.Empty)
                {
                    res = int.Parse(strValue);
                }
            }

            return res;
        }

        public string GetDynamicImageRelPath(string fileName, decimal attributeId, string outFileType)
        {
            var newName = fileName.Replace("/", "\\");
            var fileNameParts = newName.Split('.');
            fileNameParts[fileNameParts.Length - 1] = outFileType;
            return "field_" + attributeId + "\\" + string.Join(".", fileNameParts);
        }

        public static string GetDynamicImageRelUrl(string fileName, decimal attributeId, string outFileType)
        {
            var fileNameParts = fileName.Split('.');
            fileNameParts[fileNameParts.Length - 1] = outFileType;
            return "field_" + attributeId + "/" + string.Join(".", fileNameParts);
        }

        private static void CreateFolderForFile(string filePath)
        {
            var fileDirectoryParts = filePath.Split('\\');
            fileDirectoryParts[fileDirectoryParts.Length - 1] = string.Empty;
            var fileDirectoryPath = string.Join("\\", fileDirectoryParts);
            if (!Directory.Exists(fileDirectoryPath))
            {
                Directory.CreateDirectory(fileDirectoryPath);
            }
        }
    }
}
