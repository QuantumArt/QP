using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.Validators
{
    public class RuleSetAttribute : Attribute
    {
        public string Name
        {
            get;
            set;
        }

        public RuleSetAttribute(string name)
        {
            Name = name;
        }
    }
}
