Quantumart.QP8.BackendLibraryManager = function () {
  Quantumart.QP8.BackendLibraryManager.initializeBase(this);
};

Quantumart.QP8.BackendLibraryManager.prototype = {
  _libraryGroups: {},

  generateLibraryGroupCode: function (actionCode, parentEntityId) {
    let libraryGroupCode = String.format('{0}_{1}', actionCode, parentEntityId);

    return libraryGroupCode;
  },

  getLibraryGroupCode: function (fileTypeCode, folderId) {
    let folderTypeCode = fileTypeCode == window.ENTITY_TYPE_CODE_SITE_FILE ? window.ENTITY_TYPE_CODE_SITE_FOLDER : window.ENTITY_TYPE_CODE_CONTENT_FOLDER;
    let libraryActionCode = fileTypeCode == window.ENTITY_TYPE_CODE_SITE_FILE ? window.ACTION_CODE_SITE_LIBRARY : window.ACTION_CODE_CONTENT_LIBRARY;
    let parentEntityId = $o.getParentEntityId(folderTypeCode, folderId);
    return this.generateLibraryGroupCode(libraryActionCode, parentEntityId);
  },

  getLibraryGroup: function (libraryGroupCode) {
    let libraryGroup = null;
    if (this._libraryGroups[libraryGroupCode]) {
      libraryGroup = this._libraryGroups[libraryGroupCode];
    }

    return libraryGroup;
  },

  createLibraryGroup: function (libraryGroupCode) {
    let libraryGroup = this.getLibraryGroup(libraryGroupCode);
    if (!libraryGroup) {
      libraryGroup = {};
      this._libraryGroups[libraryGroupCode] = libraryGroup;
    }

    return libraryGroup;
  },

  refreshLibraryGroup: function (entityTypeId, parentEntityId, options) {
    let libraryGroup = this.getLibraryGroup(this.getLibraryGroupCode(entityTypeId, parentEntityId));

    if (libraryGroup) {
      for (let libraryElementId in libraryGroup) {
        this.refreshLibrary(libraryElementId, options);
      }
    }

    libraryGroup = null;
  },

  resetLibraryGroup: function (libraryGroupCode, options) {
    let libraryGroup = this.getLibraryGroup(libraryGroupCode);

    if (libraryGroup) {
      for (let libraryElementId in libraryGroup) {
        this.resetLibrary(libraryElementId, options);
      }
    }

    libraryGroup = null;
  },

  removeLibraryGroup: function (libraryGroupCode) {
    $q.removeProperty(this._libraryGroups, libraryGroupCode);
  },

  getAllLibraries: function () {
    let allLibraries = [];

    for (let libraryGroupCode in this._libraryGroups) {
      let libraryGroup = this._libraryGroups[libraryGroupCode];
      for (let libraryElementId in libraryGroup) {
        allLibraries.push(libraryGroup[libraryElementId]);
      }
    }

    return allLibraries;
  },

  getLibrary: function (libraryElementId) {
    let library = null;

    for (let libraryGroupCode in this._libraryGroups) {
      let libraryGroup = this._libraryGroups[libraryGroupCode];
      if (libraryGroup[libraryElementId]) {
        library = libraryGroup[libraryElementId];
        break;
      }
    }

    return library;
  },

  createLibrary: function (libraryElementId, parentEntityId, actionCode, options, hostOptions) {
    let libraryGroupCode = this.generateLibraryGroupCode(actionCode, parentEntityId);

    let library = new Quantumart.QP8.BackendLibrary(libraryGroupCode, libraryElementId, parentEntityId, actionCode, options, hostOptions);
    library.set_libraryManager(this);

    let libraryGroup = this.createLibraryGroup(libraryGroupCode);
    libraryGroup[libraryElementId] = library;

    return library;
  },

  refreshLibrary: function (libraryElementId, options) {
    let library = this.getLibrary(libraryElementId);
    if (library) {
      library.refreshCurrentFileList(options);
    }

    library = null;
  },

  resetLibrary: function (libraryElementId, options) {
    let library = this.getLibrary(libraryElementId);
    if (library) {
      library.resetCurrentFileList(options);
    }

    library = null;
  },

  removeLibrary: function (libraryElementId) {
    let library = this.getLibrary(libraryElementId);
    if (library) {
      let libraryGroupCode = library._libraryGroupCode;
      let libraryGroup = this.getLibraryGroup(libraryGroupCode);

      $q.removeProperty(libraryGroup, libraryElementId);

      if ($q.getHashKeysCount(libraryGroup) == 0) {
        this.removeLibraryGroup(libraryGroupCode);
      }
    }
  },

  destroyLibrary: function (libraryElementId) {
    let library = this.getLibrary(libraryElementId);
    if (library != null) {
      if (library.dispose) {
        library.dispose();
      }
      library = null;
    }
  },

  onActionExecuted: function (eventArgs) {
    let entityTypeCode = eventArgs.get_entityTypeCode();
    let actionTypeCode = eventArgs.get_actionTypeCode();
    if (
      (entityTypeCode == window.ENTITY_TYPE_CODE_SITE_FILE || entityTypeCode == window.ENTITY_TYPE_CODE_CONTENT_FILE)
      && ((eventArgs.get_isSaved() || eventArgs.get_isUpdated() || eventArgs.get_isRemoving()) || actionTypeCode == window.ACTION_TYPE_CODE_ALL_FILES_UPLOADED || actionTypeCode == window.ACTION_TYPE_CODE_FILE_CROPPED)
    ) {
      this.refreshLibraryGroup(entityTypeCode, eventArgs.get_parentEntityId());
    }
  },

  dispose: function () {
    Quantumart.QP8.BackendLibraryManager.callBaseMethod(this, 'dispose');

    if (this._libraryGroups) {
      for (let libraryGroupCode in this._libraryGroups) {
        let libraryGroup = this._libraryGroups[libraryGroupCode];
        Object.keys(libraryGroup).forEach(this.destroyLibrary);
      }
    }

    Quantumart.QP8.BackendLibraryManager._instance = null;
    $q.collectGarbageInIE();
  }
};

Quantumart.QP8.BackendLibraryManager._instance = null;
Quantumart.QP8.BackendLibraryManager.getInstance = function () {
  if (Quantumart.QP8.BackendLibraryManager._instance == null) {
    Quantumart.QP8.BackendLibraryManager._instance = new Quantumart.QP8.BackendLibraryManager();
  }

  return Quantumart.QP8.BackendLibraryManager._instance;
};

Quantumart.QP8.BackendLibraryManager.destroyInstance = function () {
  if (Quantumart.QP8.BackendLibraryManager._instance) {
    Quantumart.QP8.BackendLibraryManager._instance.dispose();
  }
};

Quantumart.QP8.BackendLibraryManager.registerClass('Quantumart.QP8.BackendLibraryManager', Quantumart.QP8.Observable);
