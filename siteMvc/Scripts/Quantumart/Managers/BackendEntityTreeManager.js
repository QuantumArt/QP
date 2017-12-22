class BackendEntityTreeManager extends Quantumart.QP8.Observable {
  /** @type {BackendEntityTreeManager} */
  static _instance;

  static getInstance() {
    if (!BackendEntityTreeManager._instance) {
      BackendEntityTreeManager._instance = new BackendEntityTreeManager();
    }

    return BackendEntityTreeManager._instance;
  }

  static destroyInstance() {
    if (BackendEntityTreeManager._instance) {
      BackendEntityTreeManager._instance.dispose();
      BackendEntityTreeManager._instance = null;
    }
  }

  static generateTreeGroupCode(entityTypeCode, parentEntityId) {
    return `${entityTypeCode}_${parentEntityId}`;
  }

  constructor() {
    // @ts-ignore
    super();
    this._treeGroups = {};
  }

  getTreeGroup(treeGroupCode) {
    return this._treeGroups[treeGroupCode];
  }

  createTreeGroup(treeGroupCode) {
    let treeGroup = this.getTreeGroup(treeGroupCode);
    if (!treeGroup) {
      treeGroup = {};
      this._treeGroups[treeGroupCode] = treeGroup;
    }

    return treeGroup;
  }

  removeTreeGroup(treeGroupCode) {
    $q.removeProperty(this._treeGroups, treeGroupCode);
  }

  getTree(treeElementId) {
    const groupCode = Object
      .keys(this._treeGroups)
      .find(treeGroupCode => this._treeGroups[treeGroupCode][treeElementId]);

    return this._treeGroups[groupCode][treeElementId];
  }

  // eslint-disable-next-line max-params
  createTree(treeElementId, entityTypeCode, parentEntityId, actionCode, options, hostOptions) {
    const treeGroupCode = BackendEntityTreeManager.generateTreeGroupCode(entityTypeCode, parentEntityId);
    const tree = options.virtualContentId >= 0
      ? new Quantumart.QP8.BackendVirtualFieldTree(
        treeGroupCode,
        treeElementId,
        entityTypeCode,
        parentEntityId,
        actionCode,
        options
      )
      : new Quantumart.QP8.BackendEntityTree(
        treeGroupCode,
        treeElementId,
        entityTypeCode,
        parentEntityId,
        actionCode,
        options,
        hostOptions
      );

    tree.set_treeManager(this);

    const treeGroup = this.createTreeGroup(treeGroupCode);
    treeGroup[treeElementId] = tree;

    return tree;
  }

  removeTree(treeElementId) {
    const tree = this.getTree(treeElementId);
    if (tree) {
      const treeGroupCode = tree.get_treeGroupCode();
      const treeGroup = this.getTreeGroup(treeGroupCode);

      $q.removeProperty(treeGroup, treeElementId);
      if ($q.getHashKeysCount(treeGroup) === 0) {
        this.removeTreeGroup(treeGroupCode);
      }
    }
  }

  refreshNode(entityTypeCode, parentEntityId, entityId, options) {
    const treeGroup = this.getTreeGroup(BackendEntityTreeManager.generateTreeGroupCode(entityTypeCode, parentEntityId));
    if (treeGroup) {
      Object.keys(treeGroup).forEach(treeElementId => {
        const tree = this.getTree(treeElementId);
        const nodeCode = tree.convertEntityIdToNodeCode(entityId);
        tree.refreshNode(nodeCode, options);
      }, this);
    }
  }

  refreshNodes(entityTypeCode, parentEntityId, ids, options) {
    const that = this;
    if ($q.isNullOrEmpty(ids)) {
      const treeGroup = this.getTreeGroup(
        BackendEntityTreeManager.generateTreeGroupCode(entityTypeCode, parentEntityId)
      );

      if (treeGroup) {
        Object.keys(treeGroup).forEach(treeElementId => {
          const tree = this.getTree(treeElementId);
          tree.refreshTree();
        }, this);
      }
    } else {
      $.each(ids, (index, id) => {
        that.refreshNode(entityTypeCode, parentEntityId, id, options);
      });
    }
  }

  removeNode(entityTypeCode, parentEntityId, entityId) {
    const treeGroup = this.getTreeGroup(
      BackendEntityTreeManager.generateTreeGroupCode(entityTypeCode, parentEntityId)
    );

    if (treeGroup) {
      Object.keys(treeGroup).forEach(treeElementId => {
        const tree = this.getTree(treeElementId);
        const nodeCode = tree.convertEntityIdToNodeCode(entityId);
        tree.removeNode(nodeCode);
      }, this);
    }
  }

  removeNodes(entityTypeCode, parentEntityId, ids) {
    const that = this;
    $.each(ids, (index, id) => {
      that.removeNode(entityTypeCode, parentEntityId, id);
    });
  }

  onActionExecuted(eventArgs) {
    const entityTypeCode = eventArgs.get_entityTypeCode();
    const parentEntityId = eventArgs.get_parentEntityId();
    const actionTypeCode = eventArgs.get_actionTypeCode();
    const entityId = eventArgs.get_entityId();
    const entityIds = eventArgs.get_isMultipleEntities()
      ? $o.getEntityIDsFromEntities(eventArgs.get_entities())
      : [entityId];

    if (eventArgs.get_isRemoving() || eventArgs.get_isArchiving()) {
      this.removeNodes(entityTypeCode, parentEntityId, entityIds);
    } else if ((eventArgs.get_isUpdated()
      || eventArgs.get_isLoaded()
      || actionTypeCode === window.ACTION_TYPE_CODE_CANCEL
      || actionTypeCode === window.ACTION_TYPE_CODE_CHANGE_LOCK)
      && entityTypeCode !== window.ENTITY_TYPE_CODE_VIRTUAL_ARTICLE) {
      this.refreshNode(entityTypeCode, parentEntityId, entityId, { loadChildNodes: true, saveNodesSelection: true });
    } else if (eventArgs.get_isSaved() || actionTypeCode === window.ACTION_TYPE_CODE_COPY) {
      const parentIdsInTree = $o.getParentIdsForTree(entityTypeCode, entityIds);
      this.refreshNodes(entityTypeCode, parentEntityId, parentIdsInTree, {
        loadChildNodes: true,
        saveNodesSelection: false
      });
    } else if (eventArgs.get_isRestoring() && entityTypeCode === window.ENTITY_TYPE_CODE_ARCHIVE_ARTICLE) {
      const parentArticlesInTree = $o.getParentIdsForTree(window.ENTITY_TYPE_CODE_ARTICLE, entityIds);
      this.refreshNodes(window.ENTITY_TYPE_CODE_ARTICLE, parentEntityId, parentArticlesInTree, {
        loadChildNodes: true,
        saveNodesSelection: false
      });
    } else if (eventArgs.get_isRestored() && entityTypeCode === window.ENTITY_TYPE_CODE_ARTICLE_VERSION) {
      const newParentEntityId = +$o.getParentEntityId(window.ENTITY_TYPE_CODE_ARTICLE, entityId) || 0;
      this.refreshNode(window.ENTITY_TYPE_CODE_ARTICLE, newParentEntityId, entityId, {
        loadChildNodes:
          true,
        saveNodesSelection: false
      });
    }
  }

  dispose() {
    super.dispose();
    if (this._trees) {
      Object.keys(this._trees).forEach(treeElementId => {
        const tree = this.getTree(treeElementId);
        if (tree) {
          this.removeTree(treeElementId);
          if (tree.dispose) {
            tree.dispose();
          }
        }
      }, this);
    }

    this._trees = null;
    $q.collectGarbageInIE();
  }
}

Quantumart.QP8.BackendEntityTreeManager = BackendEntityTreeManager;
