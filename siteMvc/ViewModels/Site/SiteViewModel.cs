using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.Site
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
            get => (BLL.Site)EntityData;
            set => EntityData = value;
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
            model.DefaultCommandsListItems = allCommands.Select(c => new ListItem
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

            model.AllStylesListItems = allVeStylesAndFormats.Where(s => s.IsFormat == false).Select(x => new ListItem { Value = x.Id.ToString(), Text = x.Name }).ToArray();
            model.AllFormatsListItems = allVeStylesAndFormats.Where(s => s.IsFormat).Select(x => new ListItem { Value = x.Id.ToString(), Text = x.Name }).ToArray();
            model.ActiveVeStyles = allVeStylesAndFormats.Where(s => s.IsFormat == false && s.On).Select(x => new QPCheckedItem { Value = x.Id.ToString() }).ToList();
            model.ActiveVeFormats = allVeStylesAndFormats.Where(s => s.IsFormat && s.On).Select(x => new QPCheckedItem { Value = x.Id.ToString() }).ToList();
            return model;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.Site;

        public override string ActionCode => IsNew ? Constants.ActionCode.AddNewSite : Constants.ActionCode.SiteProperties;

        public override string CaptureLockActionCode => Constants.ActionCode.CaptureLockSite;

        public List<ListItem> AssemblingTypes => new List<ListItem>
        {
            new ListItem(AssemblingType.AspDotNet, SiteStrings.AspDotNet, new[]{ AssemblingNetOptionsBlock, BinBlock, LinqBlock, TestDirectoryBlock }),
            new ListItem(AssemblingType.Asp, SiteStrings.Asp, OnScreenAspBlock)
        };

        public List<ListItem> SiteModes => new List<ListItem>
        {
            new ListItem("true", SiteStrings.Live),
            new ListItem("false", SiteStrings.Stage)
        };

        public List<ListItem> OnScreenModes => new List<ListItem>
        {
            new ListItem(OnScreenBorderMode.Always.ToString(), SiteStrings.Always),
            new ListItem(OnScreenBorderMode.OnMouseOver.ToString(), SiteStrings.OnMouseOver),
            new ListItem(OnScreenBorderMode.Never.ToString(),SiteStrings.Never)
        };

        [LocalizedDisplayName("Commands", NameResourceType = typeof(VisualEditorStrings))]
        public IEnumerable<ListItem> DefaultCommandsListItems
        {
            get;
            private set;
        }

        [LocalizedDisplayName("Commands", NameResourceType = typeof(VisualEditorStrings))]
        public IList<QPCheckedItem> ActiveVeCommands { get; set; }

        public int[] ActiveVeCommandsIds { get { return ActiveVeCommands.Select(c => int.Parse(c.Value)).ToArray(); } }

        public int[] ActiveVeStyleIds { get { return ActiveVeStyles.Union(ActiveVeFormats).Select(c => int.Parse(c.Value)).ToArray(); } }

        public IEnumerable<ListItem> AllStylesListItems { get; private set; }

        public IEnumerable<ListItem> AllFormatsListItems { get; private set; }

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
