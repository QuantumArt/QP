using System;

namespace Quantumart.QP8.BLL.Models.XmlDbUpdate
{
    public class XmlDbUpdateLogModel
    {
        public int Id { get; set; }

        public string Hash { get; set; }

        public DateTime Applied { get; set; }

        public string FileName { get; set; }

        public int UserId { get; set; }

        public string Body { get; set; }

        public string Version { get; set; }
    }
}
