using System.ComponentModel;
using System.Configuration.Install;

namespace Quantumart.QP8.ArticleScheduler.WinService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
