using System;
using System.Data;
using Quantumart.QP8.Assembling.Info;
using Quantumart.QP8.Assembling;

namespace Quantumart.QP8.Assembling
{

    public class AssembleTemplateObjectsController : AssembleControllerBase
    {
        public int TemplateId { get; private set; }

        public void FillController(int templateId)
        {
            var sqlQuery =
                " SELECT pt.*, s.* " + RenamesWithoutPage +
                " FROM page_template AS pt " +
                " INNER JOIN site AS s ON pt.site_id = s.site_id" +
                " where pt.page_template_id = " + templateId;
            FillController(templateId, sqlQuery, null);
        }

        public void FillController(int templateId, string sqlQuery, DataTable data)
        {
            CurrentAssembleMode = AssembleMode.AllTemplateObjects;
            TemplateId = templateId;
            if (data != null)
            {
                Info = new AssembleInfo(this, data);
            }
            else if (!String.IsNullOrEmpty(sqlQuery))
            {
                Info = new AssembleInfo(this, sqlQuery);
            }
        }

        public AssembleTemplateObjectsController(int templateId, DataTable data)
        {
            FillController(templateId, "", data);
        }
        public AssembleTemplateObjectsController(int templateId, DataRow row)
        {
            FillController(templateId, "", ConvertToDataTable(row));
        }

        public AssembleTemplateObjectsController(int templateId, string connectionParameter)
            : base(connectionParameter)
        {
            FillController(templateId);
        }


        public AssembleTemplateObjectsController(int templateId, DbConnector cnn)
            : base(cnn)
        {
            FillController(templateId);
        }


        internal override string GetFilter()
        {
            return " and obj.page_id is null and obj.page_template_id = " + TemplateId;
        }

        public override void Assemble()
        {
            AssembleControlSet();
            InvalidateTemplateCache();
        }

    }
}
