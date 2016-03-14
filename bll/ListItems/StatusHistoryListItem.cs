using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL.ListItems
{
    public class StatusHistoryListItem
    {
        public int Id { get; set; }
        public string StatusTypeName { get; set; }
        public string SystemStatusTypeName { get; set; }
        public DateTime ActionDate { get; set; }
        public string ActionMadeBy { get; set; }
        private string comment = String.Empty;
        [LocalizedDisplayName("LastComment", NameResourceType = typeof(ArticleStrings))]
        public string Comment
        {
            get
            {
                return comment;
            }
            set
            {
                if (value.ToString().Trim().EndsWith("Comment:"))
                {
                    comment = value.ToString().Replace("Comment: ", "");
                }
                else {
                    comment = value;
                }
                
            }
        }
    }
}
