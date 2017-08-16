Quantumart.QP8.BackendSearchBlockManager = function () {
  Quantumart.QP8.BackendSearchBlockManager.initializeBase(this);
};

Quantumart.QP8.BackendSearchBlockManager.prototype = {
  _searchBlockGroups: {},

  generateSearchBlockGroupCode(entityTypeCode, parentEntityId) {
    const searchBlockCode = String.format('{0}_{1}', entityTypeCode, parentEntityId);

    return searchBlockCode;
  },

  getSearchBlockGroup(searchBlockGroupCode) {
    let searchBlockGroup = null;
    if (this._searchBlockGroups[searchBlockGroupCode]) {
      searchBlockGroup = this._searchBlockGroups[searchBlockGroupCode];
    }

    return searchBlockGroup;
  },

  createSearchBlockGroup(searchBlockGroupCode) {
    let searchBlockGroup = this.getSearchBlockGroup(searchBlockGroupCode);
    if (!searchBlockGroup) {
      searchBlockGroup = {};
      this._searchBlockGroups[searchBlockGroupCode] = searchBlockGroup;
    }

    return searchBlockGroup;
  },

  removeSearchBlockGroup(searchBlockGroupCode) {
    $q.removeProperty(this._searchBlockGroups, searchBlockGroupCode);
  },

  getSearchBlock(searchBlockElementId) {
    let searchBlock = null;

    for (const searchBlockGroupCode in this._searchBlockGroups) {
      const searchBlockGroup = this._searchBlockGroups[searchBlockGroupCode];
      if (searchBlockGroup[searchBlockElementId]) {
        searchBlock = searchBlockGroup[searchBlockElementId];
        break;
      }
    }

    return searchBlock;
  },

  createSearchBlock(searchBlockElementId, entityTypeCode, parentEntityId, host, options) {
    const searchBlockGroupCode = this.generateSearchBlockGroupCode(entityTypeCode, parentEntityId);

    let searchBlock = null;
    if (options.contextSearch) {
      searchBlock = new Quantumart.QP8.BackendContextBlock(searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options);
    } else if (entityTypeCode == window.ENTITY_TYPE_CODE_ARTICLE || entityTypeCode == window.ENTITY_TYPE_CODE_VIRTUAL_ARTICLE || entityTypeCode == window.ENTITY_TYPE_CODE_ARCHIVE_ARTICLE) {
      if (host && host.get_documentContext() && host.get_documentContext().get_options() && host.get_documentContext().get_options().isVirtual) {
        Object.assign(options, { isVirtual: true });
      }
      searchBlock = new Quantumart.QP8.BackendArticleSearchBlock(searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options);
    } else if (entityTypeCode == window.ENTITY_TYPE_CODE_CONTENT) {
      searchBlock = new Quantumart.QP8.BackendContentSearchBlock(searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options);
    } else if (entityTypeCode == window.ENTITY_TYPE_CODE_USER) {
      searchBlock = new Quantumart.QP8.BackendUserSearchBlock(searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options);
    } else {
      searchBlock = new Quantumart.QP8.BackendSearchBlockBase(searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options);
    }
    searchBlock.initialize();

    const searchBlockGroup = this.createSearchBlockGroup(searchBlockGroupCode);
    searchBlockGroup[searchBlockElementId] = searchBlock;

    return searchBlock;
  },

  removeSearchBlock(searchBlockElementId) {
    const searchBlock = this.getSearchBlock(searchBlockElementId);
    if (searchBlock) {
      const searchBlockGroupCode = searchBlock.get_searchBlockGroupCode();
      const searchBlockGroup = this.getSearchBlockGroup(searchBlockGroupCode);

      $q.removeProperty(searchBlockGroup, searchBlockElementId);

      if ($q.getHashKeysCount(searchBlockGroup) == 0) {
        this.removeSearchBlockGroup(searchBlockGroupCode);
      }
    }
  },

  destroySearchBlock(searchBlockElementId) {
    const searchBlock = this.getSearchBlock(searchBlockElementId);
    if (searchBlock) {
      this.removeSearchBlock(searchBlockElementId);
      if (searchBlock.dispose) {
        searchBlock.dispose();
      }
    }
  },

  dispose() {
    Quantumart.QP8.BackendSearchBlockManager.callBaseMethod(this, 'dispose');
    if (this._searchBlockGroups) {
      for (const searchBlockGroupCode in this._searchBlockGroups) {
        const searchBlockGroup = this._searchBlockGroups[searchBlockGroupCode];
        Object.keys(searchBlockGroup).forEach(this.destroySearchBlock);
      }
    }
  }
};

Quantumart.QP8.BackendSearchBlockManager._instance = null;
Quantumart.QP8.BackendSearchBlockManager.getInstance = function () {
  if (Quantumart.QP8.BackendSearchBlockManager._instance == null) {
    Quantumart.QP8.BackendSearchBlockManager._instance = new Quantumart.QP8.BackendSearchBlockManager();
  }

  return Quantumart.QP8.BackendSearchBlockManager._instance;
};

Quantumart.QP8.BackendSearchBlockManager.destroyInstance = function () {
  if (Quantumart.QP8.BackendSearchBlockManager._instance) {
    Quantumart.QP8.BackendSearchBlockManager._instance.dispose();
    Quantumart.QP8.BackendSearchBlockManager._instance = null;
  }
};

Quantumart.QP8.BackendSearchBlockManager.registerClass('Quantumart.QP8.BackendSearchBlockManager', Quantumart.QP8.Observable);
