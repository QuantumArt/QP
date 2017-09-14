Quantumart.QP8.BackendBreadMenuContextMenuManager = function () {
  Quantumart.QP8.BackendBreadMenuContextMenuManager.initializeBase(this);
};

Quantumart.QP8.BackendBreadMenuContextMenuManager.prototype = {
  _сontextMenus: {},
  getContextMenu: function getContextMenu(contextMenuCode) {
    let contextMenu = null;
    if (this._сontextMenus[contextMenuCode]) {
      contextMenu = this._сontextMenus[contextMenuCode];
    }

    return contextMenu;
  },

  createContextMenu: function createContextMenu(contextMenuCode, contextMenuElementId, options) {
    const contextMenu = new Quantumart.QP8.BackendContextMenu(contextMenuCode, contextMenuElementId, options);
    contextMenu.set_contextMenuManager(this);
    contextMenu.initialize();

    this._сontextMenus[contextMenuCode] = contextMenu;
    return contextMenu;
  },

  removeContextMenu: function removeContextMenu(contextMenuCode) {
    $q.removeProperty(this._сontextMenus, contextMenuCode);
  },

  destroyContextMenu: function destroyContextMenu(contextMenuCode) {
    const contextMenu = this._сontextMenus[contextMenuCode];
    if (!$q.isNull(contextMenu) && contextMenu.dispose) {
      contextMenu.dispose();
    }
  },

  getContextMenuEventType: function getContextMenuEventType() {
    return jQuery.fn.jeegoocontext.getContextMenuEventType();
  },

  dispose: function dispose() {
    Quantumart.QP8.BackendBreadMenuContextMenuManager.callBaseMethod(this, 'dispose');
    if (this._сontextMenus) {
      Object.keys(this._сontextMenus).forEach(code => this.destroyContextMenu(code));
      this._сontextMenus = null;
    }

    Quantumart.QP8.BackendBreadMenuContextMenuManager._instance = null;
    $q.collectGarbageInIE();
  }
};


Quantumart.QP8.BackendBreadMenuContextMenuManager._instance = null;
Quantumart.QP8.BackendBreadMenuContextMenuManager.getInstance = function getInstance() {
  if (Quantumart.QP8.BackendBreadMenuContextMenuManager._instance === null) {
    Quantumart.QP8.BackendBreadMenuContextMenuManager._instance
    = new Quantumart.QP8.BackendBreadMenuContextMenuManager();
  }

  return Quantumart.QP8.BackendBreadMenuContextMenuManager._instance;
};

Quantumart.QP8.BackendBreadMenuContextMenuManager.destroyInstance = function destroyInstance() {
  if (Quantumart.QP8.BackendBreadMenuContextMenuManager._instance) {
    Quantumart.QP8.BackendBreadMenuContextMenuManager._instance.dispose();
  }
};

Quantumart.QP8.BackendBreadMenuContextMenuManager.registerClass(
  'Quantumart.QP8.BackendBreadMenuContextMenuManager', Quantumart.QP8.Observable
);
