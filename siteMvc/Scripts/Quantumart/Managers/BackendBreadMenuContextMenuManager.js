Quantumart.QP8.BackendBreadMenuContextMenuManager = function () {
  Quantumart.QP8.BackendBreadMenuContextMenuManager.initializeBase(this);
};

Quantumart.QP8.BackendBreadMenuContextMenuManager.prototype = {
  _сontextMenus: {},
  getContextMenu(contextMenuCode) {
    let contextMenu = null;
    if (this._сontextMenus[contextMenuCode]) {
      contextMenu = this._сontextMenus[contextMenuCode];
    }

    return contextMenu;
  },

  createContextMenu(contextMenuCode, contextMenuElementId, options) {
    const contextMenu = new Quantumart.QP8.BackendContextMenu(contextMenuCode, contextMenuElementId, options);
    contextMenu.set_contextMenuManager(this);
    contextMenu.initialize();

    this._сontextMenus[contextMenuCode] = contextMenu;
    return contextMenu;
  },

  removeContextMenu(contextMenuCode) {
    $q.removeProperty(this._сontextMenus, contextMenuCode);
  },

  destroyContextMenu(contextMenuCode) {
    let contextMenu = this._сontextMenus[contextMenuCode];
    if (contextMenu != null) {
      if (contextMenu.dispose) {
        contextMenu.dispose();
      }

      contextMenu = null;
    }
  },

  getContextMenuEventType() {
    return jQuery.fn.jeegoocontext.getContextMenuEventType();
  },

  dispose() {
    Quantumart.QP8.BackendBreadMenuContextMenuManager.callBaseMethod(this, 'dispose');
    if (this._сontextMenus) {
      for (const contextMenuCode in this._сontextMenus) {
        this.destroyContextMenu(contextMenuCode);
      }

      this._сontextMenus = null;
    }

    Quantumart.QP8.BackendBreadMenuContextMenuManager._instance = null;
    $q.collectGarbageInIE();
  }
};


Quantumart.QP8.BackendBreadMenuContextMenuManager._instance = null;
Quantumart.QP8.BackendBreadMenuContextMenuManager.getInstance = function () {
  if (Quantumart.QP8.BackendBreadMenuContextMenuManager._instance == null) {
    Quantumart.QP8.BackendBreadMenuContextMenuManager._instance = new Quantumart.QP8.BackendBreadMenuContextMenuManager();
  }

  return Quantumart.QP8.BackendBreadMenuContextMenuManager._instance;
};

Quantumart.QP8.BackendBreadMenuContextMenuManager.destroyInstance = function () {
  if (Quantumart.QP8.BackendBreadMenuContextMenuManager._instance) {
    Quantumart.QP8.BackendBreadMenuContextMenuManager._instance.dispose();
  }
};

Quantumart.QP8.BackendBreadMenuContextMenuManager.registerClass('Quantumart.QP8.BackendBreadMenuContextMenuManager', Quantumart.QP8.Observable);
