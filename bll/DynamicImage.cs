using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.Resources;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Quantumart.QP8.BLL
{
    public class DynamicImage
    {
        public const string JPG_EXTENSION = "JPG";
        public const string PNG_EXTENSION = "PNG";
        public const string GIF_EXTENSION = "GIF";
        public const string SVG_EXTENSION = "SVG";
        public const string WEBP_EXTENSION = "WEBP";

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

                if (Type == "WEBP")
                {
                    return "image/webp";
                }

                return string.Empty;
            }
        }

        public IImageEncoder Encoder
        {
            get
            {
                if (Type == "JPG")
                {
                    return new JpegEncoder() { Quality = Quality };
                }

                if (Type == "GIF")
                {
                    return new GifEncoder();
                }

                if (Type == "PNG")
                {
                    return new PngEncoder();
                }

                if (Type == "WEBP")
                {
                    return new WebpEncoder();
                }

                return null;
            }
        }

        public PathInfo PathInfo => Field.PathInfo.GetSubPathInfo(SubFolder);

        private string SubFolder => string.Format("{0}{1}", Field.Prefix, Id);

        #endregion

        [Required]
        public Field Field { get; set; }

        public int Id { get; set; }

        [Display(Name = "DynamicImageHeigth", ResourceType = typeof(FieldStrings))]
        public short Height { get; set; }

        [Display(Name = "DynamicImageWidth", ResourceType = typeof(FieldStrings))]
        public short Width { get; set; }

        public bool MaxSize { get; set; }

        [Display(Name = "DynamicImageQuality", ResourceType = typeof(FieldStrings))]
        public short? Quality { get; set; }

        [Display(Name = "DynamicImageType", ResourceType = typeof(FieldStrings))]
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
                var desiredFileName = GetDesiredFileName(imageValue);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    desiredFileName = desiredFileName.Replace(@"/", @"\");
                }
                if (!imageValue.ToUpper().EndsWith(SVG_EXTENSION))
                {
                    using (var image = Image.Load(baseImagePath))
                    {
                        var desiredSize = GetDesiredImageSize(new Size(image.Width, image.Height));
                        image.Mutate(x => x.Resize(desiredSize.Width, desiredSize.Height));
                        var resultPath = Path.Combine(PathInfo.Path, desiredFileName);
                        Directory.CreateDirectory(Path.GetDirectoryName(resultPath));
                        using (var fs = File.OpenWrite(resultPath))
                        {
                            image.Save(fs, Encoder);
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

                    var filename = Path.Combine(PathInfo.Path, desiredFileName);
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
