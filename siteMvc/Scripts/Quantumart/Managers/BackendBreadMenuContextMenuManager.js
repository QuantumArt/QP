class BackendBreadMenuContextMenuManager extends Quantumart.QP8.Observable {
  static getInstance() {
    if (!BackendBreadMenuContextMenuManager._instance) {
      BackendBreadMenuContextMenuManager._instance = new BackendBreadMenuContextMenuManager();
    }

    return BackendBreadMenuContextMenuManager._instance;
  }

  static destroyInstance() {
    if (BackendBreadMenuContextMenuManager._instance) {
      BackendBreadMenuContextMenuManager._instance.dispose();
      BackendBreadMenuContextMenuManager._instance = null;
    }
  }

  constructor() {
    // @ts-ignore
    super();
    this._сontextMenus = {};
  }

  getContextMenu(contextMenuCode) {
    return this._сontextMenus[contextMenuCode];
  }

  createContextMenu(contextMenuCode, contextMenuElementId, options) {
    const contextMenu = new Quantumart.QP8.BackendContextMenu(contextMenuCode, contextMenuElementId, options);
    contextMenu.set_contextMenuManager(this);
    contextMenu.initialize();

    this._сontextMenus[contextMenuCode] = contextMenu;
    return contextMenu;
  }

  removeContextMenu(contextMenuCode) {
    $q.removeProperty(this._сontextMenus, contextMenuCode);
  }

  dispose() {
    super.dispose();
    if (this._сontextMenus) {
      Object.values(this._сontextMenus).forEach(menu => {
        if (menu.dispose) {
          menu.dispose();
        }
      }, this);
    }

    this._сontextMenus = null;
    $q.collectGarbageInIE();
  }
}

Quantumart.QP8.BackendBreadMenuContextMenuManager = BackendBreadMenuContextMenuManager;
