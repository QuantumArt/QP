import { BackendDocumentHostStateStorage } from '../Document/BackendDocumentHostStateStorage';
import { BackendPopupWindow } from '../Document/BackendPopupWindow';
import { Observable } from '../Common/Observable';
import { $o } from '../Info/BackendEntityObject';
import { $q } from '../Utils';

export class BackendPopupWindowManager extends Observable {
  /** @type {BackendPopupWindowManager} */
  static _instance;

  static getInstance() {
    if (!BackendPopupWindowManager._instance) {
      BackendPopupWindowManager._instance = new BackendPopupWindowManager();
    }

    return BackendPopupWindowManager._instance;
  }

  static destroyInstance() {
    if (BackendPopupWindowManager._instance) {
      BackendPopupWindowManager._instance.dispose();
      BackendPopupWindowManager._instance = null;
    }
  }

  constructor() {
    super();
    /** @type {{ [x: string]: BackendPopupWindow }} */
    this._popupWindows = {};
    this._hostStateStorage = new BackendDocumentHostStateStorage();
  }

  generatePopupWindowId() {
    const popupWindowNums = Object.keys(this._popupWindows).map(value => {
      const matches = value.match('^win([0-9]+)$');
      return matches && matches.length === 2 ? matches[1] : 0;
    });

    const maxNumber = popupWindowNums.length > 0 ? Math.max.apply(null, popupWindowNums) : 0;
    return String.format('win{0}', maxNumber + 1);
  }

  getAllPopupWindows() {
    return Object.values(this._popupWindows);
  }

  getPopupWindow(popupWindowId) {
    if (this._popupWindows[popupWindowId]) {
      return this._popupWindows[popupWindowId];
    }

    return undefined;
  }

  getPopupWindowByEventArgs(eventArgs) {
    return $.grep(this.getAllPopupWindows(), win =>
      win.get_entityTypeCode() === eventArgs.get_entityTypeCode()
      && win.get_entityId() === eventArgs.get_entityId()
      && win.get_actionCode() === eventArgs.get_actionCode()
    );
  }

  createPopupWindow(eventArgs, options) {
    let popupWindowId = options ? options.popupWindowId : '';
    if ($q.isNullOrWhiteSpace(popupWindowId)) {
      popupWindowId = this.generatePopupWindowId();
    }

    if (this._popupWindows[popupWindowId]) {
      $q.alertError($l.PopupWindow.popupWindowIdNotUniqueErrorMessage);
      throw new Error($l.PopupWindow.popupWindowIdNotUniqueErrorMessage);
    }

    const newOptions = Object.assign({}, options, {
      width: eventArgs.get_windowWidth(),
      height: eventArgs.get_windowHeight(),
      hostStateStorage: this._hostStateStorage
    });

    const popupWindow = new BackendPopupWindow(popupWindowId, eventArgs, newOptions);
    popupWindow.set_popupWindowManager(this);
    popupWindow.initialize();

    this._popupWindows[popupWindowId] = popupWindow;
    return popupWindow;
  }

  openPopupWindow(eventArgs) {
    const popupWindow = this.createPopupWindow(eventArgs, {});
    popupWindow.openWindow();
    return popupWindow;
  }

  closeNotExistentPopupWindows(isCompleteAction = false) {
    const popupWindows = this.getAllPopupWindows();
    for (let popupWindowIndex = 0; popupWindowIndex < popupWindows.length; popupWindowIndex++) {
      const popupWindow = popupWindows[popupWindowIndex];
      if (isCompleteAction) {
        popupWindow.set_closeWithoutCheck(true);
      }
      const entityTypeCode = popupWindow.get_entityTypeCode();
      const entityId = popupWindow.get_entityId();
      const actionTypeCode = popupWindow.get_actionTypeCode();
      const isMultipleEntities = popupWindow.get_isMultipleEntities();
      if (actionTypeCode !== window.ACTION_TYPE_CODE_ADD_NEW) {
        if (isMultipleEntities && actionTypeCode !== window.ACTION_TYPE_CODE_MULTIPLE_SELECT) {
          const entities = popupWindow.get_entities();
          for (let entityIndex = 0; entityIndex < entities.length; entityIndex++) {
            const entity = entities[entityIndex];

            // eslint-disable-next-line max-depth
            if (entity && !$o.checkEntityExistence(entityTypeCode, entity.Id)) {
              popupWindow.closeWindow();
              break;
            }
          }
        } else if (entityTypeCode === window.ENTITY_TYPE_CODE_ARTICLE_EXTERNAL_WORKFLOW
          || !$o.checkEntityExistence(entityTypeCode, entityId)) {
          popupWindow.closeWindow();
        }
      }
    }
  }

  onActionExecuted(eventArgs) {
    if (eventArgs.get_isRemoving() || eventArgs.get_isArchiving() || eventArgs.get_isRestoring()
      || eventArgs.get_actionTypeCode() === window.ACTION_TYPE_CODE_COMPLETE
    ) {
      this.closeNotExistentPopupWindows(eventArgs.get_actionTypeCode() === window.ACTION_TYPE_CODE_COMPLETE);
    }
  }

  onNeedUp(eventArgs, popupWindowId) {
    const popupWindow = this.getPopupWindow(popupWindowId);
    if (popupWindow) {
      popupWindow.closeWindow();
    }
  }

  hostExternalCallerContextsUnbinded(unbindingEventArgs) {
    this.notify(window.EVENT_TYPE_HOST_EXTERNAL_CALLER_CONTEXTS_UNBINDED, unbindingEventArgs);
  }

  removePopupWindow(popupWindowId) {
    $q.removeProperty(this._popupWindows, popupWindowId);
  }

  destroyPopupWindow(popupWindowId) {
    const popupWindow = this._popupWindows[popupWindowId];
    if (popupWindow && popupWindow.dispose) {
      popupWindow.dispose();
    }
  }

  dispose() {
    super.dispose();
    if (this._popupWindows) {
      Object.keys(this._popupWindows).forEach(this.destroyPopupWindow, this);
    }

    $q.collectGarbageInIE();
  }
}

Quantumart.QP8.BackendPopupWindowManager = BackendPopupWindowManager;
