using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.Validators
{
    public class EditorAttribute : Attribute
    {
        public EditorType Type
        {
            get;
            set;
        }

        public EditorAttribute(EditorType type)
        {
            Type = type;
        }
    }
}
