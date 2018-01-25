import { $c } from '../../ControlHelpers';

export class MultistepActionExportSettings {
  constructor(options) {
    this.options = options;
  }

  initActions() {
    const fieldValues = this.options.popupWindowComponent.loadHostState();
    const $root = $(`#${this.options.popupWindowId}_editingForm`);
    $c.setAllBooleanValues($root, fieldValues);
    $c.setAllRadioListValues($root, fieldValues);
    $c.initAllCheckboxToggles($root);
    $c.initAllEntityDataLists($root);
    $c.setAllEntityDataListValues($root, fieldValues);
  }

  validate() {
    const $root = $(`#${this.options.popupWindowId}_editingForm`);
    const fieldValues = $c.getAllFieldValues($root);
    this.options.popupWindowComponent.saveHostState(fieldValues);
    return '';
  }

  serializeForm() {
    const id = this.options.popupWindowId;
    const parentContainer = document.getElementById(`${id}_editingForm`);
    const ajaxData = {
      Encoding: +document.getElementById(`${id}_Encoding`).value,
      Culture: +document.getElementById(`${id}_Culture`).value,
      Delimiter: +parentContainer.querySelector(`input[name="Delimiter"]:checked`).value,
      LineSeparator: +document.getElementById(`${id}_LineSeparator`).value,
      OrderByField: document.getElementById(`${id}_OrderByField`).value,
      AllFields: document.getElementById(`${id}_AllFields`).checked,
      ExcludeSystemFields: document.getElementById(`${id}_ExcludeSystemFields`).checked,
      CustomFields: Array
        .from(parentContainer.querySelectorAll(`input[name="CustomFields"]:checked`))
        .map(el => +el.value),
      FieldsToExpand: Array
        .from(parentContainer.querySelectorAll(`input[name="FieldsToExpand"]:checked`))
        .map(el => +el.value)
    };

    const idsElement = document.getElementById(`${id}_idsToExport`);
    if (idsElement) {
      Object.assign(ajaxData, {
        ids: idsElement.getAttribute('data-ids').split(',').map(el => +el)
      });
    }

    return ajaxData;
  }

  dispose() {
    const $root = $(`#${this.options.popupWindowId}_editingForm`);
    $c.destroyAllEntityDataLists($root);
    $c.destroyAllCheckboxToggles($root);
  }
}


MultistepActionExportSettings.addButtons = function (dataItems) {
  const exportButton = {
    Type: window.TOOLBAR_ITEM_TYPE_BUTTON,
    Value: 'Export',
    Text: $l.MultistepAction.exportTitle,
    Tooltip: $l.MultistepAction.exportTitle,
    AlwaysEnabled: false,
    Icon: 'action.gif'
  };

  return dataItems.concat(exportButton);
};


Quantumart.QP8.MultistepActionExportSettings = MultistepActionExportSettings;
