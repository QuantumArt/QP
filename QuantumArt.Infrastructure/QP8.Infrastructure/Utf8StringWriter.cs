﻿using System.IO;
using System.Text;

namespace QP8.Infrastructure
{
    public class Utf8StringWriter : StringWriter
    {
        public Utf8StringWriter(StringBuilder sb)
            : base(sb)
        {
        }

        public override Encoding Encoding => Encoding.UTF8;
    }
}
