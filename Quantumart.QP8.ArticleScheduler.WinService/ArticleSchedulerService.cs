using System.ServiceProcess;
using Quantumart.QP8.ArticleScheduler.WinService.Properties;

namespace Quantumart.QP8.ArticleScheduler.WinService
{
    public partial class ArticleSchedulerService : ServiceBase
    {
        private readonly QpSchedulerProcessor _processor;

        public ArticleSchedulerService()
        {
            InitializeComponent();
            _processor = new QpSchedulerProcessor(Settings.Default.RecurrentTimeout, Settings.SplitExceptCustomerCodes(Settings.Default.ExceptCustomerCodes));
        }

        public ArticleSchedulerService(QpSchedulerProcessor processor)
        {
            InitializeComponent();
            _processor = processor;
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
