using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ThumbnailController : AuthQpController
    {
        private readonly PathHelper _pathHelper;

        public ThumbnailController(PathHelper pathHelper)
        {
            _pathHelper = pathHelper;
        }

        public FileResult _SiteFileThumbnail(int folderId, string fileName)
        {
            return GetThumbnailResult(SiteFolderService.GetPath(folderId, fileName));
        }

        public FileResult _ContentFileThumbnail(int folderId, string fileName)
        {
            return GetThumbnailResult(ContentFolderService.GetPath(folderId, fileName));
        }

        private FileResult GetThumbnailResult(string path)
        {
            var stream = new MemoryStream();
            using var img = _pathHelper.LoadImage(path);
            if (img.Width <= 100 && img.Height <= 100)
            {
                img.SaveAsJpeg(stream);
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

                img.Mutate(ctx => ctx.Resize(w, h));
                img.SaveAsJpeg(stream);
            }

            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, "image/jpeg");
        }
    }
}
