//#region class BackendTreeMenuContextMenuManager
// === Класс "Менеджер контекстных меню" ===
Quantumart.QP8.BackendTreeMenuContextMenuManager = function () {
	Quantumart.QP8.BackendTreeMenuContextMenuManager.initializeBase(this);
};

Quantumart.QP8.BackendTreeMenuContextMenuManager.prototype = {
	_сontextMenus: {}, // список контекстных меню

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
		return jQuery.fn["jeegoocontext"].getContextMenuEventType();
	},

	dispose: function () {
		Quantumart.QP8.BackendTreeMenuContextMenuManager.callBaseMethod(this, "dispose");

		if (this._сontextMenus) {
			for (var contextMenuCode in this._сontextMenus) {
				this.destroyContextMenu(contextMenuCode);
			}

			this._сontextMenus = null;
		}

		Quantumart.QP8.BackendTreeMenuContextMenuManager._instance = null;

		$q.collectGarbageInIE();
	}
};


Quantumart.QP8.BackendTreeMenuContextMenuManager._instance = null; // экземпляр класса

// Возвращает экземпляр класса "Менеджер контекстных меню"
Quantumart.QP8.BackendTreeMenuContextMenuManager.getInstance = function Quantumart$QP8$BackendTreeMenuContextMenuManager$getInstance() {
	if (Quantumart.QP8.BackendTreeMenuContextMenuManager._instance == null) {
		Quantumart.QP8.BackendTreeMenuContextMenuManager._instance = new Quantumart.QP8.BackendTreeMenuContextMenuManager();
	}

	return Quantumart.QP8.BackendTreeMenuContextMenuManager._instance;
};

// Уничтожает экземпляр класса "Менеджер контекстных меню"
Quantumart.QP8.BackendTreeMenuContextMenuManager.destroyInstance = function Quantumart$QP8$BackendTreeMenuContextMenuManager$destroyInstance() {
	if (Quantumart.QP8.BackendTreeMenuContextMenuManager._instance) {
		Quantumart.QP8.BackendTreeMenuContextMenuManager._instance.dispose();
	}
};

Quantumart.QP8.BackendTreeMenuContextMenuManager.registerClass("Quantumart.QP8.BackendTreeMenuContextMenuManager", Quantumart.QP8.Observable);
//#endregion
