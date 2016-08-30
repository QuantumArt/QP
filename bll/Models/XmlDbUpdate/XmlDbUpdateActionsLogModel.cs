using System;

namespace Quantumart.QP8.BLL.Models.XmlDbUpdate
{
    public class XmlDbUpdateActionsLogModel
    {
        public int Id{ get; set; }

        public int UpdateId { get; set; }

        public string Ids { get; set; }

        public int ParentId { get; set; }

        public DateTime Applied { get; set; }

        public int UserId { get; set; }

        public string SourceXml { get; set; }

        public string ResultXml { get; set; }

        public string Hash { get; set; }
    }
}
