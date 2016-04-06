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

            int curWidth = GetValidValue(image.Width);
            int curHeight = GetValidValue(image.Height);
            int curQuality = GetValidValue(image.Quality);
            string path = (image.ImagePath + "\\" + image.ImageName).Replace("/", "\\");

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

                    string filePath = image.ContentLibraryPath + "\\" + GetDynamicImageRelPath(image.ImageName, image.AttrId, image.FileType);

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
            double factor = GetResizeFactor(img.Width, img.Height, width, height, fit);
            int resizeWidth = (int)factor == 0 ? width : Convert.ToInt32(img.Width * factor);
            int resizeHeight = (int)factor == 0 ? height : Convert.ToInt32(img.Height * factor);
            return Resizer.Resize(img, resizeWidth, resizeHeight); 
        }

        private static double GetResizeFactor(int width, int height, int targetWidth, int targetHeight, bool fit)
        {

            double heightFactor = Convert.ToDouble(targetHeight) / Convert.ToDouble(height);
            double widthFactor = Convert.ToDouble(targetWidth) / Convert.ToDouble(width);

            if (fit)
            {
                double result = heightFactor <= widthFactor ? heightFactor : widthFactor;
                result = result > 1 ? 1 : result;
                return result;
            }
            else
            {
                if ((int)heightFactor == 0)
                    return widthFactor;
                else if ((int)widthFactor == 0)
                    return heightFactor;
                else
                    return 0;
            }
        }

        #region Additional methods

        private int GetValidValue(object value)
        {
            int res = 0;
            if (value != null)
            {
                string strValue = Convert.ToString(value);

                if (strValue != "")
                {
                    res = int.Parse(strValue);
                }
            }

            return res;
        }

        public string GetDynamicImageRelPath(string fileName, decimal attributeId, string outFileType) 
        {
            string newName = fileName.Replace("/", "\\");
            string[] fileNameParts = newName.Split('.');
            fileNameParts[fileNameParts.Length - 1] = outFileType;
            return "field_" + attributeId + "\\" + String.Join(".", fileNameParts);
        }

        public static string GetDynamicImageRelUrl(string fileName, decimal attributeId, string outFileType)
        {
            string[] fileNameParts = fileName.Split('.');
            fileNameParts[fileNameParts.Length - 1] = outFileType;
            return "field_" + attributeId + "/" + String.Join(".", fileNameParts);

        }

        private void CreateFolderForFile(string filePath)
        {
            string[] fileDirectoryParts = filePath.Split('\\');
            fileDirectoryParts[fileDirectoryParts.Length - 1] = string.Empty;
            string fileDirectoryPath = String.Join("\\", fileDirectoryParts);

            if (!Directory.Exists(fileDirectoryPath))
            {
                Directory.CreateDirectory(fileDirectoryPath);
            }
        }

        #endregion

    }
}
