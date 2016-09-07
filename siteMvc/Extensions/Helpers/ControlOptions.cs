using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public class ControlOptions
    {
        public ControlOptions()
        {
            Enabled = true;
            HtmlAttributes = new Dictionary<string, object>();
        }

        public bool Enabled { get; set; }

        public Dictionary<string, object> HtmlAttributes { get; set; }

        public void SetMultiplePickerOptions(string name, string id, EntityDataListArgs eventArgs)
        {
            HtmlAttributes["id"] = (HtmlAttributes.ContainsKey("id") ? HtmlAttributes["id"].ToString() : id) + "_list";
            HtmlAttributes.AddCssClass(HtmlHelpersExtensions.MULTIPLE_ITEM_PICKER_CLASS_NAME);
            HtmlAttributes.AddCssClass(HtmlHelpersExtensions.SELF_CLEAR_FLOATS_CLASS_NAME);
            if (!Enabled)
            {
                HtmlAttributes.AddCssClass(HtmlHelpersExtensions.DISABLED_CLASS_NAME);
            }

            HtmlAttributes.AddData("count_limit", QPConfiguration.WebConfigSection.RelationCountLimit);
            SetDataListOptions(name, eventArgs);
        }

        public void SetDropDownOptions(string name, string id, IList<QPSelectListItem> list, EntityDataListArgs entityDataListArgs)
        {
            if (!HtmlAttributes.ContainsKey("id"))
            {
                HtmlAttributes.Add("id", id);
            }

            HtmlAttributes.AddCssClass(HtmlHelpersExtensions.DROP_DOWN_LIST_CLASS_NAME);
            if (list.Where(i => i.HasDependentItems).ToList().Count > 0)
            {
                var panelsHash = QPSelectListItem.GetPanelHash(id, list);
                HtmlAttributes.AddData("switch_for", $"{{{panelsHash}}}");
                HtmlAttributes.AddData("is_radio", bool.FalseString.ToLowerInvariant());
            }

            if (!Enabled)
            {
                HtmlAttributes.AddCssClass(HtmlHelpersExtensions.DISABLED_CLASS_NAME);
                if (!HtmlAttributes.ContainsKey("disabled"))
                {
                    HtmlAttributes.Add("disabled", "disabled");
                    HtmlAttributes.AddData("list_enabled", "false");
                }
            }

            SetDataListOptions(name, entityDataListArgs);
        }

        public void SetSinglePickerOptions(string name, string id, EntityDataListArgs entityDataListArgs, bool ignoreIdSet = false)
        {
            if (!ignoreIdSet)
            {
                HtmlAttributes["id"] = id;
            }
            else
            {
                HtmlAttributes.Add("data-bind", "value: " + name + "Id," + " attr: {id: '" + id + "'+$index(), name:'" + id + "'+$index()}");
            }

            HtmlAttributes.AddCssClass(HtmlHelpersExtensions.SINGLE_ITEM_PICKER_CLASS_NAME);
            HtmlAttributes.AddCssClass(HtmlHelpersExtensions.SELF_CLEAR_FLOATS_CLASS_NAME);
            if (!Enabled)
            {
                HtmlAttributes.AddCssClass(HtmlHelpersExtensions.DISABLED_CLASS_NAME);
            }

            SetDataListOptions(name, entityDataListArgs);
        }

        public void SetCheckBoxListOptions(string name, string id, IEnumerable<QPSelectListItem> list, RepeatDirection repeatDirection, EntityDataListArgs entityDataListArgs)
        {
            if (!HtmlAttributes.ContainsKey("id"))
            {
                HtmlAttributes.Add("id", id);
            }

            HtmlAttributes.AddCssClass(HtmlHelpersExtensions.CHECKBOXS_LIST_CLASS_NAME);
            HtmlAttributes.AddCssClass(repeatDirection == RepeatDirection.Vertical ? HtmlHelpersExtensions.VERTICAL_DIRECTION_CLASS_NAME : HtmlHelpersExtensions.HORIZONTAL_DIRECTION_CLASS_NAME);
            if (!Enabled)
            {
                HtmlAttributes.AddCssClass(HtmlHelpersExtensions.DISABLED_CLASS_NAME);
            }

            SetDataListOptions(name, entityDataListArgs);
        }

        public void SetListBoxOptions(string name, string id, IEnumerable<QPSelectListItem> list, EntityDataListArgs entityDataListArgs)
        {
            HtmlAttributes.AddCssClass(HtmlHelpersExtensions.LISTBOX_CLASS_NAME);
            if (Enabled)
            {
                HtmlAttributes.AddCssClass(HtmlHelpersExtensions.DISABLED_CLASS_NAME);
                if (!HtmlAttributes.ContainsKey("disabled"))
                {
                    HtmlAttributes.Add("disabled", "disabled");
                }
            }
        }

        public void SetRadioButtonListOptions(string name, string id, IList<QPSelectListItem> list, RepeatDirection repeatDirection, EntityDataListArgs entityDataListArgs)
        {
            HtmlAttributes.AddCssClass(HtmlHelpersExtensions.RADIO_BUTTONS_LIST_CLASS_NAME);
            HtmlAttributes.AddCssClass(repeatDirection == RepeatDirection.Vertical ? HtmlHelpersExtensions.VERTICAL_DIRECTION_CLASS_NAME : HtmlHelpersExtensions.HORIZONTAL_DIRECTION_CLASS_NAME);
            if (!Enabled)
            {
                HtmlAttributes.AddCssClass(HtmlHelpersExtensions.DISABLED_CLASS_NAME);
            }

            if (list.Where(item => item.HasDependentItems).ToList().Count > 0)
            {
                var panelsHash = QPSelectListItem.GetPanelHash(id, list);

                HtmlAttributes.AddData("switch_for", $"{{{panelsHash}}}");
                HtmlAttributes.AddData("is_radio", bool.TrueString.ToLowerInvariant());
            }

            HtmlAttributes.AddData("field_form_name", name);
        }

        private void SetDataListOptions(string name, EntityDataListArgs entityDataListArgs)
        {
            HtmlAttributes.AddCssClass(HtmlHelpersExtensions.DATA_LIST_CLASS_NAME);
            HtmlAttributes.AddData("list_item_name", name);
            if (entityDataListArgs != null)
            {
                HtmlAttributes.AddData("entity_type_code", entityDataListArgs.EntityTypeCode);
                HtmlAttributes.AddData("parent_entity_id", entityDataListArgs.ParentEntityId);
                HtmlAttributes.AddData("entity_id", entityDataListArgs.EntityId);
                if (entityDataListArgs.ListId != 0)
                {
                    HtmlAttributes.AddData("list_id", entityDataListArgs.ListId);
                }

                if (entityDataListArgs.AddNewActionCode != ActionCode.None)
                {
                    HtmlAttributes.AddData("add_new_action_code", entityDataListArgs.AddNewActionCode);
                }

                if (entityDataListArgs.ReadActionCode != ActionCode.None)
                {
                    HtmlAttributes.AddData("read_action_code", entityDataListArgs.ReadActionCode);
                }

                if (entityDataListArgs.SelectActionCode != ActionCode.None)
                {
                    HtmlAttributes.AddData("select_action_code", entityDataListArgs.SelectActionCode);
                }

                if (entityDataListArgs.MaxListWidth != 0)
                {
                    HtmlAttributes.AddData("max_list_width", entityDataListArgs.MaxListWidth);
                }

                if (entityDataListArgs.MaxListHeight != 0)
                {
                    HtmlAttributes.AddData("max_list_height", entityDataListArgs.MaxListHeight);
                }

                if (!Enabled)
                {
                    HtmlAttributes.AddData("list_enabled", "false");
                }

                if (entityDataListArgs.ShowIds)
                {
                    HtmlAttributes.AddData("show_ids", "true");
                }

                if (!string.IsNullOrEmpty(entityDataListArgs.Filter))
                {
                    HtmlAttributes.AddData("filter", entityDataListArgs.Filter);
                }

                if (entityDataListArgs.IsCollapsable)
                {
                    HtmlAttributes.AddData("is_collapsable", "true");
                }

                if (!entityDataListArgs.EnableCopy)
                {
                    HtmlAttributes.AddData("enable_copy", "false");
                }

                if (entityDataListArgs.ReadDataOnInsert)
                {
                    HtmlAttributes.AddData("read_data_on_insert", "true");
                }
            }
        }
    }
}
