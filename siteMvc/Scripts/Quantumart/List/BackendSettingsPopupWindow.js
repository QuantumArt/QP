Quantumart.QP8.BackendSettingsPopupWindow = function BackendSettingsPopupWindow(eventArgs, options, callback) {
  Quantumart.QP8.BackendSettingsPopupWindow.initializeBase(this, [eventArgs, options]);
  this._eventsArgs = eventArgs;
  this._actionCode = eventArgs.get_actionCode();
  this._settingsActionUrl = eventArgs.settingsActionUrl;

  if (options) {
    this._isSettingsSet = options.isSettingsSet;
    this._contentId = options.ContentId;
  }

  this._callback = callback;

  this._initializeSettingsWindow();
  this._popupWindowToolbarComponent = this._createToolbar();
  this.openWindow();

  this._settingsWindow.initActions(this, options);
};

Quantumart.QP8.BackendSettingsPopupWindow.prototype = {
  _contentId: 0,
  _eventsArgs: null,
  _isSettingsSet: false,
  _callback: null,
  _settingsWindow: null,
  _settingsActionUrl: null,
  NEXT_BUTTON: 'Next',

  get SettingsClass() {
    switch (this._actionCode) {
      case 'import_articles':
        return Quantumart.QP8.MultistepActionImportSettings;
      case 'export_articles':
      case 'multiple_export_article':
      case 'export_virtual_articles':
      case 'multiple_export_virtual_article':
        return Quantumart.QP8.MultistepActionExportSettings;
      case 'copy_site':
        return Quantumart.QP8.MultistepActionCopySiteSettings;
      default:
        return null;
    }
  },

  _initializeSettingsWindow() {
    const options = {
      popupWindowComponent: this._popupWindowComponent,
      popupWindowId: this._popupWindowId,
      actionCode: this._actionCode,
      parentEntityId: this._parentEntityId,
      contentId: this._contentId,
      wrapperElementId: this._popupWindowComponent.get_documentWrapperElementId()
    };

    if (this.SettingsClass) {
      this._settingsWindow = new this.SettingsClass(options);
    }
  },

  _createToolbar() {
    const backendToolbar = new Quantumart.QP8.BackendToolbar();
    backendToolbar.set_toolbarElementId(`popupWindowToolbar_${this._popupWindowId}`);
    backendToolbar.initialize();
    backendToolbar.attachObserver(
      window.EVENT_TYPE_TOOLBAR_BUTTON_CLICKED,
      $.proxy(this._onPopupWindowToolbarButtonClicked, this)
    );

    backendToolbar.addToolbarItemsToToolbar(this._getToolbarItems());
    return backendToolbar;
  },

  openWindow() {
    if (this._popupWindowComponent) {
      this._popupWindowComponent.openWindow({ hasSettings: true, isSettingsSet: this._isSettingsSet, asyncReq: false });
    }
  },

  _getToolbarItems() {
    return this.SettingsClass.addButtons([]);
  },

  _onPopupWindowToolbarButtonClicked() {
    if (this._popupWindowComponent) {
      const errors = this._settingsWindow.validate();
      if (errors.length) {
        $q.alertError(errors);
      } else {
        this._isSettingsSet = true;
        const ajaxData = this._settingsWindow.serializeForm();
        this.submitForm(ajaxData);
      }
    }
  },

  submitForm(ajaxData) {
    const url = this._settingsActionUrl.replace('Settings', 'SetupWithParams');
    const callback = response => {
      if (response && response.data) {
        $(`#${this._popupWindowComponent.get_documentWrapperElementId()}`).html(response.data);
      } else {
        this._popupWindowComponent.closeWindow();
        $('.t-overlay').remove();
        this._callback({ isSettingsSet: true });
      }
    };

    if (this.SettingsClass === Quantumart.QP8.MultistepActionImportSettings) {
      $.ajax({
        url,
        data: ajaxData,
        type: 'POST',
        success: callback
      });
    } else {
      $q.postAjax(url, ajaxData, callback);
    }
  },

  _popupWindowClosedHandler() {
    this._settingsWindow.dispose();
    Quantumart.QP8.BackendSettingsPopupWindow.callBaseMethod(this, 'dispose');
  }
};

Quantumart.QP8.BackendSettingsPopupWindow.registerClass(
  'Quantumart.QP8.BackendSettingsPopupWindow',
  Quantumart.QP8.BackendSelectPopupWindow
);
