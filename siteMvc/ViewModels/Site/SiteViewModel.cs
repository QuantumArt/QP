using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using B = Quantumart.QP8.BLL;
using C = Quantumart.QP8.Constants;
using System.Web.Script.Serialization;
using System;


namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class SiteViewModel : LockableEntityViewModel
	{		
		public readonly string AssemblingNetOptionsBlock = "assemblingBlock";
		public readonly string LinqBlock = "linqBlock";
		public readonly string BinBlock = "binBlock";
		public readonly string StageDnsBlock = "stageDnsBlock";
		public readonly string UploadUrlPrefixBlock = "uploadUrlPrefixBlock";
		public readonly string TestDirectoryPathBlock = "testDirectoryPathBlock";
		public readonly string MappingWithDbBlock = "mappingWithDbBlock";
		public readonly string OnScreenAspBlock = "onScreenAspBlock";
		public readonly string MapFileBlock = "mapFileBlock";
		public readonly string OnScreenBlock = "onScreenBlock";
		public readonly string TestDirectoryBlock = "testDirectoryBlock";
		public readonly string XamlDictionariesBlock = "XamlDictionariesBlock";

		public string AggregationListItems_Data_ExternalCssItems { get; set; }


		public new B.Site Data
		{
			get
			{
				return (B.Site)EntityData;
			}
			set
			{
				EntityData = value;
			}
		}
		
		#region creation
		
		public static SiteViewModel Create(Site site, string tabId, int parentId)
		{
			var model = EntityViewModel.Create<SiteViewModel>(site, tabId, parentId);
			var allCommands = SiteService.GetAllVisualEditorCommands();//all commands
			var commandsBinding = SiteService.GetCommandBinding(site.Id);

			foreach (var bnd in commandsBinding)
				allCommands.Single(x => x.Id == bnd.Key).On = bnd.Value;

			model.ActiveVeCommands = allCommands.Where(c => c.On).Select(c => new QPCheckedItem { Value = c.Id.ToString() }).ToList();
			model.DefaultCommandsListItems = allCommands.Select(c => new ListItem
				{
					Value = c.Id.ToString(),
					Text = c.Alias
				})
				.ToArray();

			var allVeStylesAndFormats = SiteService.GetAllVeStyles().ToList();
			var stylesBinding = SiteService.GetStyleBinding(site.Id);

			foreach (var bnd in stylesBinding)
				allVeStylesAndFormats.Single(x => x.Id == bnd.Key).On = bnd.Value;

			model.AllStylesListItems = allVeStylesAndFormats.Where(s => s.IsFormat == false).Select(x => new ListItem
			{
				Value = x.Id.ToString(),
				Text = x.Name
			})
			.ToArray();

			model.AllFormatsListItems = allVeStylesAndFormats.Where(s => s.IsFormat == true).Select(x => new ListItem
			{
				Value = x.Id.ToString(),
				Text = x.Name
			})
			.ToArray();

			model.ActiveVeStyles = allVeStylesAndFormats.Where(s => s.IsFormat == false && s.On).Select(x => new QPCheckedItem { Value = x.Id.ToString() }).ToList();
			model.ActiveVeFormats = allVeStylesAndFormats.Where(s => s.IsFormat == true && s.On).Select(x => new QPCheckedItem { Value = x.Id.ToString() }).ToList();
			return model;
		}

		#endregion

		#region read-only members
		public override string EntityTypeCode
		{
			get
			{
				return C.EntityTypeCode.Site;
			}
		}

		public override string ActionCode
		{
			get
			{
				if (this.IsNew)
				{
					return C.ActionCode.AddNewSite;
				}
				else
				{
					return C.ActionCode.SiteProperties;
				}
			}
		}

		public override string CaptureLockActionCode
		{
			get
			{
				return C.ActionCode.CaptureLockSite;
			}
		}

		public List<ListItem> AssemblingTypes
		{
			get
			{
				return new List<ListItem>() {
                    new ListItem(C.AssemblingType.AspDotNet, SiteStrings.AspDotNet, new[]{ AssemblingNetOptionsBlock, BinBlock, LinqBlock, TestDirectoryBlock }),
                    new ListItem(C.AssemblingType.Asp, SiteStrings.Asp, OnScreenAspBlock),
                };
			}
		}

		public List<ListItem> SiteModes
		{
			get
			{
				return new List<ListItem>() {
                    new ListItem("true", SiteStrings.Live),
                    new ListItem("false", SiteStrings.Stage),
                };
			}
		}

		public List<ListItem> OnScreenModes
		{
			get
			{
				return new List<ListItem>() {
                    new ListItem(C.OnScreenBorderMode.Always.ToString(), SiteStrings.Always),
                    new ListItem(C.OnScreenBorderMode.OnMouseOver.ToString(), SiteStrings.OnMouseOver),
                    new ListItem(C.OnScreenBorderMode.Never.ToString(),SiteStrings.Never),
                };
			}
		}

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

		public IEnumerable<ListItem> AllStylesListItems
		{
			get;
			private set;
		}

		
		public IEnumerable<ListItem> AllFormatsListItems
		{
			get;
			private set;
		}

		[LocalizedDisplayName("Styles", NameResourceType = typeof(VisualEditorStrings))]
		public IList<QPCheckedItem> ActiveVeStyles { get; set; }

		[LocalizedDisplayName("Formats", NameResourceType = typeof(VisualEditorStrings))]
		public IList<QPCheckedItem> ActiveVeFormats { get; set; }

		#endregion

		internal void DoCustomBinding()
		{
			if (!String.IsNullOrEmpty(AggregationListItems_Data_ExternalCssItems))
			{
				Data.ExternalCssItems = new JavaScriptSerializer().Deserialize<List<ExternalCss>>(AggregationListItems_Data_ExternalCssItems);
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