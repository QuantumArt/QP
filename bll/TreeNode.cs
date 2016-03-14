using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Quantumart.QP8;
using Quantumart.QP8.DAL;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Mappers;

namespace Quantumart.QP8.BLL
{
    public class TreeNode
    {
        public int Id
        {
            get;
            set;
        }
        
        public string Code
        {
            get;
            set;
        }
        
        public int? ParentId
        {
            get;
            set;
        }

		public int? ParentGroupId
		{
			get;
			set;
		}
        
        public bool IsFolder
        {
            get;
            set;
        }

		public bool IsGroup
		{
			get;
			set;
		}

		public string GroupItemCode
		{
			get;
			set;
		}
        
        public string Icon
        {
            get;
            set;
        }
        
        public string Title
        {
            get;
            set;
        }
        
        public string DefaultActionCode
        {
            get;
            set;
        }
        
        public string DefaultActionTypeCode
        {
            get;
            set;
        }
        
        public string ContextMenuCode
        {
            get;
            set;
        }
        
        public bool HasChildren
        {
            get;
            set;
        }

        public IEnumerable<TreeNode> ChildNodes 
		{ 
			get; 
			internal set; 
		}
    }
}
