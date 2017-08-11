// #region class BackendLibraryManager
// === Класс "Менеджер библиотек" ===
Quantumart.QP8.BackendLibraryManager = function () {
	Quantumart.QP8.BackendLibraryManager.initializeBase(this);
};

Quantumart.QP8.BackendLibraryManager.prototype = {
	_libraryGroups: {}, // список групп библиотек

	generateLibraryGroupCode: function (actionCode, parentEntityId) {
		var libraryGroupCode = String.format("{0}_{1}", actionCode, parentEntityId);

		return libraryGroupCode;
	},

	getLibraryGroupCode: function (fileTypeCode, folderId) {
		var folderTypeCode = (fileTypeCode == ENTITY_TYPE_CODE_SITE_FILE) ? ENTITY_TYPE_CODE_SITE_FOLDER : ENTITY_TYPE_CODE_CONTENT_FOLDER;
		var libraryActionCode = (fileTypeCode == ENTITY_TYPE_CODE_SITE_FILE) ? ACTION_CODE_SITE_LIBRARY : ACTION_CODE_CONTENT_LIBRARY;
		var parentEntityId = $o.getParentEntityId(folderTypeCode, folderId);
		return this.generateLibraryGroupCode(libraryActionCode, parentEntityId);
	},

	getLibraryGroup: function (libraryGroupCode) {
		var libraryGroup = null;
		if (this._libraryGroups[libraryGroupCode]) {
			libraryGroup = this._libraryGroups[libraryGroupCode];
		}

		return libraryGroup;
	},

	createLibraryGroup: function (libraryGroupCode) {
		var libraryGroup = this.getLibraryGroup(libraryGroupCode);
		if (!libraryGroup) {
			libraryGroup = {};
			this._libraryGroups[libraryGroupCode] = libraryGroup;
		}

		return libraryGroup;
	},

	refreshLibraryGroup: function (entityTypeId, parentEntityId, options) {
		var libraryGroup = this.getLibraryGroup(this.getLibraryGroupCode(entityTypeId, parentEntityId));

		if (libraryGroup) {
			for (var libraryElementId in libraryGroup) {
				this.refreshLibrary(libraryElementId, options);
			}
		}

		libraryGroup = null;
	},

	resetLibraryGroup: function (libraryGroupCode, options) {
		var libraryGroup = this.getLibraryGroup(libraryGroupCode);

		if (libraryGroup) {
			for (var libraryElementId in libraryGroup) {
				this.resetLibrary(libraryElementId, options);
			}
		}

		libraryGroup = null;
	},

	removeLibraryGroup: function (libraryGroupCode) {
		$q.removeProperty(this._libraryGroups, libraryGroupCode);
	},

	getAllLibraries: function () {
		var allLibraries = [];

		for (var libraryGroupCode in this._libraryGroups) {
			var libraryGroup = this._libraryGroups[libraryGroupCode];
			for (var libraryElementId in libraryGroup) {
				allLibraries.push(libraryGroup[libraryElementId]);
			}
		}

		return allLibraries;
	},

	getLibrary: function (libraryElementId) {
		var library = null;

		for (var libraryGroupCode in this._libraryGroups) {
			var libraryGroup = this._libraryGroups[libraryGroupCode];
			if (libraryGroup[libraryElementId]) {
				library = libraryGroup[libraryElementId];
				break;
			}
		}

		return library;
	},

	createLibrary: function (libraryElementId, parentEntityId, actionCode, options, hostOptions) {
		var libraryGroupCode = this.generateLibraryGroupCode(actionCode, parentEntityId);

		var library = new Quantumart.QP8.BackendLibrary(libraryGroupCode, libraryElementId, parentEntityId, actionCode, options, hostOptions);
		library.set_libraryManager(this);

		var libraryGroup = this.createLibraryGroup(libraryGroupCode);
		libraryGroup[libraryElementId] = library;

		return library;
	},

	refreshLibrary: function (libraryElementId, options) {
		var library = this.getLibrary(libraryElementId);
		if (library) {
			library.refreshCurrentFileList(options);
		}

		library = null;
	},

	resetLibrary: function (libraryElementId, options) {
		var library = this.getLibrary(libraryElementId);
		if (library) {
			library.resetCurrentFileList(options);
		}

		library = null;
	},

	removeLibrary: function (libraryElementId) {
		var library = this.getLibrary(libraryElementId);
		if (library) {
			var libraryGroupCode = library._libraryGroupCode;
			var libraryGroup = this.getLibraryGroup(libraryGroupCode);

			$q.removeProperty(libraryGroup, libraryElementId);

			if ($q.getHashKeysCount(libraryGroup) == 0) {
				this.removeLibraryGroup(libraryGroupCode);
			}
		}
	},

	destroyLibrary: function (libraryElementId) {
		var library = this.getLibrary(libraryElementId);
		if (library != null) {
			if (library.dispose) {
				library.dispose();
			}
			library = null;
		}
	},

	onActionExecuted: function (eventArgs) {
		var entityTypeCode = eventArgs.get_entityTypeCode();
		var actionTypeCode = eventArgs.get_actionTypeCode();
		if (
			(entityTypeCode == ENTITY_TYPE_CODE_SITE_FILE || entityTypeCode == ENTITY_TYPE_CODE_CONTENT_FILE)
			&& ((eventArgs.get_isSaved() || eventArgs.get_isUpdated() || eventArgs.get_isRemoving()) || actionTypeCode == ACTION_TYPE_CODE_ALL_FILES_UPLOADED || actionTypeCode == ACTION_TYPE_CODE_FILE_CROPPED)
		) {
			this.refreshLibraryGroup(entityTypeCode, eventArgs.get_parentEntityId());
		}
	},

	dispose: function () {
		Quantumart.QP8.BackendLibraryManager.callBaseMethod(this, "dispose");

		if (this._libraryGroups) {
			for (libraryGroupCode in this._libraryGroups) {
				var libraryGroup = this._libraryGroups[libraryGroupCode];

				for (libraryElementId in libraryGroup) {
					this.destroyLibrary(libraryElementId);
				}
			}

			this._libraryGroups = null;
		}

		Quantumart.QP8.BackendLibraryManager._instance = null;

		$q.collectGarbageInIE();
	}
};

Quantumart.QP8.BackendLibraryManager._instance = null; // экземпляр класса

// Возвращает экземпляр класса "Менеджер библиотек"
Quantumart.QP8.BackendLibraryManager.getInstance = function Quantumart$QP8$BackendLibraryManager$getInstance() {
	if (Quantumart.QP8.BackendLibraryManager._instance == null) {
		Quantumart.QP8.BackendLibraryManager._instance = new Quantumart.QP8.BackendLibraryManager();
	}

	return Quantumart.QP8.BackendLibraryManager._instance;
};

// Уничтожает экземпляр класса "Менеджер библиотек"
Quantumart.QP8.BackendLibraryManager.destroyInstance = function Quantumart$QP8$BackendLibraryManager$destroyInstance() {
	if (Quantumart.QP8.BackendLibraryManager._instance) {
		Quantumart.QP8.BackendLibraryManager._instance.dispose();
	}
};

Quantumart.QP8.BackendLibraryManager.registerClass("Quantumart.QP8.BackendLibraryManager", Quantumart.QP8.Observable);

// #endregion
