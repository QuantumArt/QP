Quantumart.QP8.MultistepActionCopySiteSettings = function (options) {
  this.options = options;
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

  serializeForm() {
    return $(`#${
      this.options._popupWindowComponent._documentWrapperElementId
    } form input, #${
      this.options._popupWindowComponent._documentWrapperElementId
    } form select`).serialize();
  },

  submitForm(ajaxData) {
    const that = this;
    $.ajax({
      url: that._settingsActionUrl.replace('Settings', 'SetupWithParams'),
      data: ajaxData,
      type: 'POST',
      success(response) {
        if (response && response.data) {
          $(`#${that._popupWindowComponent._documentWrapperElementId}`).html(response.data);
        } else {
          that._popupWindowComponent.closeWindow();
          $('.t-overlay').remove();
          that._callback({ isSettingsSet: true });
        }
      }
    });
  },

  dispose() {
    // empty fn
  }
};
