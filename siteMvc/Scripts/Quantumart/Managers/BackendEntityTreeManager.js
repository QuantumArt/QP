Quantumart.QP8.BackendEntityTreeManager = function () {
  Quantumart.QP8.BackendEntityTreeManager.initializeBase(this);
};

Quantumart.QP8.BackendEntityTreeManager.prototype = {
  _treeGroups: {},

  initialize: function () {
  },

  generateTreeGroupCode: function (entityTypeCode, parentEntityId) {
    let treeGroupCode = String.format('{0}_{1}', entityTypeCode, parentEntityId);

    return treeGroupCode;
  },

  getTreeGroup: function (treeGroupCode) {
    let treeGroup = null;
    if (this._treeGroups[treeGroupCode]) {
      treeGroup = this._treeGroups[treeGroupCode];
    }

    return treeGroup;
  },

  createTreeGroup: function (treeGroupCode) {
    let treeGroup = this.getTreeGroup(treeGroupCode);
    if (!treeGroup) {
      treeGroup = {};
      this._treeGroups[treeGroupCode] = treeGroup;
    }

    return treeGroup;
  },

  removeTreeGroup: function (treeGroupCode) {
    $q.removeProperty(this._treeGroups, treeGroupCode);
  },

  getTree: function (treeElementId) {
    let tree = null;

    for (let treeGroupCode in this._treeGroups) {
      let treeGroup = this._treeGroups[treeGroupCode];
      if (treeGroup[treeElementId]) {
        tree = treeGroup[treeElementId];
        break;
      }
    }

    return tree;
  },

  createTree: function (treeElementId, entityTypeCode, parentEntityId, actionCode, options, hostOptions) {
    let treeGroupCode = this.generateTreeGroupCode(entityTypeCode, parentEntityId);

    let tree = null;
    if ($q.isNullOrEmpty(options.virtualContentId)) {
      tree = new Quantumart.QP8.BackendEntityTree(treeGroupCode, treeElementId, entityTypeCode, parentEntityId, actionCode, options, hostOptions);
    } else {
      tree = new Quantumart.QP8.BackendVirtualFieldTree(treeGroupCode, treeElementId, entityTypeCode, parentEntityId, actionCode, options);
    }

    tree.set_treeManager(this);

    let treeGroup = this.createTreeGroup(treeGroupCode);
    treeGroup[treeElementId] = tree;
    return tree;
  },

  removeTree: function (treeElementId) {
    let tree = this.getTree(treeElementId);
    if (tree) {
      let treeGroupCode = tree.get_treeGroupCode();
      let treeGroup = this.getTreeGroup(treeGroupCode);

      $q.removeProperty(treeGroup, treeElementId);

      if ($q.getHashKeysCount(treeGroup) == 0) {
        this.removeTreeGroup(treeGroupCode);
      }
    }
  },

  destroyTree: function (treeElementId) {
    let tree = this.getTree(treeElementId);
    if (tree != null) {
      this.removeTree(treeElementId);
      if (tree.dispose) {
        tree.dispose();
      }
      tree = null;
    }
  },

  refreshNode: function (entityTypeCode, parentEntityId, entityId, options) {
    let treeGroup = this.getTreeGroup(this.generateTreeGroupCode(entityTypeCode, parentEntityId));
    if (treeGroup) {
      for (let treeElementId in treeGroup) {
        let tree = this.getTree(treeElementId);
        let nodeCode = tree.convertEntityIdToNodeCode(entityId);

        tree.refreshNode(nodeCode, options);
      }
    }
  },

  refreshNodes: function (entityTypeCode, parentEntityId, ids, options) {
    let self = this;
    if ($q.isNullOrEmpty(ids)) {
      let treeGroup = this.getTreeGroup(this.generateTreeGroupCode(entityTypeCode, parentEntityId));
      if (treeGroup) {
        for (let treeElementId in treeGroup) {
          let tree = this.getTree(treeElementId);
          tree.refreshTree();
        }
      }
    } else {
      jQuery.each(ids, (index, id) => {
        self.refreshNode(entityTypeCode, parentEntityId, id, options);
      });
    }
  },

  removeNode: function (entityTypeCode, parentEntityId, entityId) {
    let treeGroup = this.getTreeGroup(this.generateTreeGroupCode(entityTypeCode, parentEntityId));
    if (treeGroup) {
      for (let treeElementId in treeGroup) {
        let tree = this.getTree(treeElementId);
        let nodeCode = tree.convertEntityIdToNodeCode(entityId);

        tree.removeNode(nodeCode);
      }
    }
  },

  removeNodes: function (entityTypeCode, parentEntityId, ids) {
    let self = this;
    jQuery.each(ids, (index, id) => {
      self.removeNode(entityTypeCode, parentEntityId, id);
    });
  },

  onActionExecuted: function (eventArgs) {
    let entityTypeCode = eventArgs.get_entityTypeCode();
    let parentEntityId = eventArgs.get_parentEntityId();
    let actionTypeCode = eventArgs.get_actionTypeCode();
    let entityId = eventArgs.get_entityId();
    let entityIds = eventArgs.get_isMultipleEntities() ? $o.getEntityIDsFromEntities(eventArgs.get_entities()) : [entityId];

    if (eventArgs.get_isRemoving() || eventArgs.get_isArchiving()) {
      this.removeNodes(entityTypeCode, parentEntityId, entityIds);
    } else if ((eventArgs.get_isUpdated()
      || eventArgs.get_isLoaded()
      || actionTypeCode == window.ACTION_TYPE_CODE_CANCEL
      || actionTypeCode == window.ACTION_TYPE_CODE_CHANGE_LOCK)
      && entityTypeCode != window.ENTITY_TYPE_CODE_VIRTUAL_ARTICLE) {
      this.refreshNode(entityTypeCode, parentEntityId, entityId, { loadChildNodes: true, saveNodesSelection: true });
    } else if (eventArgs.get_isSaved() || actionTypeCode == window.ACTION_TYPE_CODE_COPY) {
      let parentIdsInTree = $o.getParentIdsForTree(entityTypeCode, entityIds);
      this.refreshNodes(entityTypeCode, parentEntityId, parentIdsInTree, { loadChildNodes: true, saveNodesSelection: false });
    } else if (eventArgs.get_isRestoring() && entityTypeCode == window.ENTITY_TYPE_CODE_ARCHIVE_ARTICLE) {
      let parentArticlesInTree = $o.getParentIdsForTree(window.ENTITY_TYPE_CODE_ARTICLE, entityIds);
      this.refreshNodes(window.ENTITY_TYPE_CODE_ARTICLE, parentEntityId, parentArticlesInTree, { loadChildNodes: true, saveNodesSelection: false });
    } else if (eventArgs.get_isRestored() && entityTypeCode == window.ENTITY_TYPE_CODE_ARTICLE_VERSION) {
      let newParentEntityId = +$o.getParentEntityId(window.ENTITY_TYPE_CODE_ARTICLE, entityId) || 0;
      this.refreshNode(window.ENTITY_TYPE_CODE_ARTICLE, newParentEntityId, entityId, { loadChildNodes: true, saveNodesSelection: false });
    }
  },

  dispose: function () {
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
