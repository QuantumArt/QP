namespace Assembling.Info
{
    public class InitialCode
    {
        public string CodeBehind { get; }

        public string Presentation { get; }

        public InitialCode(string presentation, string codeBehind)
        {
            CodeBehind = codeBehind;
            Presentation = presentation;
        }

    }
}