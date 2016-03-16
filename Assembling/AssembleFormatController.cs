using System;
using System.Data;
using Assembling.Info;

namespace Assembling
{
    public class AssembleFormatController : AssembleControllerBase
    {
        public int FormatId { get; protected set; }


        public AssembleFormatController(int formatId, AssembleMode mode, string connectionParameter, bool isCustomerCode, AssembleLocation fixedLocation)
            : base(connectionParameter, isCustomerCode)
        {
            UseFixedLocation = true;
            FixedLocation = fixedLocation;
            FillController(formatId, mode);
        }

        public AssembleFormatController(int formatId, AssembleMode mode, string connectionParameter, bool isCustomerCode)
            : base(connectionParameter, isCustomerCode)
        {
            FillController(formatId, mode);
        }

        public AssembleFormatController(int formatId, AssembleMode mode, string customerCode)
            : base(customerCode)
        {
            FillController(formatId, mode);
        }

        public AssembleFormatController(int formatId, AssembleMode mode, DataTable data)
        {
            FillController(formatId, mode, "", data);
        }


        private void FillController(int formatId, AssembleMode mode)
        {
            var sqlQuery =
                " SELECT pt.*, p.*, s.*, obj.object_id, obj.object_name, objf.format_name " + Renames +
                " FROM object_format objf " +
                " INNER JOIN object obj on objf.object_id = obj.object_id" +
                " INNER JOIN page_template AS pt ON pt.page_template_id=obj.page_template_id" +
                " LEFT JOIN page AS p ON p.page_id=obj.page_id" +
                " INNER JOIN site AS s ON pt.site_id = s.site_id" +
                " WHERE objf.object_format_id=" + formatId;
            FillController(formatId, mode, sqlQuery, null);
        }

        public void FillController(int formatId, AssembleMode mode, string sqlQuery, DataTable data)
        {
            FormatId = formatId;
            CurrentAssembleMode = mode;
            if (data != null)
            {
                Info = new AssembleInfo(this, data);
            }
            else if (!String.IsNullOrEmpty(sqlQuery))
            {
                Info = new AssembleInfo(this, sqlQuery);
            }
        }



        public override void Assemble()
        {
            if (String.IsNullOrEmpty(Info.PageId))
            {
                InvalidateTemplateCache();
            }
            else
            {
                InvalidatePageCache();
            }
            AssembleControlSet();
            AssemblePageFiles();
        }

        internal override string GetFilter()
        {
            return "and objf.object_format_id = " + FormatId;
        }

    }
}