Quantumart.QP8.BackendEntityTreeManager = function () {
  Quantumart.QP8.BackendEntityTreeManager.initializeBase(this);
};

Quantumart.QP8.BackendEntityTreeManager.prototype = {
  _treeGroups: {},

  initialize() {
  },

  generateTreeGroupCode(entityTypeCode, parentEntityId) {
    const treeGroupCode = String.format('{0}_{1}', entityTypeCode, parentEntityId);

    return treeGroupCode;
  },

  getTreeGroup(treeGroupCode) {
    let treeGroup = null;
    if (this._treeGroups[treeGroupCode]) {
      treeGroup = this._treeGroups[treeGroupCode];
    }

    return treeGroup;
  },

  createTreeGroup(treeGroupCode) {
    let treeGroup = this.getTreeGroup(treeGroupCode);
    if (!treeGroup) {
      treeGroup = {};
      this._treeGroups[treeGroupCode] = treeGroup;
    }

    return treeGroup;
  },

  removeTreeGroup(treeGroupCode) {
    $q.removeProperty(this._treeGroups, treeGroupCode);
  },

  getTree(treeElementId) {
    let tree = null;

    for (const treeGroupCode in this._treeGroups) {
      const treeGroup = this._treeGroups[treeGroupCode];
      if (treeGroup[treeElementId]) {
        tree = treeGroup[treeElementId];
        break;
      }
    }

    return tree;
  },

  createTree(treeElementId, entityTypeCode, parentEntityId, actionCode, options, hostOptions) {
    const treeGroupCode = this.generateTreeGroupCode(entityTypeCode, parentEntityId);

    let tree = null;
    if ($q.isNullOrEmpty(options.virtualContentId)) {
      tree = new Quantumart.QP8.BackendEntityTree(treeGroupCode, treeElementId, entityTypeCode, parentEntityId, actionCode, options, hostOptions);
    } else {
      tree = new Quantumart.QP8.BackendVirtualFieldTree(treeGroupCode, treeElementId, entityTypeCode, parentEntityId, actionCode, options);
    }

    tree.set_treeManager(this);

    const treeGroup = this.createTreeGroup(treeGroupCode);
    treeGroup[treeElementId] = tree;
    return tree;
  },

  removeTree(treeElementId) {
    const tree = this.getTree(treeElementId);
    if (tree) {
      const treeGroupCode = tree.get_treeGroupCode();
      const treeGroup = this.getTreeGroup(treeGroupCode);

      $q.removeProperty(treeGroup, treeElementId);

      if ($q.getHashKeysCount(treeGroup) == 0) {
        this.removeTreeGroup(treeGroupCode);
      }
    }
  },

  destroyTree(treeElementId) {
    let tree = this.getTree(treeElementId);
    if (tree != null) {
      this.removeTree(treeElementId);
      if (tree.dispose) {
        tree.dispose();
      }
      tree = null;
    }
  },

  refreshNode(entityTypeCode, parentEntityId, entityId, options) {
    const treeGroup = this.getTreeGroup(this.generateTreeGroupCode(entityTypeCode, parentEntityId));
    if (treeGroup) {
      for (const treeElementId in treeGroup) {
        const tree = this.getTree(treeElementId);
        const nodeCode = tree.convertEntityIdToNodeCode(entityId);

        tree.refreshNode(nodeCode, options);
      }
    }
  },

  refreshNodes(entityTypeCode, parentEntityId, ids, options) {
    const self = this;
    if ($q.isNullOrEmpty(ids)) {
      const treeGroup = this.getTreeGroup(this.generateTreeGroupCode(entityTypeCode, parentEntityId));
      if (treeGroup) {
        for (const treeElementId in treeGroup) {
          const tree = this.getTree(treeElementId);
          tree.refreshTree();
        }
      }
    } else {
      jQuery.each(ids, (index, id) => {
        self.refreshNode(entityTypeCode, parentEntityId, id, options);
      });
    }
  },

  removeNode(entityTypeCode, parentEntityId, entityId) {
    const treeGroup = this.getTreeGroup(this.generateTreeGroupCode(entityTypeCode, parentEntityId));
    if (treeGroup) {
      for (const treeElementId in treeGroup) {
        const tree = this.getTree(treeElementId);
        const nodeCode = tree.convertEntityIdToNodeCode(entityId);

        tree.removeNode(nodeCode);
      }
    }
  },

  removeNodes(entityTypeCode, parentEntityId, ids) {
    const self = this;
    jQuery.each(ids, (index, id) => {
      self.removeNode(entityTypeCode, parentEntityId, id);
    });
  },

  onActionExecuted(eventArgs) {
    const entityTypeCode = eventArgs.get_entityTypeCode();
    const parentEntityId = eventArgs.get_parentEntityId();
    const actionTypeCode = eventArgs.get_actionTypeCode();
    const entityId = eventArgs.get_entityId();
    const entityIds = eventArgs.get_isMultipleEntities() ? $o.getEntityIDsFromEntities(eventArgs.get_entities()) : [entityId];

    if (eventArgs.get_isRemoving() || eventArgs.get_isArchiving()) {
      this.removeNodes(entityTypeCode, parentEntityId, entityIds);
    } else if ((eventArgs.get_isUpdated()
      || eventArgs.get_isLoaded()
      || actionTypeCode == window.ACTION_TYPE_CODE_CANCEL
      || actionTypeCode == window.ACTION_TYPE_CODE_CHANGE_LOCK)
      && entityTypeCode != window.ENTITY_TYPE_CODE_VIRTUAL_ARTICLE) {
      this.refreshNode(entityTypeCode, parentEntityId, entityId, { loadChildNodes: true, saveNodesSelection: true });
    } else if (eventArgs.get_isSaved() || actionTypeCode == window.ACTION_TYPE_CODE_COPY) {
      const parentIdsInTree = $o.getParentIdsForTree(entityTypeCode, entityIds);
      this.refreshNodes(entityTypeCode, parentEntityId, parentIdsInTree, { loadChildNodes: true, saveNodesSelection: false });
    } else if (eventArgs.get_isRestoring() && entityTypeCode == window.ENTITY_TYPE_CODE_ARCHIVE_ARTICLE) {
      const parentArticlesInTree = $o.getParentIdsForTree(window.ENTITY_TYPE_CODE_ARTICLE, entityIds);
      this.refreshNodes(window.ENTITY_TYPE_CODE_ARTICLE, parentEntityId, parentArticlesInTree, { loadChildNodes: true, saveNodesSelection: false });
    } else if (eventArgs.get_isRestored() && entityTypeCode == window.ENTITY_TYPE_CODE_ARTICLE_VERSION) {
      const newParentEntityId = +$o.getParentEntityId(window.ENTITY_TYPE_CODE_ARTICLE, entityId) || 0;
      this.refreshNode(window.ENTITY_TYPE_CODE_ARTICLE, newParentEntityId, entityId, { loadChildNodes: true, saveNodesSelection: false });
    }
  },

  dispose() {
    Quantumart.QP8.BackendEntityTreeManager.callBaseMethod(this, 'dispose');

    if (this._trees) {
      Object.keys(this._trees).forEach(this.destroyTree);
    }

    Quantumart.QP8.BackendEntityTreeManager._instance = null;
    $q.collectGarbageInIE();
  }
};

Quantumart.QP8.BackendEntityTreeManager._instance = null;
Quantumart.QP8.BackendEntityTreeManager.getInstance = function () {
  if (Quantumart.QP8.BackendEntityTreeManager._instance == null) {
    Quantumart.QP8.BackendEntityTreeManager._instance = new Quantumart.QP8.BackendEntityTreeManager();
  }

  return Quantumart.QP8.BackendEntityTreeManager._instance;
};

Quantumart.QP8.BackendEntityTreeManager.destroyInstance = function () {
  if (Quantumart.QP8.BackendEntityTreeManager._instance) {
    Quantumart.QP8.BackendEntityTreeManager._instance.dispose();
  }
};

Quantumart.QP8.BackendEntityTreeManager.registerClass('Quantumart.QP8.BackendEntityTreeManager', Quantumart.QP8.Observable);
