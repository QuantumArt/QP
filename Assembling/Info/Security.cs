namespace Quantumart.QP8.Assembling.Info
{
    public class SecurityOptions
    {
        public AssembleInfo Info { get; }

        public SecurityOptions(AssembleInfo info)
        {
            Info = info;
            UserIdVbName = string.IsNullOrEmpty(AssembleInfo.Configuration("security_UID_varname_VB")) ? "Session(\"qp_UID\")" : AssembleInfo.Configuration("security_UID_varname_VB");
            GroupIdVbName = string.IsNullOrEmpty(AssembleInfo.Configuration("security_GID_varname_VB")) ? "Session(\"qp_GID\")" : AssembleInfo.Configuration("security_GID_varname_VB");
            UserIdCSharpName = string.IsNullOrEmpty(AssembleInfo.Configuration("security_UID_varname_CSharp")) ? "Session[\"qp_UID\"]" : AssembleInfo.Configuration("security_UID_varname_CSharp");
            GroupIdCSharpName = string.IsNullOrEmpty(AssembleInfo.Configuration("security_GID_varname_CSharp")) ? "Session[\"qp_GID\"]" : AssembleInfo.Configuration("security_GID_varname_CSharp");
        }

        public string UserIdVbName { get; }

        public string GroupIdVbName { get; }

        public string UserIdCSharpName { get; }

        public string GroupIdCSharpName { get; }
    }
}
