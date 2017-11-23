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

  _initializeSettingsWindow() {
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
    return this._settingsWindow.addButtons([]);
  },

  _onPopupWindowToolbarButtonClicked() {
    if (this._popupWindowComponent) {
      const errors = this._settingsWindow.validate();
      if (errors.length) {
        $q.alertError(errors);
      } else {
        this._isSettingsSet = true;
        const parentContainer = document.getElementById(`${this._popupWindowId}_editingForm`);
        const ajaxData = {
          Encoding: +document.getElementById(`${this._popupWindowId}_Encoding`).value,
          Culture: +document.getElementById(`${this._popupWindowId}_Culture`).value,
          Delimiter: +parentContainer.querySelector(`input[name="Delimiter"]:checked`).value,
          LineSeparator: +document.getElementById(`${this._popupWindowId}_LineSeparator`).value,
          OrderByField: document.getElementById(`${this._popupWindowId}_OrderByField`).value,
          AllFields: document.getElementById(`${this._popupWindowId}_AllFields`).checked,
          ExcludeSystemFields: document.getElementById(`${this._popupWindowId}_ExcludeSystemFields`).checked,
          CustomFields: [
            ...parentContainer.querySelectorAll(`input[name="CustomFields"]:checked`)
          ].map(el => +el.value),
          FieldsToExpand: [
            ...parentContainer.querySelectorAll(`input[name="FieldsToExpand"]:checked`)
          ].map(el => +el.value)
        };

        const idsElement = document.getElementById(`${this._popupWindowId}_idsToExport`);
        if (idsElement) {
          Object.assign(ajaxData, {
            ids: idsElement.getAttribute('data-ids').split(',').map(el => +el.value)
          });
        }

        $q.postAjax(this._settingsActionUrl.replace('Settings', 'SetupWithParams'), ajaxData, data => {
          if (data && data.view) {
            $(`#${this._popupWindowComponent._documentWrapperElementId}`).html(data.view);
          } else {
            this._popupWindowComponent.closeWindow();
            $('.t-overlay').remove();
            this._callback({ isSettingsSet: true });
          }
        });
      }
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
