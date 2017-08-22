Quantumart.QP8.MultistepActionCopySiteSettings = function () {
  // ctor
};

Quantumart.QP8.MultistepActionCopySiteSettings.prototype = {
  COPY_BUTTON: 'Create like site',
  addButtons(dataItems) {
    const exportButton = {
      Type: window.TOOLBAR_ITEM_TYPE_BUTTON,
      Value: this.COPY_BUTTON,
      Text: $l.MultistepAction.createLikeSite,
      Tooltip: $l.MultistepAction.createLikeSite,
      AlwaysEnabled: false,
      Icon: 'action.gif'
    };

    return dataItems.concat(exportButton);
  },

  initActions() {
    // empty fn
  },

  validate() {
    return '';
  },

  dispose() {
    // empty fn
  }
};
