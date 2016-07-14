using System;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Web.SessionState;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
	[SessionState(SessionStateBehavior.Disabled)]
	public class ThumbnailController : QPController
	{				
		[HttpGet]
		public FileResult _SiteFileThumbnail(int folderId, string fileName)
		{
			return GetThumbnailResult(SiteFolderService.GetPath(folderId,fileName));		
		}		

		[HttpGet]
		public FileResult _ContentFileThumbnail(int folderId, string fileName)
		{
			return GetThumbnailResult(ContentFolderService.GetPath(folderId, fileName));		
		}
		
		/// <summary>
		/// Сформировать Thumbnail из файла и FileResult с картинкой
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private FileResult GetThumbnailResult(string path)
		{
			MemoryStream stream = new MemoryStream();
			try
			{
				using (var img = Image.FromFile(path))
				{
					if (img.Width <= 100 && img.Height <= 100)
						img.Save(stream, ImageFormat.Jpeg);
					else
					{
						double k = Convert.ToDouble(img.Width) / Convert.ToDouble(img.Height);
						int w = 0;
						int h = 0;
						if (img.Width >= img.Height)
						{
							w = 100;
							if (k == 0)
								h = 1;
							else
								h = Math.Min(Convert.ToInt32(w / k), 100);
						}
						else
						{
							h = 100;
							w = Math.Min(Convert.ToInt32(h * k), 100);
							if (w == 0)
								w = 1;
						}
						using (var resImg = new Bitmap(w, h))
						{

							using (Graphics oGraphic = Graphics.FromImage(resImg))
							{
								oGraphic.CompositingQuality = CompositingQuality.HighQuality;
								oGraphic.SmoothingMode = SmoothingMode.HighQuality;
								oGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;

								Rectangle oRectangle = new Rectangle(0, 0, w, h);
								oGraphic.DrawImage(img, oRectangle);
							}

							resImg.Save(stream, ImageFormat.Jpeg);
						}
					}
					stream.Seek(0, SeekOrigin.Begin);
					return File(stream, "image/jpeg");
				}
			}
			catch (OutOfMemoryException)
			{
				return null;
			}
		}
	}
}
