using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.IO;
using System.Drawing;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class LibraryController : QPController
	{
		#region Constants
		private string ContentDispositionKey = "Content-Disposition";
		private string ContentDispositionTemplate = "attachment; filename=\"{0}\"; filename*=UTF-8''{0}";
		#endregion

		#region private

		public class FilePropertiesOptions
		{
			public bool IsVersion { get; set; }
			public string NotSupportedMessage { get; set; }

			public FilePropertiesOptions()
			{
				IsVersion = false;
				NotSupportedMessage = LibraryStrings.OperationIsNotSupported;
			}
		}
		
		
		private JsonResult GetTestFileDownloadResult(PathInfo info, string fileName, bool isVersion)
		{
			string normilizedFileName = isVersion ? Path.GetFileName(fileName) : fileName;
			string path = info.GetPath(normilizedFileName);
			if (System.IO.File.Exists(path))
			{
				return Json(new { proceed = true, key = SavePath(path) }, JsonRequestBehavior.AllowGet);
			}
			else
			{
				return Json(new { proceed = false, msg = String.Format(LibraryStrings.NotExists, normilizedFileName) }, JsonRequestBehavior.AllowGet);
			}
		}

		private string SavePath(string path)
		{
			Random rnd = new Random();
			string key = rnd.Next().ToString();
			TempData[key] = path;
			return key;
		}

		private static PathInfo GetFilePathInfo(int fieldId, int? entityId, bool isVersion)
		{
			if (!isVersion)
			{
				return FieldService.GetPathInfo(fieldId, entityId);
			}
			else
			{
				return ArticleVersionService.GetPathInfo(fieldId, (int)entityId);
			}
		}

		private JsonResult GetFileProperties(PathInfo pathInfo, string fileName, FilePropertiesOptions options)
		{
			string normalizedFileName = (options.IsVersion) ? Path.GetFileName(fileName) : fileName;
			string path = pathInfo.GetPath(normalizedFileName);
			string url = "";
			string message = ""; 
			int width = 0, height = 0;
			if (System.IO.File.Exists(path))
			{
				url = pathInfo.GetUrl(normalizedFileName);
				
				if (!GetImageSize(path, ref width, ref height))
				{
					message = String.Format(options.NotSupportedMessage, normalizedFileName, Path.GetExtension(normalizedFileName));
				}
			}
			else
			{
				message = String.Format(LibraryStrings.NotExists, normalizedFileName);
			}

			object result = (!String.IsNullOrEmpty(message)) ?
				result = new { proceed = false, msg = message } :
				result = new { proceed = true, url = url, folderUrl = pathInfo.Url, width = width, height = height };
			return Json(result, JsonRequestBehavior.AllowGet);
		}

		private static bool GetImageSize(string path, ref int width, ref int height)
		{
			try
			{
				using (Image img = Image.FromFile(path))
				{
					if (img != null)
					{
						width = img.Width;
						height = img.Height;
						return true;
					}
					else
					{
						return false;
					}
				}
			}
			catch (OutOfMemoryException) // sic!!!
			{
				return false;
			}
		}

		/// <summary>
		/// Формирует имя файла для ресолвинга
		/// </summary>
		/// <param name="path"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		private static string GetResolvingFileName(string path, string name)
		{
			string result;
			int index = 0;
			do
			{
				index++;
				result = MutateHelper.MutateFileName(name, index);
			}
			while (System.IO.File.Exists(Path.Combine(path, result)));
			return result;
		}

		#endregion

		[HttpGet]
		public JsonResult FullNameFileExists(string name)
        {
            return Json(new { result = (name != null && System.IO.File.Exists(name)) }, JsonRequestBehavior.AllowGet);
        }

		[HttpGet]
		public JsonResult FileExists(string path, string name)
		{
			return new JsonResult { Data = new { result = (name != null && System.IO.File.Exists(Path.Combine(path, name))) }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpGet]
		public JsonResult ResolveFileName(string path, string name)
        {
			string result = GetResolvingFileName(path, name);

            return Json( new { result = result }, JsonRequestBehavior.AllowGet);
        }

		[NonAction]
		public static string UploadSecuritySessionKey(string path)
		{
			return "upload_" + path;
		}
		
		[HttpGet]
		public JsonResult CheckSecurity(string path)
		{
			bool result = PathInfo.CheckSecurity(path).Result && CheckFolderExistence(path);
			Session[UploadSecuritySessionKey(path)] = result;
			return Json(new { result = result }, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public JsonResult TestFieldValueDownload(string id, string fileName, bool isVersion, int? entityId)
        {
            int fieldId = Field.ParseFormName(id);
            if (fieldId == 0)
                return Json(new { proceed = false, msg = String.Format(LibraryStrings.InvalidId, id) }, JsonRequestBehavior.AllowGet);
            else
            {
                PathInfo info = GetFilePathInfo(fieldId, entityId, isVersion);
				return GetTestFileDownloadResult(info, fileName, isVersion);
            }
        }

        [HttpGet]
        public JsonResult ExportFileDownload(int id, string fileName)
        {
            Site currentSite = SiteService.Read(id);
            string folderForUpload = String.Format("{0}\\temp\\", currentSite.LiveDirectory);
            PathInfo inf = new PathInfo();
            inf.Path = folderForUpload;
            return GetTestFileDownloadResult(inf, fileName, false);
        }


		[HttpGet]
		public JsonResult TestLibraryFileDownload(int id, string fileName, string entityTypeCode)
		{
			PathInfo pathInfo = (entityTypeCode == EntityTypeCode.ContentFile) ? ContentFolderService.GetPathInfo(id) : SiteFolderService.GetPathInfo(id);
			if (pathInfo == null)
			{
				string formatString = (entityTypeCode == EntityTypeCode.ContentFile) ? LibraryStrings.ContentFolderNotExists : LibraryStrings.SiteFolderNotExists;
				return Json(new { proceed = false, msg = String.Format(formatString, id) }, JsonRequestBehavior.AllowGet);
			}
			else
			{
				return GetTestFileDownloadResult(pathInfo, fileName, false);
			}
		}

		[HttpGet]
		public JsonResult GetImageProperties(string id, string fileName, bool isVersion, int? entityId, int? parentEntityId)
        {
            int fieldId = Field.ParseFormName(id);
            if (fieldId == 0)
                return Json(new { proceed = false, msg = String.Format(LibraryStrings.InvalidId, id) }, JsonRequestBehavior.AllowGet);
            else
            {
				PathInfo pathInfo = GetFilePathInfo(fieldId, entityId, isVersion);
				return GetFileProperties(pathInfo, fileName, new FilePropertiesOptions() { IsVersion = isVersion });
            }
        }

		[HttpGet]
		public JsonResult GetLibraryImageProperties(int id, string fileName, string entityTypeCode)
		{
			PathInfo pathInfo = (entityTypeCode == EntityTypeCode.ContentFile) ? ContentFolderService.GetPathInfo(id) : SiteFolderService.GetPathInfo(id);
			return GetFileProperties(pathInfo, fileName, new FilePropertiesOptions());
		}

		[HttpPost]
		public JsonResult CheckForCrop(string targetFileUrl)
		{
			string path = PathInfo.ConvertToPath(targetFileUrl);
			string ext = Path.GetExtension(path);
			if (System.IO.File.Exists(path))
				return Json(new { ok = false, message = String.Format(LibraryStrings.FileExistsTryAnother, path)  });
			else if (String.IsNullOrEmpty(ext) || GetImageCodecInfo(ext) == null)
				return Json(new { ok = false, message = String.Format(LibraryStrings.ExtensionIsNotAllowed, ext) });
			else if (!PathInfo.CheckSecurity(path).Result)
				return Json(new { ok = false, message = String.Format(LibraryStrings.AccessDenied, path, QPContext.CurrentUserName) });
			else
				return Json(new { ok = true, message = String.Empty });
		}

		[HttpPost]
		public JsonResult Crop(bool overwriteFile, string targetFileUrl, string sourceFileUrl, double resize, int? top, int? left, int? width, int? height)
		{
			string sourcePath = PathInfo.ConvertToPath(sourceFileUrl);
			string targetPath = (overwriteFile) ? sourcePath : PathInfo.ConvertToPath(targetFileUrl);
			if (!PathInfo.CheckSecurity(targetPath).Result)
				return Json(new { ok = false, message = LibraryStrings.AccessDenied });

			int sourceWidth = 0, sourceHeight = 0, sourceTop = 0, sourceLeft = 0;
			if (!GetImageSize(sourcePath, ref sourceWidth, ref sourceHeight))
				return Json(new { ok = false, message = LibraryStrings.ExtensionIsNotAllowed });

			int targetWidth = (width.HasValue) ? width.Value : (int)(sourceWidth * resize);
			int targetHeight = (height.HasValue) ? height.Value : (int)(sourceHeight * resize);
			int targetTop = 0;
			int targetLeft = 0;

			sourceWidth = (width.HasValue) ? (int)((double)width.Value / resize) : sourceWidth;
			sourceHeight = (height.HasValue) ? (int)((double)height.Value / resize) : sourceHeight;
			sourceTop = (top.HasValue) ? (int)((double)top.Value / resize) : sourceTop;
			sourceLeft = (left.HasValue) ? (int)((double)left.Value / resize) : sourceLeft;

			if (System.IO.File.Exists(sourcePath))
			{
				using (Bitmap output = new Bitmap(targetWidth, targetHeight))
				{
					Graphics resizer = Graphics.FromImage(output);
					resizer.InterpolationMode = (resize < 1) ? InterpolationMode.HighQualityBicubic : InterpolationMode.HighQualityBilinear;
					
					using (Bitmap input = new Bitmap(sourcePath))
					{
						resizer.DrawImage(input, new Rectangle(targetLeft, targetTop, targetWidth, targetHeight), sourceLeft, sourceTop, sourceWidth, sourceHeight, GraphicsUnit.Pixel);
					}	

					Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
					if (System.IO.File.Exists(targetPath))
					{
						System.IO.File.SetAttributes(targetPath, FileAttributes.Normal);
						System.IO.File.Delete(targetPath);
					}

					using (FileStream fs = System.IO.File.OpenWrite(targetPath))
					{
						string ext = Path.GetExtension(targetPath);
						output.Save(fs, GetImageCodecInfo(ext), GetEncoderParameters(ext));
					}

				}
			}
			return Json(new { ok = true, message = String.Empty });
		}

		private EncoderParameters GetEncoderParameters(string extension)
		{
			if (extension.Equals(".JPG", StringComparison.InvariantCultureIgnoreCase) || extension.Equals(".JPEG", StringComparison.InvariantCultureIgnoreCase))
			{
					EncoderParameters parameters = new EncoderParameters(1);
					parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);
					return parameters;
			}
			else
				return null;
		}

		private static string GetMimeType(string extension)
		{
			if (extension.Equals(".JPG", StringComparison.InvariantCultureIgnoreCase) || extension.Equals(".JPEG", StringComparison.InvariantCultureIgnoreCase))
				return "image/jpeg";
			else if (extension.Equals(".GIF", StringComparison.InvariantCultureIgnoreCase))
				return "image/gif";
			else if (extension.Equals(".PNG", StringComparison.InvariantCultureIgnoreCase))
				return "image/png";
			else
				return String.Empty;
		}

		private static ImageCodecInfo GetImageCodecInfo(string extension)
		{
			return ImageCodecInfo.GetImageEncoders().Where(n => n.MimeType == GetMimeType(extension)).SingleOrDefault();
		}
		
		[HttpGet]
        public ActionResult DownloadFile(string id, string fileName)
        {
            string path = (string)TempData[id];
            if (!String.IsNullOrEmpty(path))
            {				
				Response.AppendHeader(ContentDispositionKey, string.Format(ContentDispositionTemplate, Server.UrlPathEncode(fileName)));
				return File(path, "application/octet-stream");
            }
            else
                return null;
        }

		[HttpPost]		
		public ActionResult UploadFile(string folderPath, bool resolveFileName)
		{
			if (String.IsNullOrEmpty(folderPath))
				throw new ArgumentException("Folder Path is empty");

			if (!PathInfo.CheckSecurity(folderPath).Result || !CheckFolderExistence(folderPath))
				return Json(new { proceed = false, msg = String.Format(LibraryStrings.UploadIsNotAllowed, folderPath)});

			List<string> allFileNames = new List<string>(this.HttpContext.Request.Files.Count);
			foreach (var fileKey in this.HttpContext.Request.Files.AllKeys)
			{				
				var file = this.HttpContext.Request.Files[fileKey];
				var path = Path.Combine(folderPath, file.FileName);				
				if (System.IO.File.Exists(path))
				{
					// если файл существуем то ресовим его, иначе удаляем
					if (resolveFileName)
					{
						path = Path.Combine(folderPath, GetResolvingFileName(folderPath, file.FileName));
					}
					else
					{
						System.IO.File.SetAttributes(path, FileAttributes.Normal);
						System.IO.File.Delete(path);
					}

				}
				file.SaveAs(path);
				allFileNames.Add(Path.GetFileName(path));
			}
			return Json(new { fileNames = allFileNames.ToArray(), proceed = true }, "text/plain");
		}

		private bool CheckFolderExistence(string path)
		{
			bool result = true;
			if (!Directory.Exists(path))
			{
				try
				{
					Directory.CreateDirectory(path);
				}
				catch
				{
					result = false;
				}
			}
			return result;
		}
    }
}
