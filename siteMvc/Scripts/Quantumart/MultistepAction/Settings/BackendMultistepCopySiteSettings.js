Quantumart.QP8.MultistepActionCopySiteSettings = function (options) {
  this.options = options;
};

Quantumart.QP8.MultistepActionCopySiteSettings.prototype = {

  initActions() {
    // empty fn
  },

  validate() {
    return '';
  },

  serializeForm() {
    const id = this.options.wrapperElementId;
    return $(`#${id} form input, #${id} form select`).serialize();
  },

  dispose() {
    // empty fn
  }
};

Quantumart.QP8.MultistepActionCopySiteSettings.addButtons = function (dataItems) {
  const exportButton = {
    Type: window.TOOLBAR_ITEM_TYPE_BUTTON,
    Value: 'Create like site',
    Text: $l.MultistepAction.createLikeSite,
    Tooltip: $l.MultistepAction.createLikeSite,
    AlwaysEnabled: false,
    Icon: 'action.gif'
  };

  return dataItems.concat(exportButton);
}
