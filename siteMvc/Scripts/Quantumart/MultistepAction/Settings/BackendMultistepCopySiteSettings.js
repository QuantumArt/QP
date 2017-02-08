Quantumart.QP8.MultistepActionCopySiteSettings = function () {};
Quantumart.QP8.MultistepActionCopySiteSettings.prototype = {
  COPY_BUTTON: "Create like site",
  AddButtons: function (dataItems) {
      var exportButton = {
          Type: TOOLBAR_ITEM_TYPE_BUTTON,
          Value: this.COPY_BUTTON,
          Text: $l.MultistepAction.createLikeSite,
          Tooltip: $l.MultistepAction.createLikeSite,
          AlwaysEnabled: false,
          Icon: "action.gif"
      };

      Array.add(dataItems, exportButton);
      return dataItems;
  },
  InitActions: function (object, options) {
  },
  Validate: function () {
      return '';
  },
  dispose: function () {
  }
}
