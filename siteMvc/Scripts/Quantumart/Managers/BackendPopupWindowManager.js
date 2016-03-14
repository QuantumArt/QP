//#region class BackendPopupWindowManager
// === Класс "Менеджер всплывающих окон" ===
Quantumart.QP8.BackendPopupWindowManager = function (options) {
    Quantumart.QP8.BackendPopupWindowManager.initializeBase(this);

    this._hostStateStorage = new Quantumart.QP8.BackendDocumentHostStateStorage();
    this._hostStateStorage._get_host_key = (function (that) {
        var original = that._hostStateStorage._get_host_key;
        return function (hostParams) {
            hostParams.entityId = 0;
            return original(hostParams);
        };
    })(this);
};

Quantumart.QP8.BackendPopupWindowManager.prototype = {
    _popupWindows: {},
    _hostStateStorage: null,

	generatePopupWindowId: function () {
		var popupWindowNumber = 1;
		var popupWindows = this._popupWindows;

		if ($q.getHashKeysCount(popupWindows) > 0) {
			var popupWindowIDs = [];

			for (var popupWindowId in popupWindows) {
				Array.add(popupWindowIDs, popupWindowId)
			}

			var sortedPopupWindowIDs = popupWindowIDs.sort();
			var lastPopupWindowId = sortedPopupWindowIDs[sortedPopupWindowIDs.length - 1];
			var numberMatch = lastPopupWindowId.match("[0-9]+");

			if (numberMatch.length == 1) {
				popupWindowNumber = parseInt(numberMatch[0]) + 1;
			}
			else {
				alert($l.PopupWindow.popupWindowIdGenerationErrorMessage);
				return null;
			}
		}

		var popupWindowId = String.format("win{0}", popupWindowNumber);

		return popupWindowId;
	},

	generatePopupWindowTitle: function (eventArgs) {
		var popupWindowTitle = Quantumart.QP8.BackendDocumentHost.generateTitle(eventArgs, { "isTab": false });
		return popupWindowTitle;
	},

	getAllPopupWindows: function () {
		var popupWindowsHash = this._popupWindows;
		var popupWindows = [];

		for (var popupWindowId in popupWindowsHash) {
			Array.add(popupWindows, popupWindowsHash[popupWindowId]);
		}

		return popupWindows;
	},

	getPopupWindow: function (popupWindowId) {
		var popupWindow = null;

		if (this._popupWindows[popupWindowId]) {
			popupWindow = this._popupWindows[popupWindowId];
		}

		return popupWindow;
	},

	getPopupWindowByEventArgs: function (eventArgs) {
		return jQuery.grep(this.getAllPopupWindows(), function (w) {
			return w.get_entityTypeCode() == eventArgs.get_entityTypeCode() &&
					w.get_entityId() == eventArgs.get_entityId() &&
					w.get_actionCode() == eventArgs.get_actionCode();
		});
	},

	createPopupWindow: function (eventArgs, options) {
		var popupWindowId = options ? options.popupWindowId : "";
		if ($q.isNullOrWhiteSpace(popupWindowId)) {
			popupWindowId = this.generatePopupWindowId();
		}

		if (this._popupWindows[popupWindowId]) {
			alert($l.PopupWindow.popupWindowIdNotUniqueErrorMessage);
			return null;
		}

		jQuery.extend(options,
		{
			width: eventArgs.get_windowWidth(),
			height: eventArgs.get_windowHeight(),
			hostStateStorage: this._hostStateStorage
		});

		var popupWindow = new Quantumart.QP8.BackendPopupWindow(popupWindowId, eventArgs, options);
		popupWindow.set_popupWindowManager(this);
		popupWindow.initialize();

		this._popupWindows[popupWindowId] = popupWindow;

		return popupWindow;
	},

	openPopupWindow: function (eventArgs) {
		var popupWindow = this.createPopupWindow(eventArgs, {});
		popupWindow.openWindow();

		return popupWindow;
	},

	removePopupWindow: function (popupWindowId) {
		$q.removeProperty(this._popupWindows, popupWindowId);
	},

	destroyPopupWindow: function (popupWindowId) {
		var popupWindow = this._popupWindows[popupWindowId];

		if (popupWindow != null) {
			if (popupWindow.dispose) {
				popupWindow.dispose();
			}
			popupWindow = null;
		}
	},

	closeNotExistentPopupWindows: function () {
		var popupWindows = this.getAllPopupWindows();

		for (var popupWindowIndex = 0, popupWindowCount = popupWindows.length; popupWindowIndex < popupWindowCount; popupWindowIndex++) {
			var popupWindow = popupWindows[popupWindowIndex];

			var entityTypeCode = popupWindow.get_entityTypeCode();
			var entityId = popupWindow.get_entityId();
			var actionTypeCode = popupWindow.get_actionTypeCode();
			var isMultipleEntities = popupWindow.get_isMultipleEntities();

			if (actionTypeCode != ACTION_TYPE_CODE_ADD_NEW) {
				if (isMultipleEntities && actionTypeCode != ACTION_TYPE_CODE_MULTIPLE_SELECT) {
					var entities = popupWindow.get_entities();

					for (var entityIndex = 0; entityIndex < entities.length; entityIndex++) {
						var entity = entities[entityIndex];
						if (entity) {
							var entityExist = $o.checkEntityExistence(entityTypeCode, entity.Id)
							if (!entityExist) {
								popupWindow.closeWindow();
								break;
							}
						}
					}
				}
				else {
					var entityExist = $o.checkEntityExistence(entityTypeCode, entityId);
					if (!entityExist) {
						popupWindow.closeWindow();
					}
				}
			}

			popupWindow = null;
		}

		popupWindows = null;
	},

	onActionExecuted: function (eventArgs) {
		if (eventArgs.get_isRemoving() || eventArgs.get_isArchiving() || eventArgs.get_isRestoring()) {
			this.closeNotExistentPopupWindows();
		}
	},

	onNeedUp: function (eventArgs, popupWindowId) {
		var popupWindow = this.getPopupWindow(popupWindowId);
		if (popupWindow) {
			popupWindow.closeWindow();
		}
	},

	hostExternalCallerContextsUnbinded: function (unbindingEventArgs) {
		this.notify(EVENT_TYPE_HOST_EXTERNAL_CALLER_CONTEXTS_UNBINDED, unbindingEventArgs);
	},

	dispose: function () {
		Quantumart.QP8.BackendPopupWindowManager.callBaseMethod(this, "dispose");

		if (this._popupWindows) {
			for (var popupWindowId in this._popupWindows) {
				this.destroyPopupWindow(popupWindowId);
			}

			this._popupWindows = null;
		}

		if (this._hostStateStorage) {
		    this._hostStateStorage.dispose();
		    this._hostStateStorage = null;
		}

		Quantumart.QP8.BackendPopupWindowManager._instance = null;

		$q.collectGarbageInIE();
	}
};

Quantumart.QP8.BackendPopupWindowManager._instance = null; // экземпляр класса

// Возвращает экземпляр класса "Менеджер всплывающих окон"
Quantumart.QP8.BackendPopupWindowManager.getInstance = function Quantumart$QP8$BackendPopupWindowManager$getInstance(options) {
	if (Quantumart.QP8.BackendPopupWindowManager._instance == null) {
		Quantumart.QP8.BackendPopupWindowManager._instance = new Quantumart.QP8.BackendPopupWindowManager(options);
	}

	return Quantumart.QP8.BackendPopupWindowManager._instance;
};

// Уничтожает экземпляр класса "Менеджер всплывающих окон"
Quantumart.QP8.BackendPopupWindowManager.destroyInstance = function Quantumart$QP8$BackendPopupWindowManager$destroyInstance() {
	if (Quantumart.QP8.BackendPopupWindowManager._instance) {
		Quantumart.QP8.BackendPopupWindowManager._instance.dispose();
	}
};

Quantumart.QP8.BackendPopupWindowManager.registerClass("Quantumart.QP8.BackendPopupWindowManager", Quantumart.QP8.Observable);
//#endregion