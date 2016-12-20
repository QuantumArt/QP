using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class SiteViewModel : LockableEntityViewModel
    {
        public readonly string AssemblingNetOptionsBlock = "assemblingBlock";
        public readonly string LinqBlock = "linqBlock";
        public readonly string BinBlock = "binBlock";
        public readonly string StageDnsBlock = "stageDnsBlock";
        public readonly string ExtDevBlock = "extDevBlock";
        public readonly string UploadUrlPrefixBlock = "uploadUrlPrefixBlock";
        public readonly string TestDirectoryPathBlock = "testDirectoryPathBlock";
        public readonly string MappingWithDbBlock = "mappingWithDbBlock";
        public readonly string OnScreenAspBlock = "onScreenAspBlock";
        public readonly string MapFileBlock = "mapFileBlock";
        public readonly string OnScreenBlock = "onScreenBlock";
        public readonly string TestDirectoryBlock = "testDirectoryBlock";
        public readonly string XamlDictionariesBlock = "XamlDictionariesBlock";

        public string AggregationListItemsDataExternalCssItems { get; set; }

        public new BLL.Site Data
        {
            get
            {
                return (BLL.Site)EntityData;
            }
            set
            {
                EntityData = value;
            }
        }

        public static SiteViewModel Create(BLL.Site site, string tabId, int parentId)
        {
            var model = Create<SiteViewModel>(site, tabId, parentId);
            var allCommands = SiteService.GetAllVisualEditorCommands().ToList();
            var commandsBinding = SiteService.GetCommandBinding(site.Id);

            foreach (var bnd in commandsBinding)
            {
                allCommands.Single(x => x.Id == bnd.Key).On = bnd.Value;
            }

            model.ActiveVeCommands = allCommands.Where(c => c.On).Select(c => new QPCheckedItem { Value = c.Id.ToString() }).ToList();
            model.DefaultCommandsListItems = allCommands.Select(c => new BLL.ListItem
            {
                Value = c.Id.ToString(),
                Text = c.Alias
            }).ToArray();

            var allVeStylesAndFormats = SiteService.GetAllVeStyles().ToList();
            var stylesBinding = SiteService.GetStyleBinding(site.Id);
            foreach (var bnd in stylesBinding)
            {
                allVeStylesAndFormats.Single(x => x.Id == bnd.Key).On = bnd.Value;
            }

            model.AllStylesListItems = allVeStylesAndFormats.Where(s => s.IsFormat == false).Select(x => new BLL.ListItem { Value = x.Id.ToString(), Text = x.Name }).ToArray();
            model.AllFormatsListItems = allVeStylesAndFormats.Where(s => s.IsFormat).Select(x => new BLL.ListItem { Value = x.Id.ToString(), Text = x.Name }).ToArray();
            model.ActiveVeStyles = allVeStylesAndFormats.Where(s => s.IsFormat == false && s.On).Select(x => new QPCheckedItem { Value = x.Id.ToString() }).ToList();
            model.ActiveVeFormats = allVeStylesAndFormats.Where(s => s.IsFormat && s.On).Select(x => new QPCheckedItem { Value = x.Id.ToString() }).ToList();
            return model;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.Site;

        public override string ActionCode => IsNew ? Constants.ActionCode.AddNewSite : Constants.ActionCode.SiteProperties;

        public override string CaptureLockActionCode => Constants.ActionCode.CaptureLockSite;

        public List<BLL.ListItem> AssemblingTypes => new List<BLL.ListItem>
        {
            new BLL.ListItem(Constants.AssemblingType.AspDotNet, SiteStrings.AspDotNet, new[]{ AssemblingNetOptionsBlock, BinBlock, LinqBlock, TestDirectoryBlock }),
            new BLL.ListItem(Constants.AssemblingType.Asp, SiteStrings.Asp, OnScreenAspBlock)
        };

        public List<BLL.ListItem> SiteModes => new List<BLL.ListItem>
        {
            new BLL.ListItem("true", SiteStrings.Live),
            new BLL.ListItem("false", SiteStrings.Stage)
        };

        public List<BLL.ListItem> OnScreenModes => new List<BLL.ListItem>
        {
            new BLL.ListItem(Constants.OnScreenBorderMode.Always.ToString(), SiteStrings.Always),
            new BLL.ListItem(Constants.OnScreenBorderMode.OnMouseOver.ToString(), SiteStrings.OnMouseOver),
            new BLL.ListItem(Constants.OnScreenBorderMode.Never.ToString(),SiteStrings.Never)
        };

        [LocalizedDisplayName("Commands", NameResourceType = typeof(VisualEditorStrings))]
        public IEnumerable<BLL.ListItem> DefaultCommandsListItems
        {
            get;
            private set;
        }

        [LocalizedDisplayName("Commands", NameResourceType = typeof(VisualEditorStrings))]
        public IList<QPCheckedItem> ActiveVeCommands { get; set; }

        public int[] ActiveVeCommandsIds { get { return ActiveVeCommands.Select(c => int.Parse(c.Value)).ToArray(); } }

        public int[] ActiveVeStyleIds { get { return ActiveVeStyles.Union(ActiveVeFormats).Select(c => int.Parse(c.Value)).ToArray(); } }

        public IEnumerable<BLL.ListItem> AllStylesListItems { get; private set; }

        public IEnumerable<BLL.ListItem> AllFormatsListItems { get; private set; }

        [LocalizedDisplayName("Styles", NameResourceType = typeof(VisualEditorStrings))]
        public IList<QPCheckedItem> ActiveVeStyles { get; set; }

        [LocalizedDisplayName("Formats", NameResourceType = typeof(VisualEditorStrings))]
        public IList<QPCheckedItem> ActiveVeFormats { get; set; }

        internal void DoCustomBinding()
        {
            if (!string.IsNullOrEmpty(AggregationListItemsDataExternalCssItems))
            {
                Data.ExternalCssItems = new JavaScriptSerializer().Deserialize<List<ExternalCss>>(AggregationListItemsDataExternalCssItems);
                Data.ExternalCss = ExternalCssHelper.ConvertToString(Data.ExternalCssItems);
            }
        }

        public override void Validate(ModelStateDictionary modelState)
        {
            modelState.Clear();
            base.Validate(modelState);
        }
    }
}
