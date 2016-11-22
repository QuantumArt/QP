using System.ServiceProcess;

namespace Quantumart.QP8.ArticleScheduler.WinService
{
    public partial class ArticleSchedulerService : ServiceBase
    {
        private readonly QpSchedulerProcessor _processor;

        public ArticleSchedulerService()
        {
            InitializeComponent();
            var settings = Properties.Settings.Default;
            _processor = new QpSchedulerProcessor(settings.RecurrentTimeout, Properties.Settings.SplitExceptCustomerCodes(settings.ExceptCustomerCodes));
        }

        protected override void OnStart(string[] args)
        {
            _processor.Run();
        }

        protected override void OnStop()
        {
            _processor.Stop();
        }
    }
}
