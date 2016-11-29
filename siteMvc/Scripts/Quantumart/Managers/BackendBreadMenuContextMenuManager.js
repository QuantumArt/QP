Quantumart.QP8.BackendBreadMenuContextMenuManager = function () {
  Quantumart.QP8.BackendBreadMenuContextMenuManager.initializeBase(this);
};

Quantumart.QP8.BackendBreadMenuContextMenuManager.prototype = {
  _сontextMenus: {},
  getContextMenu: function (contextMenuCode) {
    var contextMenu = null;
    if (this._сontextMenus[contextMenuCode]) {
      contextMenu = this._сontextMenus[contextMenuCode];
    }

    return contextMenu;
  },

  createContextMenu: function (contextMenuCode, contextMenuElementId, options) {
    var contextMenu = new Quantumart.QP8.BackendContextMenu(contextMenuCode, contextMenuElementId, options);
    contextMenu.set_contextMenuManager(this);
    contextMenu.initialize();

    this._сontextMenus[contextMenuCode] = contextMenu;
    return contextMenu;
  },

  removeContextMenu: function (contextMenuCode) {
    $q.removeProperty(this._сontextMenus, contextMenuCode);
  },

  destroyContextMenu: function (contextMenuCode) {
    var contextMenu = this._сontextMenus[contextMenuCode];
    if (contextMenu != null) {
      if (contextMenu.dispose) {
        contextMenu.dispose();
      }

      contextMenu = null;
    }
  },

  getContextMenuEventType: function () {
    return jQuery.fn['jeegoocontext'].getContextMenuEventType();
  },

  dispose: function () {
    Quantumart.QP8.BackendBreadMenuContextMenuManager.callBaseMethod(this, 'dispose');
    if (this._сontextMenus) {
      for (var contextMenuCode in this._сontextMenus) {
        this.destroyContextMenu(contextMenuCode);
      }

      this._сontextMenus = null;
    }

    Quantumart.QP8.BackendBreadMenuContextMenuManager._instance = null;
    $q.collectGarbageInIE();
  }
};


Quantumart.QP8.BackendBreadMenuContextMenuManager._instance = null;
Quantumart.QP8.BackendBreadMenuContextMenuManager.getInstance = function Quantumart$QP8$BackendBreadMenuContextMenuManager$getInstance() {
  if (Quantumart.QP8.BackendBreadMenuContextMenuManager._instance == null) {
    Quantumart.QP8.BackendBreadMenuContextMenuManager._instance = new Quantumart.QP8.BackendBreadMenuContextMenuManager();
  }

  return Quantumart.QP8.BackendBreadMenuContextMenuManager._instance;
};

Quantumart.QP8.BackendBreadMenuContextMenuManager.destroyInstance = function Quantumart$QP8$BackendBreadMenuContextMenuManager$destroyInstance() {
  if (Quantumart.QP8.BackendBreadMenuContextMenuManager._instance) {
    Quantumart.QP8.BackendBreadMenuContextMenuManager._instance.dispose();
  }
};

Quantumart.QP8.BackendBreadMenuContextMenuManager.registerClass('Quantumart.QP8.BackendBreadMenuContextMenuManager', Quantumart.QP8.Observable);