
using System;

namespace Quantumart.QP8.Assembling.Info
{
    public class Code
    {
        public CodeFile CodeBehind { get; private set; }

        public CodeFile Presentation { get; private set; }

        public CodeFile SystemCodeBehind { get; private set; }

        public string AbsolutePath { get; private set; }

        public string RelativePath { get; private set; }

        public Code(CodeFile presentation, CodeFile codeBehind, CodeFile systemCodeBehind)
        {
            Fill(presentation, codeBehind, systemCodeBehind, "", "");
        }

        public Code(CodeFile presentation, CodeFile codeBehind, CodeFile systemCodeBehind, string basePath, string fullPath)
        {
            Fill(presentation, codeBehind, systemCodeBehind, basePath, fullPath);
        }

        private void Fill(CodeFile presentation, CodeFile codeBehind, CodeFile systemCodeBehind, string basePath, string fullPath)
        {
            CodeBehind = codeBehind;
            Presentation = presentation;
            SystemCodeBehind = systemCodeBehind;
            AbsolutePath = fullPath;
            RelativePath = String.IsNullOrEmpty(fullPath) ? "" : fullPath.Replace(basePath, "");
        }
    }
}