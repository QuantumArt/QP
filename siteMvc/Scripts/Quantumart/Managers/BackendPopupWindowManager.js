Quantumart.QP8.BackendPopupWindowManager = function () {
  Quantumart.QP8.BackendPopupWindowManager.initializeBase(this);
  this._hostStateStorage = new Quantumart.QP8.BackendDocumentHostStateStorage();

  // eslint-disable-next-line camelcase
  this._hostStateStorage._get_host_key = () => {
    const originalFn = this._hostStateStorage._get_host_key;
    return hostParams => originalFn(Object.assign({}, hostParams, {
      entityId: 0
    }));
  };
};

Quantumart.QP8.BackendPopupWindowManager.prototype = {
  _popupWindows: {},
  _hostStateStorage: null,

  generatePopupWindowId() {
    const popupWindowNums = Object.keys(this._popupWindows).map(value => {
      const matches = value.match('^win([0-9]+)$');
      return matches && matches.length === 2 ? matches[1] : 0;
    });

    const maxNumber = popupWindowNums.length > 0 ? Math.max.apply(null, popupWindowNums) : 0;
    return String.format('win{0}', maxNumber + 1);
  },

  generatePopupWindowTitle(eventArgs) {
    const popupWindowTitle = Quantumart.QP8.BackendDocumentHost.generateTitle(eventArgs, { isTab: false });
    return popupWindowTitle;
  },

  getAllPopupWindows() {
    const popupWindowsHash = this._popupWindows;
    return Object.values(popupWindowsHash);
  },

  getPopupWindow(popupWindowId) {
    let popupWindow = null;
    if (this._popupWindows[popupWindowId]) {
      popupWindow = this._popupWindows[popupWindowId];
    }

    return popupWindow;
  },

  getPopupWindowByEventArgs(eventArgs) {
    return jQuery.grep(this.getAllPopupWindows(), w =>
      w.get_entityTypeCode() === eventArgs.get_entityTypeCode()
      && w.get_entityId() === eventArgs.get_entityId()
      && w.get_actionCode() === eventArgs.get_actionCode()
    );
  },

  createPopupWindow(eventArgs, options) {
    let popupWindowId = options ? options.popupWindowId : '';
    if ($q.isNullOrWhiteSpace(popupWindowId)) {
      popupWindowId = this.generatePopupWindowId();
    }

    if (this._popupWindows[popupWindowId]) {
      window.alert($l.PopupWindow.popupWindowIdNotUniqueErrorMessage);
      return null;
    }

    const newOptions = Object.assign({}, options, {
      width: eventArgs.get_windowWidth(),
      height: eventArgs.get_windowHeight(),
      hostStateStorage: this._hostStateStorage
    });

    const popupWindow = new Quantumart.QP8.BackendPopupWindow(popupWindowId, eventArgs, newOptions);
    popupWindow.set_popupWindowManager(this);
    popupWindow.initialize();

    this._popupWindows[popupWindowId] = popupWindow;
    return popupWindow;
  },

  openPopupWindow(eventArgs) {
    const popupWindow = this.createPopupWindow(eventArgs, {});
    popupWindow.openWindow();
    return popupWindow;
  },

  removePopupWindow(popupWindowId) {
    $q.removeProperty(this._popupWindows, popupWindowId);
  },

  destroyPopupWindow(popupWindowId) {
    const popupWindow = this._popupWindows[popupWindowId];
    if (popupWindow && popupWindow.dispose) {
      popupWindow.dispose();
    }
  },

  closeNotExistentPopupWindows() {
    let popupWindows = this.getAllPopupWindows();
    for (
      let popupWindowIndex = 0, popupWindowCount = popupWindows.length;
      popupWindowIndex < popupWindowCount;
      popupWindowIndex++
    ) {
      let popupWindow = popupWindows[popupWindowIndex];
      const entityTypeCode = popupWindow.get_entityTypeCode();
      const entityId = popupWindow.get_entityId();
      const actionTypeCode = popupWindow.get_actionTypeCode();
      const isMultipleEntities = popupWindow.get_isMultipleEntities();
      if (actionTypeCode !== window.ACTION_TYPE_CODE_ADD_NEW) {
        if (isMultipleEntities && actionTypeCode !== window.ACTION_TYPE_CODE_MULTIPLE_SELECT) {
          const entities = popupWindow.get_entities();
          for (let entityIndex = 0; entityIndex < entities.length; entityIndex++) {
            const entity = entities[entityIndex];
            if (entity) {
              const entityExist = $o.checkEntityExistence(entityTypeCode, entity.Id);
              if (!entityExist) {
                popupWindow.closeWindow();
                break;
              }
            }
          }
        } else {
          const entityExist = $o.checkEntityExistence(entityTypeCode, entityId);
          if (!entityExist) {
            popupWindow.closeWindow();
          }
        }
      }

      popupWindow = null;
    }

    popupWindows = null;
  },

  onActionExecuted(eventArgs) {
    if (eventArgs.get_isRemoving() || eventArgs.get_isArchiving() || eventArgs.get_isRestoring()) {
      this.closeNotExistentPopupWindows();
    }
  },

  onNeedUp(eventArgs, popupWindowId) {
    const popupWindow = this.getPopupWindow(popupWindowId);
    if (popupWindow) {
      popupWindow.closeWindow();
    }
  },

  hostExternalCallerContextsUnbinded(unbindingEventArgs) {
    this.notify(window.EVENT_TYPE_HOST_EXTERNAL_CALLER_CONTEXTS_UNBINDED, unbindingEventArgs);
  },

  dispose() {
    Quantumart.QP8.BackendPopupWindowManager.callBaseMethod(this, 'dispose');

    if (this._popupWindows) {
      Object.keys(this._popupWindows).forEach(this.destroyPopupWindow);
    }

    Quantumart.QP8.BackendPopupWindowManager._instance = null;
    $q.collectGarbageInIE();
  }
};

Quantumart.QP8.BackendPopupWindowManager._instance = null;
Quantumart.QP8.BackendPopupWindowManager.getInstance = function (options) {
  if (!Quantumart.QP8.BackendPopupWindowManager._instance) {
    Quantumart.QP8.BackendPopupWindowManager._instance = new Quantumart.QP8.BackendPopupWindowManager(options);
  }

  return Quantumart.QP8.BackendPopupWindowManager._instance;
};

Quantumart.QP8.BackendPopupWindowManager.destroyInstance = function () {
  if (Quantumart.QP8.BackendPopupWindowManager._instance) {
    Quantumart.QP8.BackendPopupWindowManager._instance.dispose();
  }
};

Quantumart.QP8.BackendPopupWindowManager.registerClass(
  'Quantumart.QP8.BackendPopupWindowManager',
  Quantumart.QP8.Observable
);
