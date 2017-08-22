class BackendLibraryManager extends Quantumart.QP8.Observable {
  static getInstance() {
    if (!BackendLibraryManager._instance) {
      BackendLibraryManager._instance = new BackendLibraryManager();
    }

    return BackendLibraryManager._instance;
  }

  static destroyInstance() {
    if (BackendLibraryManager._instance) {
      BackendLibraryManager._instance.dispose();
      BackendLibraryManager._instance = null;
    }
  }

  static generateLibraryGroupCode(actionCode, parentEntityId) {
    return `${actionCode}_${parentEntityId}`;
  }

  static getLibraryGroupCode(fileTypeCode, folderId) {
    const folderTypeCode = fileTypeCode === window.ENTITY_TYPE_CODE_SITE_FILE
      ? window.ENTITY_TYPE_CODE_SITE_FOLDER
      : window.ENTITY_TYPE_CODE_CONTENT_FOLDER;

    const libraryActionCode = fileTypeCode === window.ENTITY_TYPE_CODE_SITE_FILE
      ? window.ACTION_CODE_SITE_LIBRARY
      : window.ACTION_CODE_CONTENT_LIBRARY;

    const parentEntityId = $o.getParentEntityId(folderTypeCode, folderId);
    return BackendLibraryManager.generateLibraryGroupCode(libraryActionCode, parentEntityId);
  }

  constructor() {
    super();
    this._libraryGroups = {};
  }

  getLibraryGroup(libraryGroupCode) {
    return this._libraryGroups[libraryGroupCode];
  }

  createLibraryGroup(libraryGroupCode) {
    this._libraryGroups[libraryGroupCode] = this.getLibraryGroup(libraryGroupCode) || {};
    return this._libraryGroups[libraryGroupCode];
  }

  refreshLibraryGroup(entityTypeId, parentEntityId, options) {
    const libraryGroup = this.getLibraryGroup(BackendLibraryManager.getLibraryGroupCode(entityTypeId, parentEntityId));
    Object.keys(libraryGroup || {}).forEach(libraryElementId => this.refreshLibrary(libraryElementId, options), this);
  }

  resetLibraryGroup(libraryGroupCode, options) {
    const libraryGroup = this.getLibraryGroup(libraryGroupCode);
    Object.keys(libraryGroup || {}).forEach(libraryElementId => this.resetLibrary(libraryElementId, options), this);
  }

  removeLibraryGroup(libraryGroupCode) {
    $q.removeProperty(this._libraryGroups, libraryGroupCode);
  }

  getAllLibraries() {
    const allLibraries = [];
    Object.values(this._libraryGroups || {}).forEach(libraryGroup => {
      Object.values(libraryGroup || {}).forEach(val => allLibraries.push(val), this);
    }, this);

    return allLibraries;
  }

  getLibrary(libraryElementId) {
    const libraryGroup = Object.values(this._libraryGroups || {}).find(val => val[libraryElementId]);
    return libraryGroup[libraryElementId];
  }

  createLibrary(libraryElementId, parentEntityId, actionCode, options, hostOptions) {
    const libraryGroupCode = BackendLibraryManager.generateLibraryGroupCode(actionCode, parentEntityId);
    const library = new Quantumart.QP8.BackendLibrary(
      libraryGroupCode,
      libraryElementId,
      parentEntityId,
      actionCode,
      options,
      hostOptions
    );

    library.set_libraryManager(this);

    const libraryGroup = this.createLibraryGroup(libraryGroupCode);
    libraryGroup[libraryElementId] = library;

    return library;
  }

  refreshLibrary(libraryElementId, options) {
    const library = this.getLibrary(libraryElementId);
    if (library) {
      library.refreshCurrentFileList(options);
    }
  }

  resetLibrary(libraryElementId, options) {
    const library = this.getLibrary(libraryElementId);
    if (library) {
      library.resetCurrentFileList(options);
    }
  }

  removeLibrary(libraryElementId) {
    const library = this.getLibrary(libraryElementId);
    if (library) {
      const libraryGroup = this.getLibraryGroup(library._libraryGroupCode);
      $q.removeProperty(libraryGroup, libraryElementId);
      if ($q.getHashKeysCount(libraryGroup) === 0) {
        this.removeLibraryGroup(library._libraryGroupCode);
      }
    }
  }

  onActionExecuted(eventArgs) {
    const entityTypeCode = eventArgs.get_entityTypeCode();
    const actionTypeCode = eventArgs.get_actionTypeCode();
    if ((entityTypeCode === window.ENTITY_TYPE_CODE_SITE_FILE
      || entityTypeCode === window.ENTITY_TYPE_CODE_CONTENT_FILE)
      && ((eventArgs.get_isSaved()
        || eventArgs.get_isUpdated()
        || eventArgs.get_isRemoving()
      ) || actionTypeCode === window.ACTION_TYPE_CODE_ALL_FILES_UPLOADED
      || actionTypeCode === window.ACTION_TYPE_CODE_FILE_CROPPED)
    ) {
      this.refreshLibraryGroup(entityTypeCode, eventArgs.get_parentEntityId());
    }
  }

  dispose() {
    super.dispose();
    if (this._libraryGroups) {
      Object.values(this._libraryGroups || {}).forEach(libraryGroup => {
        Object.keys(libraryGroup || {}).forEach(libraryElementId => {
          const library = this.getLibrary(libraryElementId);
          if (library && library.dispose) {
            library.dispose();
          }
        }, this);
      }, this);
    }

    this._libraryGroups = null;
    $q.collectGarbageInIE();
  }
}

Quantumart.QP8.BackendLibraryManager = BackendLibraryManager;
