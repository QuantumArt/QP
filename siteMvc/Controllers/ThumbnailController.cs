using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;
using System.Web.SessionState;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    [SessionState(SessionStateBehavior.Disabled)]
    public class ThumbnailController : QPController
    {
        [HttpGet]
        public FileResult _SiteFileThumbnail(int folderId, string fileName) => GetThumbnailResult(SiteFolderService.GetPath(folderId, fileName));

        [HttpGet]
        public FileResult _ContentFileThumbnail(int folderId, string fileName) => GetThumbnailResult(ContentFolderService.GetPath(folderId, fileName));

        private FileResult GetThumbnailResult(string path)
        {
            var stream = new MemoryStream();
            try
            {
                using (var img = Image.FromFile(path))
                {
                    if (img.Width <= 100 && img.Height <= 100)
                    {
                        img.Save(stream, ImageFormat.Jpeg);
                    }
                    else
                    {
                        var k = Convert.ToDouble(img.Width) / Convert.ToDouble(img.Height);
                        int w, h;
                        if (img.Width >= img.Height)
                        {
                            w = 100;

                            // ReSharper disable once CompareOfFloatsByEqualityOperator
                            h = k == 0 ? 1 : Math.Min(Convert.ToInt32(w / k), 100);
                        }
                        else
                        {
                            h = 100;
                            w = Math.Min(Convert.ToInt32(h * k), 100);
                            if (w == 0)
                            {
                                w = 1;
                            }
                        }

                        using (var resImg = new Bitmap(w, h))
                        {
                            using (var graphic = Graphics.FromImage(resImg))
                            {
                                graphic.CompositingQuality = CompositingQuality.HighQuality;
                                graphic.SmoothingMode = SmoothingMode.HighQuality;
                                graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;

                                var rectangle = new Rectangle(0, 0, w, h);
                                graphic.DrawImage(img, rectangle);
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
