using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.EF.CodeFirstV6.Data
{
    public class Setting
    {
        public int Id { get; set; }
        public int STATUS_TYPE_ID { get; set; }
        public bool VISIBLE { get; set; }
        public bool ARCHIVE { get; set; }
        public DateTime CREATED { get; set; }
        public DateTime MODIFIED { get; set; }
        public int LAST_MODIFIED_BY { get; set; }
        public string Title { get; set; }
        public string Value { get; set; }
        public ICollection<Setting> RelatedSettings { get; set; }

        public ICollection<Setting> RelatedSettingsBackward { get; set; }

        public StringDetail ValueExtended { get; set; }
    }

    public class StringDetail 
    {
        public int Id { get; set; }
        public string ValueExtended { get; set; }
    }
}
