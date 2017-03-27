Quantumart.QP8.BackendSettingsPopupWindow = function BackendSettingsPopupWindow(eventArgs, options, callback) {
  this._eventsArgs = eventArgs;
  this._actionCode = eventArgs.get_actionCode();
  this._settingsActionUrl = eventArgs.settingsActionUrl;
  if (options) {
    this._isSettingsSet = options.isSettingsSet;
    this._contentId = options.ContentId;
  }

  this._callback = callback;
  this._initializeSettingsWindow();
  Quantumart.QP8.BackendSettingsPopupWindow.initializeBase(this, [eventArgs, options]);

  this._popupWindowToolbarComponent = this._createToolbar();
  this.openWindow();
  this._settingsWindow.InitActions(this, options);
};

Quantumart.QP8.BackendSettingsPopupWindow.prototype = {
  _contentId: 0,
  _eventsArgs: null,
  _isSettingsSet: false,
  _callback: null,
  _settingsWindow: null,
  _settingsActionUrl: null,
  NEXT_BUTTON: 'Next',

  _initializeSettingsWindow: function () {
    switch (this._actionCode) {
      case 'import_articles':
        this._settingsWindow = new Quantumart.QP8.MultistepActionImportSettings(this);
        break;
      case 'export_articles':
      case 'multiple_export_article':
      case 'export_virtual_articles':
      case 'multiple_export_virtual_article':
        this._settingsWindow = new Quantumart.QP8.MultistepActionExportSettings(this);
        break;
      case 'copy_site':
        this._settingsWindow = new Quantumart.QP8.MultistepActionCopySiteSettings();
        break;
      default:
        break;
    }
  },

  _createToolbar: function () {
    var toolbar = new Quantumart.QP8.BackendToolbar();
    toolbar.set_toolbarElementId('popupWindowToolbar_' + this._popupWindowId);
    toolbar.initialize();
    toolbar.attachObserver(window.EVENT_TYPE_TOOLBAR_BUTTON_CLICKED, jQuery.proxy(this._onPopupWindowToolbarButtonClicked, this));
    toolbar.addToolbarItemsToToolbar(this._getToolbarItems());
    return toolbar;
  },

  openWindow: function () {
    if (this._popupWindowComponent) {
      this._popupWindowComponent.openWindow({ hasSettings: true, isSettingsSet: this._isSettingsSet, asyncReq: false });
    }
  },

  _getToolbarItems: function () {
    var dataItems = [];
    dataItems = this._settingsWindow.AddButtons(dataItems);
    return dataItems;
  },

  _onPopupWindowToolbarButtonClicked: function (eventType, sender) {
    if (this._popupWindowComponent) {
      var options = jQuery.extend(this._eventsArgs, sender);
      var errors = this._settingsWindow.Validate();
      if (errors.length === 0) {
        var btn = $('#' + sender._toolbarElementId + '> ul > li');
        var className = 'disabled';
        options.isSettingsSet = true;
        var prms = $('#' + this._popupWindowComponent._documentWrapperElementId + ' form input, #' + this._popupWindowComponent._documentWrapperElementId + ' form select').serialize();
        var that = this;
        $.ajax({
          url: that._settingsActionUrl.replace('Settings', 'SetupWithParams'),
          data: prms,
          type: 'POST',
          beforeSend: function () {
            btn.addClass(className);
          },
          complete: function () {
            btn.removeClass(className);
          },
          success: function (data) {
            if (data.view == null) {
              that._popupWindowComponent.closeWindow();
              $('.t-overlay').remove();
              that._callback({ isSettingsSet: true });
            } else {
              $('#' + that._popupWindowComponent._documentWrapperElementId).html(data.view);
            }
          }
        });
      } else {
        alert(errors);
      }
    }
  },

  _popupWindowClosedHandler: function () {
    this._settingsWindow.dispose();
    Quantumart.QP8.BackendSettingsPopupWindow.callBaseMethod(this, 'dispose');
  }
};

Quantumart.QP8.BackendSettingsPopupWindow.registerClass('Quantumart.QP8.BackendSettingsPopupWindow', Quantumart.QP8.BackendSelectPopupWindow);
