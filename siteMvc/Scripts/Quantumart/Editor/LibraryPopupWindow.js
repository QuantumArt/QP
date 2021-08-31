import { BackendSelectPopupWindow } from '../List/BackendSelectPopupWindow';

export class LibraryPopupWindow {
  constructor(eventArgs, options) {
    Object.assign(this._options, options);

    this._eventArgs = eventArgs;
    this._selectPopupWindowComponent = new BackendSelectPopupWindow(this._eventArgs, this._options);
    this._selectPopupWindowComponent.attachObserver(
      window.EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED, jQuery.proxy(this._librarySelectedHandler, this)
    );

    this._selectPopupWindowComponent.attachObserver(
      window.EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED, jQuery.proxy(this._libraryClosedHandler, this)
    );
  }

  _options = {
    entityName: '',
    isMultipleEntities: false,
    entities: [],
    isMultiOpen: true,
    contentId: 0
  }
  _eventArgs = null;
  _selectPopupWindowComponent = null;

  setContentId(_contentId) {
    this._options.contentId = _contentId;
  }

  openWindow() {
    this._selectPopupWindowComponent.openWindow();
  }

  closeWindow() {
    this._selectPopupWindowComponent.closeWindow();
  }

  setSelectCallback(callback) {
    this._options.selectCallback = callback;
  }

  _librarySelectedHandler(eventType, sender, args) {
    this.closeWindow();
    const sep = window.DIRECTORY_SEPARATOR_CHAR;
    if (args) {
      const { entities } = args;
      if (entities.length > 0) {
        let folderUrl = args.context;
        if (folderUrl.charAt(0) === sep) {
          folderUrl = folderUrl.substring(1, folderUrl.length);
        }
        let imgUrl = '';
        if (this._options.contentId === 0) {
          imgUrl = this._options.libraryUrl + folderUrl + entities[0].Name;
        } else {
          const libraryUrl = this._options.libraryUrl.replace('images/', '');
          imgUrl = `${libraryUrl}contents/${this._options.contentId}/${folderUrl}${entities[0].Name}`;
        }
        const re = new RegExp(sep + sep, 'g');
        imgUrl = imgUrl.replace(re, '/');
        if (this._options.selectCallback) {
          this._options.selectCallback(imgUrl);
        }
      }
    }
  }

  _libraryClosedHandler() {
    this.closeWindow();
  }


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
}


Quantumart.QP8.LibraryPopupWindow = LibraryPopupWindow;
