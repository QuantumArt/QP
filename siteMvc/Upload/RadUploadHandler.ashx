<%@ WebHandler Language="C#" Class="RadUploadHandler" %>

using System;
using System.Web;
using System.IO;
using Quantumart.QPublishing.Database;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.WebMvc.Controllers;
using System.Web.SessionState;

public class RadUploadHandler : Telerik.Windows.RadUploadHandler, IReadOnlySessionState
{
    
    protected string tempFileName;
	protected string fileName;

	private bool CheckSecurity()
	{
		object obj = HttpContext.Current.Session[LibraryController.UploadSecuritySessionKey(this.GetTargetFolder())];
		return (obj != null && (bool)obj);
	}
	
    public override void ProcessStream() 
    {
		if (!CheckSecurity())
			return;
			    
		this.ResultChunkTag = String.Empty;
        this.fileName = GetFilePath();
		
        if (this.IsNewFileRequest())
        {
			this.ResultChunkTag = string.Format(" [{0:yyyymmdd.hhmmss.ff}]", DateTime.Now);
        }
        else
        {
            if (this.FormChunkTag != null)
            {
                this.ResultChunkTag = this.FormChunkTag;
            }

        }
        this.TargetPhysicalFolder = QPConfiguration.TempDirectory;

		this.tempFileName = GetFilePath();

        base.ProcessStream();

		if (this.IsFinalFileRequest())  
        {
			if (File.Exists(this.fileName))
			{
				File.SetAttributes(this.fileName, FileAttributes.Normal);
				File.Delete(this.fileName); 
			}
            File.Move(this.tempFileName, this.fileName);
		}  
    }

	public override string GetFilePath(string fileName)
	{
		if (!String.IsNullOrEmpty(this.ResultChunkTag))  
        {  
            int i = fileName.LastIndexOf('.');  
            if (i >= 0)  
            {  
                fileName = fileName.Insert(i, this.ResultChunkTag);  
            }  
         }
        return base.GetFilePath(fileName);
	}
 
}