namespace Quantumart.QPublishing.Info
{
	public class Site
	{
        public int Id { get; set; }
        public string Name { get; set; }
        public string Dns { get; set; }
        public string StageDns { get; set; }
        public string LiveDirectory { get; set; }
        public string StageDirectory { get; set; }
        public string TestDirectory { get; set; }
        public string AssemblyDirectory { get; set; }
        public string StageAssemblyDirectory { get; set; }
        public string ContextClassName { get; set; }
        public bool AssembleFormatsInLive { get; set; }
        public string LiveVirtualRoot { get; set; }
        public string StageVirtualRoot { get; set; }
        public string UploadUrl { get; set; }
        public string UploadDir { get; set; }
        public bool UseAbsoluteUploadUrl { get; set; }
        public string UploadUrlPrefix { get; set; }
        public int FieldBorderMode {get; set; }
        public bool IsLive { get; set; }
        public string ScriptLanguage { get; set; }
        public bool AllowUserSessions { get; set; }
        public bool EnableOnScreen { get; set; }
	}
}
