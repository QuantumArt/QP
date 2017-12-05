Quantumart.QP8.MultistepActionExportSettings = function (options) {
  this.options = options;
};

Quantumart.QP8.MultistepActionExportSettings.prototype = {
  EXPORT_BUTTON: 'Export',
  addButtons(dataItems) {
    const exportButton = {
      Type: window.TOOLBAR_ITEM_TYPE_BUTTON,
      Value: this.EXPORT_BUTTON,
      Text: $l.MultistepAction.exportTitle,
      Tooltip: $l.MultistepAction.exportTitle,
      AlwaysEnabled: false,
      Icon: 'action.gif'
    };

    return dataItems.concat(exportButton);
  },

  initActions() {
    const fieldValues = this.options._popupWindowComponent.loadHostState();
    const $root = $(`#${this.options._popupWindowId}_editingForm`);
    $c.setAllBooleanValues($root, fieldValues);
    $c.setAllRadioListValues($root, fieldValues);
    $c.initAllCheckboxToggles($root);
    $c.initAllEntityDataLists($root);
    $c.setAllEntityDataListValues($root, fieldValues);
  },

  validate() {
    const $root = $(`#${this.options._popupWindowId}_editingForm`);
    const fieldValues = $c.getAllFieldValues($root);
    this.options._popupWindowComponent.saveHostState(fieldValues);
    return '';
  },

  serializeForm() {
    const parentContainer = document.getElementById(`${this.options._popupWindowId}_editingForm`);
    const ajaxData = {
      Encoding: +document.getElementById(`${this.options._popupWindowId}_Encoding`).value,
      Culture: +document.getElementById(`${this.options._popupWindowId}_Culture`).value,
      Delimiter: +parentContainer.querySelector(`input[name="Delimiter"]:checked`).value,
      LineSeparator: +document.getElementById(`${this.options._popupWindowId}_LineSeparator`).value,
      OrderByField: document.getElementById(`${this.options._popupWindowId}_OrderByField`).value,
      AllFields: document.getElementById(`${this.options._popupWindowId}_AllFields`).checked,
      ExcludeSystemFields: document.getElementById(`${this.options._popupWindowId}_ExcludeSystemFields`).checked,
      CustomFields: [
        ...parentContainer.querySelectorAll(`input[name="CustomFields"]:checked`)
      ].map(el => +el.value),
      FieldsToExpand: [
        ...parentContainer.querySelectorAll(`input[name="FieldsToExpand"]:checked`)
      ].map(el => +el.value)
    };

    const idsElement = document.getElementById(`${this.options._popupWindowId}_idsToExport`);
    if (idsElement) {
      Object.assign(ajaxData, {
        ids: idsElement.getAttribute('data-ids').split(',').map(el => +el.value)
      });
    }

    return ajaxData;
  },

  submitForm(ajaxData) {
    $q.postAjax(this._settingsActionUrl.replace('Settings', 'SetupWithParams'), ajaxData, response => {
      if (response && response.data) {
        $(`#${this._popupWindowComponent._documentWrapperElementId}`).html(response.data);
      } else {
        this._popupWindowComponent.closeWindow();
        $('.t-overlay').remove();
        this._callback({ isSettingsSet: true });
      }
    });
  },

  dispose() {
    const $root = $(`#${this.options._popupWindowId}_editingForm`);
    $c.destroyAllEntityDataLists($root);
    $c.destroyAllCheckboxToggles($root);
  }
};
