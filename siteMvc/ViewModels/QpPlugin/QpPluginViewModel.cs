using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading.Tasks;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.QpPlugin
{
    public class QpPluginViewModel : EntityViewModel
    {
        private IHttpClientFactory _factory;
        public static QpPluginViewModel Create(BLL.QpPlugin plugin, string tabId, int parentId, IHttpClientFactory factory)
        {
            var model = Create<QpPluginViewModel>(plugin, tabId, parentId);
            model.CreationMode = QpPluginCreationMode.ByServiceUrl;
            model._factory = factory;
            return model;
        }

        public BLL.QpPlugin Data
        {
            get => (BLL.QpPlugin)EntityData;
            set => EntityData = value;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.QpPlugin;
        public override string ActionCode  => IsNew ? Constants.ActionCode.AddNewQpPlugin : Constants.ActionCode.QpPluginProperties;

        [Display(Name = "PluginCreationMode", ResourceType = typeof(QpPluginStrings))]
        public QpPluginCreationMode CreationMode { get; set; }


        [Display(Name = "ReloadContract", ResourceType = typeof(QpPluginStrings))]
        public bool ReloadContract { get; set; }

        public IEnumerable<ListItem> GetCreationModes() => new[]
        {
            new ListItem(QpPluginCreationMode.ByServiceUrl.ToString(), QpPluginStrings.ByServiceUrl, "ByServiceUrlPanel"),
            new ListItem(QpPluginCreationMode.ByContract.ToString(), QpPluginStrings.ByContract, "ByContractPanel")
        };

        public override void DoCustomBinding()
        {
            if (CreationMode == QpPluginCreationMode.ByServiceUrl && !String.IsNullOrEmpty(Data.ServiceUrl) && (IsNew || ReloadContract))
            {
                try
                {
                    var client = _factory.CreateClient();
                    var task = Task.Run(() => client.GetAsync(Data.ServiceUrl));
                    task.Wait();
                    var response = task.Result;
                    Data.Contract = response.IsSuccessStatusCode ? response.Content.ReadAsStringAsync().Result : null;
                    Data.ContractLoaded = true;
                }
                catch (Exception ex)
                {
                    Data.LoadedContractInvalidMessage = ex.Message;
                }
            }

            base.DoCustomBinding();
        }

        public Dictionary<string, object> AreaAttributes =>
            new Dictionary<string, object>
            {
                { "class", "textbox hta-JsonTextArea highlightedTextarea" },
                { "style", "height: 300px" }
            };

        public Dictionary<string, object> DisabledAreaAttributes
        {
            get
            {
                var attrs = AreaAttributes;
                attrs.Add("disabled", "disabled");
                return attrs;
            }

        }
    }
}
