using System;
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

        private string _comment = string.Empty;

        [LocalizedDisplayName("LastComment", NameResourceType = typeof(ArticleStrings))]
        public string Comment
        {
            get
            {
                return _comment;
            }
            set
            {
                _comment = value.Trim().EndsWith("Comment:") ? value.Replace("Comment: ", string.Empty) : value;
            }
        }
    }
}
