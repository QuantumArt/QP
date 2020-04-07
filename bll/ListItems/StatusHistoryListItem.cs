using System;
using System.ComponentModel.DataAnnotations;
using Quantumart.QP8.Resources;

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

        [Display(Name = "LastComment", ResourceType = typeof(ArticleStrings))]
        public string Comment
        {
            get => _comment;
            set => _comment = value.Trim().EndsWith("Comment:") ? value.Replace("Comment: ", string.Empty) : value;
        }
    }
}
