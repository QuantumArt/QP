using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Validators;
using Quantumart.QP8.Resources;


namespace Quantumart.QP8.BLL
{
	public class DynamicImage
	{

		public const string JPG_EXTENSION = "JPG";
		public const string PNG_EXTENSION = "PNG";
		public const string GIF_EXTENSION = "GIF";

        public const short MinImageSize = 1;
        public const short MaxImageSize = 9999;
        public const short MinQuality = 50;
        public const short MaxQuality = 100;


		public static DynamicImage Load(Field field)
		{
			DynamicImage image = FieldRepository.GetDynamicImageInfoById(field.Id);
			if (image != null)
				image.Field = field;
			return image;
		}

		public static DynamicImage Create(Field field)
		{			
			return new DynamicImage
			{					
				Field = field,
				Height = 100,
				Width = 100,
				MaxSize = false,
				Quality = 90,
				Type = JPG_EXTENSION
			};		
		}
		
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
					return ImageResizeMode.ByWidth;
				else if (Width == 0 && Height != 0)
					return ImageResizeMode.ByHeight;
				else if (Width != 0 && Height != 0)
					return (MaxSize) ? ImageResizeMode.Fit : ImageResizeMode.Absolute;
				else
					return ImageResizeMode.None;
			}
		}

		public string GetDesiredFileName(string baseFileName)
		{
			if (String.IsNullOrEmpty(baseFileName))
				return String.Empty;

			string[] fileNameParts = baseFileName.Split('.');
            fileNameParts[fileNameParts.Length - 1] = Type;
            return String.Join(".", fileNameParts);
		}

		private Size GetDesiredImageSize(Size currentSize)
		{
			if (ResizeMode == ImageResizeMode.Absolute)
			{
				return new Size(Width, Height);
			}
			else
			{
				double widthCoef = (double)Width / (double)currentSize.Width;
				double heightCoef = (double)Height / (double)currentSize.Height;
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
					targetCoef = (heightCoef >= 1 && widthCoef >= 1) ? 1 : Math.Min(heightCoef, widthCoef);
				}
				else
					throw new Exception("Incorrect resize mode");

				return new Size((int)(currentSize.Width * targetCoef), (int)(currentSize.Height * targetCoef));
			}
		}

		private string MimeType
		{
			get
			{
				if (Type == "JPG")
					return "image/jpeg";
				else if (Type == "GIF")
					return "image/gif";
				else if (Type == "PNG")
					return "image/png";
				else
					return String.Empty;
			}
		}

		private ImageCodecInfo ImageCodecInfo
		{
			get
			{
				return ImageCodecInfo.GetImageEncoders().Where(n => n.MimeType == MimeType).SingleOrDefault();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private EncoderParameters EncoderParameters
		{
			get
			{
				if (Type == "JPG")
				{
					EncoderParameters parameters = new EncoderParameters(1);
					parameters.Param[0] = new EncoderParameter(Encoder.Quality, (int)Quality);
					return parameters;
				}
				else
					return null;
			}
		}

		public PathInfo PathInfo
		{
			get
			{
				return Field.PathInfo.GetSubPathInfo(SubFolder);
			}
		}


		private string SubFolder
		{
			get
			{
				return String.Format("{0}{1}", Field.Prefix, Id);
			}
		}


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

		public bool IsNew { get { return Id == default(int); } }

		public string GetValue(string baseFileName)
		{
			if (String.IsNullOrEmpty(baseFileName))
				return String.Empty;
			return String.Format("{0}/{1}", SubFolder, GetDesiredFileName(baseFileName));

		}

		public void CreateDynamicImage(string baseImagePath, string imageValue)
		{
			if (File.Exists(baseImagePath))
			{
				using (Bitmap input = new Bitmap(baseImagePath))
				{
					Size desiredSize = GetDesiredImageSize(input.Size);
					using (Bitmap output = new Bitmap(desiredSize.Width, desiredSize.Height))
					{
						Graphics resizer = Graphics.FromImage(output);
						resizer.InterpolationMode = (output.Width < input.Width && output.Height < input.Height) ? InterpolationMode.HighQualityBicubic : InterpolationMode.HighQualityBilinear;
						resizer.DrawImage(input, 0, 0, desiredSize.Width, desiredSize.Height);
						string resultPath = Path.Combine(PathInfo.Path, GetDesiredFileName(imageValue).Replace(@"/", @"\"));
						Directory.CreateDirectory(Path.GetDirectoryName(resultPath));
						using (FileStream fs = File.OpenWrite(resultPath))
						{
							output.Save(fs, ImageCodecInfo, EncoderParameters);
						}
					}
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
			DynamicImage result = (DynamicImage)this.MemberwiseClone();
			result.Id = 0;
			result.Field = field;
			return result;
		}
	}
}
