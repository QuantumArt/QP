using System;
using System.Data;
using Assembling.Info;
using Quantumart.QP8.Assembling;

namespace Assembling
{
    public class AssemblePageObjectsController : AssembleControllerBase
    {
        public int PageId { get; private set; }

        public void FillController(int pageId)
        {
            var sqlQuery =
                " SELECT p.*, pt.*, s.* " + Renames +
                " FROM page AS p  " +
                " INNER JOIN page_template AS pt ON pt.page_template_id = p.page_template_id" +
                " INNER JOIN site AS s ON pt.site_id = s.site_id" +
                " where p.page_id = " + pageId;
            FillController(pageId, sqlQuery, null);
        }

        public void FillController(int pageId, string sqlQuery, DataTable data)
        {
            CurrentAssembleMode = AssembleMode.AllPageObjects;
            PageId = pageId;
            if (data != null)
            {
                Info = new AssembleInfo(this, data);
            }
            else if (!String.IsNullOrEmpty(sqlQuery))
            {
                Info = new AssembleInfo(this, sqlQuery);
            }
        }

        public AssemblePageObjectsController(int pageId, string connectionParameter)
            : base(connectionParameter)
        {
            FillController(pageId);
        }

        public AssemblePageObjectsController(int pageId, DbConnector cnn)
            : base(cnn)
        {
            FillController(pageId);
        }

        public AssemblePageObjectsController(int pageId, DataTable data)
        {
            FillController(pageId, "", data);
        }


        internal override string GetFilter()
        {
            return " and obj.page_id = " + PageId;
        }

        public override void Assemble()
        {
            AssembleControlSet();
            InvalidatePageCache();
        }

    }
}
