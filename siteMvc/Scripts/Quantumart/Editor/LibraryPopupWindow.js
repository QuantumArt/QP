Quantumart.QP8.LibraryPopupWindow = function (eventArgs, options) {
  Object.assign(this._options, options);
  this._eventArgs = eventArgs;
  this._selectPopupWindowComponent = new Quantumart.QP8.BackendSelectPopupWindow(this._eventArgs, this._options);
  this._selectPopupWindowComponent.attachObserver(window.EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED, jQuery.proxy(this._librarySelectedHandler, this));
  this._selectPopupWindowComponent.attachObserver(window.EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED, jQuery.proxy(this._libraryClosedHandler, this));
};

Quantumart.QP8.LibraryPopupWindow.prototype
= {
    _options:
  {
    entityName: '',
    isMultipleEntities: false,
    entities: [],
    isMultiOpen: true,
    contentId: 0
  },
    _eventArgs: null,

    _selectPopupWindowComponent: null,

    setContentId(_contentId) {
      this._options.contentId = _contentId;
    },

    openWindow() {
      this._selectPopupWindowComponent.openWindow();
    },

    closeWindow() {
      this._selectPopupWindowComponent.closeWindow();
    },

    setSelectCallback(callback) {
      this._options.selectCallback = callback;
    },

    _librarySelectedHandler(eventType, sender, args) {
      this.closeWindow();
      if (args) {
        const entities = args.entities;
        if (entities.length > 0) {
          let folderUrl = args.context;
          if (folderUrl.charAt(0) == '\\') {
            folderUrl = folderUrl.substring(1, folderUrl.length);
          }
          let imgUrl = '';
          if (this._options.contentId != 0) {
            imgUrl = `${this._options.libraryUrl.replace('images/', '')}contents/${this._options.contentId}/${folderUrl}${entities[0].Name}`;
          } else {
            imgUrl = this._options.libraryUrl + folderUrl + entities[0].Name;
          }
          imgUrl = imgUrl.replace(new RegExp('\\\\', 'g'), '/');

          if (this._options.selectCallback) {
            this._options.selectCallback(imgUrl);
          }
        }
      }
    },

    _libraryClosedHandler(eventType, sender, args) {
      this.closeWindow();
    },


    dispose() {
      if (this._selectPopupWindowComponent) {
        this._selectPopupWindowComponent.detachObserver(window.EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED);
        this._selectPopupWindowComponent.detachObserver(window.EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED);
        this._selectPopupWindowComponent.closeWindow();
        this._selectPopupWindowComponent.dispose();
        this._selectPopupWindowComponent = null;
      }

      this._eventArgs = null;
      this._options = null;
    }
  };
