using System;
using System.Data;
using System.IO;
using System.Text;
using Quantumart.QP8.Assembling.Info;

namespace Quantumart.QP8.Assembling
{
    public class AssemblePageController : AssembleControllerBase
    {
        public int PageId { get; protected set; }

        public bool FirstInBatch { get; protected set; }

        private void FillController(int pageId, DataTable data, bool firstInBatch)
        {
            FirstInBatch = firstInBatch;
            CurrentAssembleMode = AssembleMode.Page;
            PageId = pageId;
            if (data != null)
            {
                Info = new AssembleInfo(this, data);
            }
            else
            {
                var sqlQuery = "select p.*, pt.*, s.* " + Renames + " from page p inner join page_template pt on p.page_template_id = pt.page_template_id inner join site s on pt.site_id = s.site_id where page_id=" + pageId;
                Info = new AssembleInfo(this, sqlQuery);
            }
        }

        public AssemblePageController(int pageId, string connectionParameter, bool firstInBatch)
            : base(connectionParameter)
        {
            FillController(pageId, null, firstInBatch);
        }

        public AssemblePageController(int pageId, string connectionParameter)
            : base(connectionParameter)
        {
            FillController(pageId, null, true);
        }

        public AssemblePageController(int pageId, DataTable data, bool firstInBatch)
        {
            FillController(pageId, data, firstInBatch);
        }

        public AssemblePageController(int pageId, DataTable data)
        {
            ChangePage(pageId, data);
        }

        public AssemblePageController(int pageId, DataRow row)
        {
            ChangePage(pageId, row);
        }

        public void ChangePage(int pageId, DataRow row)
        {
            FillController(pageId, ConvertToDataTable(row), true);
        }

        public void ChangePage(int pageId, DataTable data)
        {
            FillController(pageId, data, true);
        }

        private void ClearPageTrace()
        {
            if (IsDbConnected)
            {
                Cnn.ExecuteSql("delete from page_trace where page_id = " + PageId);
            }
        }

        private void RemoveOldTemplateControl()
        {
            var sb = new StringBuilder();
            sb.Append(Info.Paths.PageControlsPath);
            sb.Append(@"\");
            sb.Append(Info.NetTemplateName).Append(".ascx");
            var fileName = sb.ToString();
            if (File.Exists(fileName))
            {
                DeleteFile(fileName);
                DeleteFile(fileName + ".vb");
                DeleteFile(fileName + ".cs");
            }
        }

        public override void Assemble()
        {
            CreateFolder(Info.Paths.PageControlsPath);
            RemoveOldTemplateControl();
            CreateFolder(Info.Paths.TemplateControlsPath);
            ClearPageTrace();

            if (Info.AssembleAllControls)
            {
                AssembleAllControls();
            }
            else
            {
                AssembleControlSet();
            }

            AssemblePageFiles();
            InvalidatePageCache();
            InvalidateTemplateCache();
            InvalidateStructureCache();
        }

        private void AssembleAllControls()
        {
            if (!IsDbConnected)
            {
                throw new InvalidOperationException("Cannot proceed \"Assemble All Controls\" command in disconnected mode");
            }

            if (FirstInBatch)
            {
                var templateController = new AssembleTemplateObjectsController(Info.TemplateId, Cnn);
                templateController.Info.Controls.FillTemplateRow();
                templateController.Assemble();
            }

            var pageController = new AssemblePageObjectsController(PageId, Cnn);
            pageController.Assemble();
        }
    }
}
