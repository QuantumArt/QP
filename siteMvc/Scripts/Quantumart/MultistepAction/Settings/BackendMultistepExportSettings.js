Quantumart.QP8.MultistepActionExportSettings = function MultistepActionExportSettings(options) {
  this.options = options;
};

Quantumart.QP8.MultistepActionExportSettings.prototype = {
  EXPORT_BUTTON: 'Export',
  addButtons: function (dataItems) {
    var exportButton = {
      Type: window.TOOLBAR_ITEM_TYPE_BUTTON,
      Value: this.EXPORT_BUTTON,
      Text: $l.MultistepAction.exportTitle,
      Tooltip: $l.MultistepAction.exportTitle,
      AlwaysEnabled: false,
      Icon: 'action.gif'
    };

    return dataItems.concat(exportButton);
  },

  initActions: function () {
    var fieldValues = this.options._popupWindowComponent.loadHostState();
    var id = this.options._popupWindowId;
    var $root = $(`#${id}_editingForm`);
    $c.setAllBooleanValues($root, fieldValues);
    $c.setAllRadioListValues($root, fieldValues);
    $c.initAllCheckboxToggles($root);
    $c.initAllEntityDataLists($root);
    $c.setAllEntityDataListValues($root, fieldValues);
  },

  validate: function () {
    var id = this.options._popupWindowId;
    var $root = $(`#${id}_editingForm`);
    var fieldValues = $c.getAllFieldValues($root);
    this.options._popupWindowComponent.saveHostState(fieldValues);
    return '';
  },

  dispose: function () {
    var id = this.options._popupWindowId;
    var $root = $(`#${id}_editingForm`);
    $c.destroyAllEntityDataLists($root);
    $c.destroyAllCheckboxToggles($root);
  }
};
