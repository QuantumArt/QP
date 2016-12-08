namespace Quantumart.QP8.Assembling.Info
{
    public class CodeFile
    {
        public string Code { get; }

        public string FileName { get; }

        public CodeFile(string code, string fileName)
        {
            Code = code;
            FileName = fileName;
        }
    }
}