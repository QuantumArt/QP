using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{
    public class DynamicImage
    {
        public const string JPG_EXTENSION = "JPG";
        public const string PNG_EXTENSION = "PNG";
        public const string GIF_EXTENSION = "GIF";
        public const string SVG_EXTENSION = "SVG";

        public const short MinImageSize = 1;
        public const short MaxImageSize = 9999;
        public const short MinQuality = 50;
        public const short MaxQuality = 100;

        public static DynamicImage Load(Field field)
        {
            var image = FieldRepository.GetDynamicImageInfoById(field.Id);
            if (image != null)
            {
                image.Field = field;
            }
            return image;
        }

        public static DynamicImage Create(Field field) => new DynamicImage
        {
            Field = field,
            Height = 100,
            Width = 100,
            MaxSize = false,
            Quality = 90,
            Type = JPG_EXTENSION
        };

        #region private

        public enum ImageResizeMode
        {
            None,
            ByWidth,
            ByHeight,
            Absolute,
            Fit
        }

        public ImageResizeMode ResizeMode
        {
            get
            {
                if (Width != 0 && Height == 0)
                {
                    return ImageResizeMode.ByWidth;
                }

                if (Width == 0 && Height != 0)
                {
                    return ImageResizeMode.ByHeight;
                }

                if (Width != 0 && Height != 0)
                {
                    return MaxSize ? ImageResizeMode.Fit : ImageResizeMode.Absolute;
                }

                return ImageResizeMode.None;
            }
        }

        public string GetDesiredFileName(string baseFileName)
        {
            if (string.IsNullOrEmpty(baseFileName))
            {
                return string.Empty;
            }

            var fileNameParts = baseFileName.Split('.');
            if (!fileNameParts[fileNameParts.Length - 1].Equals(SVG_EXTENSION, StringComparison.InvariantCultureIgnoreCase))
            {
                fileNameParts[fileNameParts.Length - 1] = Type;
            }
            return string.Join(".", fileNameParts);
        }

        private Size GetDesiredImageSize(Size currentSize)
        {
            if (ResizeMode == ImageResizeMode.Absolute)
            {
                return new Size(Width, Height);
            }

            var widthCoef = Width / (double)currentSize.Width;
            var heightCoef = Height / (double)currentSize.Height;
            double targetCoef;
            if (ResizeMode == ImageResizeMode.ByWidth)
            {
                targetCoef = widthCoef;
            }
            else if (ResizeMode == ImageResizeMode.ByHeight)
            {
                targetCoef = heightCoef;
            }
            else if (ResizeMode == ImageResizeMode.Fit)
            {
                targetCoef = heightCoef >= 1 && widthCoef >= 1 ? 1 : Math.Min(heightCoef, widthCoef);
            }
            else
            {
                throw new Exception("Incorrect resize mode");
            }

            return new Size((int)(currentSize.Width * targetCoef), (int)(currentSize.Height * targetCoef));
        }

        private string MimeType
        {
            get
            {
                if (Type == "JPG")
                {
                    return "image/jpeg";
                }

                if (Type == "GIF")
                {
                    return "image/gif";
                }

                if (Type == "PNG")
                {
                    return "image/png";
                }

                return string.Empty;
            }
        }

        private ImageCodecInfo ImageCodecInfo
        {
            get { return ImageCodecInfo.GetImageEncoders().Where(n => n.MimeType == MimeType).SingleOrDefault(); }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private EncoderParameters EncoderParameters
        {
            get
            {
                if (Type == "JPG")
                {
                    var parameters = new EncoderParameters(1);
                    parameters.Param[0] = new EncoderParameter(Encoder.Quality, (int)Quality);
                    return parameters;
                }

                return null;
            }
        }

        public PathInfo PathInfo => Field.PathInfo.GetSubPathInfo(SubFolder);

        private string SubFolder => string.Format("{0}{1}", Field.Prefix, Id);

        #endregion

        public Field Field { get; set; }

        public int Id { get; set; }

        [LocalizedDisplayName("DynamicImageHeigth", NameResourceType = typeof(FieldStrings))]
        public short Height { get; set; }

        [LocalizedDisplayName("DynamicImageWidth", NameResourceType = typeof(FieldStrings))]
        public short Width { get; set; }

        public bool MaxSize { get; set; }

        [LocalizedDisplayName("DynamicImageQuality", NameResourceType = typeof(FieldStrings))]
        public short? Quality { get; set; }

        [LocalizedDisplayName("DynamicImageType", NameResourceType = typeof(FieldStrings))]
        public string Type { get; set; }

        public bool IsNew => Id == default(int);

        public string GetValue(string baseFileName)
        {
            if (string.IsNullOrEmpty(baseFileName))
            {
                return string.Empty;
            }

            return string.Format("{0}/{1}", SubFolder, GetDesiredFileName(baseFileName));
        }

        public void CreateDynamicImage(string baseImagePath, string imageValue)
        {
            if (File.Exists(baseImagePath))
            {
                if (!imageValue.ToUpper().EndsWith(SVG_EXTENSION))
                {
                    using (var input = new Bitmap(baseImagePath))
                    {
                        var desiredSize = GetDesiredImageSize(input.Size);
                        using (var output = new Bitmap(desiredSize.Width, desiredSize.Height))
                        {
                            var resizer = Graphics.FromImage(output);
                            resizer.InterpolationMode = output.Width < input.Width && output.Height < input.Height ? InterpolationMode.HighQualityBicubic : InterpolationMode.HighQualityBilinear;
                            resizer.DrawImage(input, 0, 0, desiredSize.Width, desiredSize.Height);
                            var resultPath = Path.Combine(PathInfo.Path, GetDesiredFileName(imageValue).Replace(@"/", @"\"));
                            Directory.CreateDirectory(Path.GetDirectoryName(resultPath));
                            using (var fs = File.OpenWrite(resultPath))
                            {
                                output.Save(fs, ImageCodecInfo, EncoderParameters);
                            }
                        }
                    }
                }
                else
                {
                    var xmlDocument = new XmlDocument();
                    xmlDocument.Load(baseImagePath);
                    var documentElement = xmlDocument.DocumentElement;
                    var width = 0;
                    var height = 0;
                    var widthAttr = documentElement.Attributes.GetNamedItem("width");
                    if (widthAttr != null)
                    {
                        width = int.Parse(Regex.Match(widthAttr.Value, "\\d+").Value);
                    }

                    var heightAttr = documentElement.Attributes.GetNamedItem("height");
                    if (heightAttr != null)
                    {
                        height = int.Parse(Regex.Match(heightAttr.Value, "\\d+").Value);
                    }

                    if (ResizeMode == ImageResizeMode.Fit)
                    {
                        documentElement.SetAttribute("preserveAspectRatio", "none");
                    }
                    else
                    {
                        documentElement.SetAttribute("preserveAspectRatio", "XMinYMin meet");
                    }

                    var desiredImageSize = GetDesiredImageSize(new Size(width, height));
                    widthAttr.Value = desiredImageSize.Width.ToString();
                    heightAttr.Value = desiredImageSize.Height.ToString();

                    var filename = Path.Combine(PathInfo.Path, GetDesiredFileName(imageValue).Replace("/", "\\"));
                    xmlDocument.Save(filename);
                }
            }
        }

        public void DeleteDirectory()
        {
            if (!IsNew)
            {
                Folder.ForceDelete(PathInfo.Path);
            }
        }

        public DynamicImage Clone(Field field)
        {
            var result = (DynamicImage)MemberwiseClone();
            result.Id = 0;
            result.Field = field;
            return result;
        }
    }
}
