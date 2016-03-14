using Quantumart.QP8.BLL.Services.MultistepActions.Import;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Quantumart.QP8.BLL.Exceptions
{
	[Serializable]
	public class ImportException : Exception
	{
		private const string SettingsKey = "Settings";

		public ImportSettings Settings { get; private set; }

		public ImportException(string message, Exception innerException, ImportSettings settings)
			: base(message, innerException)
		{
			Settings = settings;
		}

		protected ImportException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			Settings = (ImportSettings)info.GetValue(SettingsKey, typeof(ImportSettings));
		}

		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(SettingsKey, Settings, typeof(ImportSettings));
			base.GetObjectData(info, context);
		}
	}
}
