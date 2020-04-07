
namespace Quantumart.QP8.Configuration
{
    public class Authentication
    {
        public Authentication()
        {
            AllowSaveUserInformationInCookie = true;
            WinLogonUrl = "";
            WinLogonIpRanges = new IpRange[] {};
        }
        public bool AllowSaveUserInformationInCookie { get; set; }

        public string WinLogonUrl { get; set; }

        public IpRange[] WinLogonIpRanges { get; set; }

    }
}
