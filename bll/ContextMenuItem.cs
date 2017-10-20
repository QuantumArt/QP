namespace Quantumart.QP8.BLL
{
    public class ContextMenuItem
    {
		public int ContextMenuId { get; set; }

		public int ActionId { get; set; }

		
		public string Name
        {
            get;
            set;
        }
        
        public string Icon
        {
            get;
            set;
        }

		public string IconDisabled { get; set; }
        
        public bool BottomSeparator
        {
            get;
            set;
        }

		public int Order { get; set; }

        
        public string ActionCode
        {
            get;
            set;
        }
        
        public string ActionTypeCode
        {
            get;
            set;
        }


    }
}
