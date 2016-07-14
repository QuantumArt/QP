using System;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.ActionResults;
using Quantumart.QP8.BLL.Services.DTO;
using System.Threading;
using System.Xml.Linq;
using Quantumart.QP8.BLL.Repository;
using System.IO;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{        
    //[ConnectionScope]
    public class TestController : QPController
    {
        //
        // GET: /Test/
        [HttpGet]		
        public ActionResult CustomAction(string backend_sid)
        {			
			Thread.Sleep(new TimeSpan(0, 0, 2));

			if(new Random(DateTime.Now.Millisecond).Next(3) == 2)
				return new JsonpResult
				{
					Data = MessageResult.Error("Тестовая ошибка. SID: " + backend_sid),
					JsonRequestBehavior = JsonRequestBehavior.AllowGet
				};
			else			
				return new JsonpResult
				{
					Data = null,
					JsonRequestBehavior = JsonRequestBehavior.AllowGet
				};
        }

		[HttpGet]
		public HtmlString UICustomAction(string backend_sid)
		{
			return new HtmlString("<a href='http://www.google.com/'>www.microsoft.com</a>");
		}


		[HttpGet]
		public HtmlString Fingerprint()
		{
			string xmlSettings = @"
				<fingerprint>
					<entityType code=""site"">
						<included>
							<id>34</id>
						</included>
					</entityType>

					<entityType code=""content"" >
						<!--<included>
							<parentId>34</parentId>
						</included>
						<excepted>
							<id>285</id>
						</excepted>-->
					</entityType>
					<entityType code=""field"">
						<excepted>
							<parentId>285</parentId>
							<parentId>286</parentId>
						</excepted>
					</entityType>
					<entityType code=""article"" considerIdentity=""true"" />
					<entityType code=""notification"" />
					<entityType code=""workflow"" />
					<entityType code=""status_type"" />
					<entityType code=""custom_action"" />
					<entityType code=""visual_editor_plugin"" />
					<entityType code=""visual_editor_style"" />
					<entityType code=""user"" />
					<entityType code=""user_group"" />
					<entityType code=""site_folder"" />
					<entityType code=""content_folder"" />

					<entityType code=""site_permission"" />
					<entityType code=""content_permission"" />
					<entityType code=""workflow_permission"" />
					<entityType code=""site_folder_permission"" />
					<entityType code=""entity_type_permission"" />
					<entityType code=""action_permission"" />

					<entityType code=""template"" />
					<entityType code=""template_object"" />
					<entityType code=""template_object_format"" />
					<entityType code=""page"" />
					<entityType code=""page_object"" />
					<entityType code=""page_object_format"" />

				</fingerprint>";
			XDocument settings = XDocument.Load(new StringReader(xmlSettings));
			byte[] fingerprint = new FingerprintService(new FingerprintRepository()).GetFingerprint(settings);
			return new HtmlString(Convert.ToBase64String(fingerprint));
		}

    }
}
