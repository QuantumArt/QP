using System.ServiceProcess;
using Quantumart.QP8.ArticleScheduler.WinService.Properties;

namespace Quantumart.QP8.ArticleScheduler.WinService
{
    public partial class ArticleSchedulerService : ServiceBase
    {
        internal readonly QpSchedulerProcessor Processor;

        public ArticleSchedulerService()
        {
            InitializeComponent();
            Processor = new QpSchedulerProcessor(Settings.Default.RecurrentTimeout, Settings.SplitExceptCustomerCodes(Settings.Default.ExceptCustomerCodes));
        }

        public ArticleSchedulerService(QpSchedulerProcessor processor)
        {
            InitializeComponent();
            Processor = processor;
        }

        protected override void OnStart(string[] args)
        {
            Processor.Run();
        }

        protected override void OnStop()
        {
            Processor.Stop();
        }
    }
}
