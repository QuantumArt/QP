using System;
using System.Data;
using Assembling.Info;
using Quantumart.QP8.Assembling;

namespace Assembling
{
    public class AssembleSelectedObjectsController : AssembleControllerBase
    {
        public string Ids { get; }

        public AssembleSelectedObjectsController(string ids, string connectionParameter)
            : base(connectionParameter)
        {
            Ids = ids;
            CurrentAssembleMode = AssembleMode.SelectedObjects;
            var sqlQuery =
                " SELECT pt.*, s.*, p.* " + Renames +
                " from object obj " +
                " INNER JOIN page_template AS pt ON pt.page_template_id=obj.page_template_id" +
                " LEFT JOIN page AS p ON p.page_id=obj.page_id" +
                " INNER JOIN site AS s ON pt.site_id = s.site_id" +
                " where obj.object_id in ( " + Ids + ")";
            Info = new AssembleInfo(this, sqlQuery);

        }

        public AssembleSelectedObjectsController(string ids, DataTable data)
        {
            Ids = ids;
            CurrentAssembleMode = AssembleMode.SelectedObjects;
            Info = new AssembleInfo(this, data);
        }

        internal override string GetFilter()
        {
            return " and obj.object_id in ( " + Ids + ")";
        }

        public override void Assemble()
        {
            AssembleControlSet();
            if (String.IsNullOrEmpty(Info.PageId))
            {
                InvalidateTemplateCache();
            }
            else
            {
                InvalidatePageCache();
            }
        }
    }
}
