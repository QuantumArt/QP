using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Quantumart.QPublishing.Resizer
{
    public class Resizer
    {
        public static Bitmap Resize(Bitmap img, int width, int height)
        {
            var output = new Bitmap(width, height);
            var resizer = Graphics.FromImage(output);
            resizer.InterpolationMode = InterpolationMode.HighQualityBilinear;
            resizer.DrawImage(img, 0, 0, width, height);
            return output;
        }

        public static void SaveImage(Bitmap img, string fileType, string filePath, int quality)
        {
            ImageFormat convertFormat;
            switch (fileType)
            {
                case "JPG":

                    var parameters = new EncoderParameters(1);
                    var parameter = new EncoderParameter(Encoder.Quality, quality);
                    parameters.Param[0] = parameter;

                    var imageCodecInfo = GetEncoderInfo("image/jpeg");

                    using (var fs = File.OpenWrite(filePath))
                    {
                        img.Save(fs, imageCodecInfo, parameters);
                    }

                    break;
                case "GIF":
                    convertFormat = ImageFormat.Gif;
                    using (var fs = File.OpenWrite(filePath))
                    {
                        img.Save(fs, convertFormat);
                    }

                    break;
                case "PNG":
                    convertFormat = ImageFormat.Png;
                    using (var fs = File.OpenWrite(filePath))
                    {
                        img.Save(fs, convertFormat);
                    }
                    break;
            }
        }

        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            int j;
            var encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                {
                    return encoders[j];
                }
            }

            return null;
        }
    }
}
