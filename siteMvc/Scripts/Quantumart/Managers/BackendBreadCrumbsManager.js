Quantumart.QP8.BackendBreadCrumbsManager = function (breadCrumbsContainerElementId, options) {
  Quantumart.QP8.BackendBreadCrumbsManager.initializeBase(this);

  if (!breadCrumbsContainerElementId) {
    throw new Error($l.BreadCrumbs.breadCrumbsContainerElementIdNotSpecified);
  }

  this._breadCrumbsContainerElementId = breadCrumbsContainerElementId;
  if (options && options.contextMenuManager) {
    this._contextMenuManager = options.contextMenuManager;
  }
};

Quantumart.QP8.BackendBreadCrumbsManager.prototype = {
  _contextMenuManager: null,
  _breadCrumbsContainerElementId: '',
  _breadCrumbs: {},
  ITEM_CLASS_NAME: 'item',

  getBreadCrumbs (breadCrumbsElementId) {
    let breadCrumbs = null;
    if (this._breadCrumbs[breadCrumbsElementId]) {
      breadCrumbs = this._breadCrumbs[breadCrumbsElementId];
    }

    return breadCrumbs;
  },

  createBreadCrumbs (breadCrumbsElementId, options) {
    const breadCrumbs = new Quantumart.QP8.BackendBreadCrumbs(breadCrumbsElementId, options);
    breadCrumbs.set_manager(this);
    breadCrumbs.initialize();

    this._breadCrumbs[breadCrumbsElementId] = breadCrumbs;
    return breadCrumbs;
  },

  refreshBreadCrumbs (breadCrumbsElementId, callback) {
    const breadCrumbs = this.getBreadCrumbs(breadCrumbsElementId);
    if (breadCrumbs) {
      breadCrumbs.addItemsToBreadCrumbs(callback);
    }
  },

  removeBreadCrumbs (breadCrumbsElementId) {
    const breadCrumbs = this.getBreadCrumbs(breadCrumbsElementId);
    if (breadCrumbs) {
      $q.removeProperty(this._breadCrumbs, breadCrumbsElementId);
    }
  },

  destroyBreadCrumbs (breadCrumbsElementId) {
    const breadCrumbs = this.getBreadCrumbs(breadCrumbsElementId);
    if (breadCrumbs) {
      this.removeBreadCrumbs(breadCrumbsElementId);
      if (breadCrumbs.dispose) {
        breadCrumbs.dispose();
      }
    }
  },

  generateItemCode (entityTypeCode, parentEntityId, entityId) {
    return String.format('{0}_{1}_{2}', entityTypeCode, +parentEntityId || 0, entityId);
  },

  _getBreadCrumbsElementIdByItem (itemElem) {
    let breadCrumbsElementId = '';
    let $breadCrumbs = $q.toJQuery(itemElem).parent().parent();
    if (!$q.isNullOrEmpty($breadCrumbs)) {
      breadCrumbsElementId = $breadCrumbs.attr('id');
    }

    $breadCrumbs = null;
    return breadCrumbsElementId;
  },

  getItems (itemCode) {
    return $(`#${this._breadCrumbsContainerElementId}`).find(`LI[code = '${itemCode}'].${this.ITEM_CLASS_NAME}`);
  },

  getBreadCrumbsListByItemCode (itemCode) {
    const breadCrumbsList = [];
    const $items = this.getItems(itemCode);

    for (let itemIndex = 0, itemCount = $items.length; itemIndex < itemCount; itemIndex++) {
      const breadCrumbsElementId = this._getBreadCrumbsElementIdByItem($items.eq(itemIndex));
      const breadCrumbs = this.getBreadCrumbs(breadCrumbsElementId);
      if (breadCrumbs) {
        Array.add(breadCrumbsList, breadCrumbs);
      }
    }

    return breadCrumbsList;
  },

  refreshBreadCrumbsList (entityTypeCode, parentEntityId, entityId) {
    const breadCrumbsList = this.getBreadCrumbsListByItemCode(this.generateItemCode(entityTypeCode, parentEntityId, entityId));

    for (let breadCrumbsIndex = 0, breadCrumbsCount = breadCrumbsList.length; breadCrumbsIndex < breadCrumbsCount; breadCrumbsIndex++) {
      const breadCrumbs = breadCrumbsList[breadCrumbsIndex];
      if (breadCrumbs) {
        breadCrumbs.addItemsToBreadCrumbs();
      }
    }
  },

  onActionExecuted (eventArgs) {
    const entityTypeCode = eventArgs.get_entityTypeCode();
    const parentEntityId = eventArgs.get_parentEntityId();
    const actionTypeCode = eventArgs.get_actionTypeCode();
    const entityId = eventArgs.get_entityId();
    const entities = eventArgs.get_entities();

    if (actionTypeCode == window.ACTION_TYPE_CODE_RESTORE || eventArgs.get_isUpdated()) {
      this.refreshBreadCrumbsList(entityTypeCode, parentEntityId, entityId);
    } else if (actionTypeCode == window.ACTION_TYPE_CODE_MULTIPLE_RESTORE) {
      for (let entityIndex = 0, entityCount = entities.length; entityIndex < entityCount; entityIndex++) {
        this.refreshBreadCrumbsList(entityTypeCode, parentEntityId, entities[entityIndex].Id);
      }
    }

    if (actionTypeCode == window.ACTION_TYPE_CODE_RESTORE && entityTypeCode == window.ENTITY_TYPE_CODE_ARTICLE_VERSION) {
      const newEntityTypeCode = window.ENTITY_TYPE_CODE_ARTICLE;
      const newParentEntityId = +$o.getParentEntityId(newEntityTypeCode, parentEntityId) || 0;
      this.refreshBreadCrumbsList(newEntityTypeCode, newParentEntityId, parentEntityId);
    }
  },

  dispose () {
    Quantumart.QP8.BackendBreadCrumbsManager.callBaseMethod(this, 'dispose');
    if (this._breadCrumbs) {
      Object.keys(this._breadCrumbs).forEach(this.destroyBreadCrumbs);
    }

    Quantumart.QP8.BackendBreadCrumbsManager._instance = null;
  }
};

Quantumart.QP8.BackendBreadCrumbsManager._instance = null;

Quantumart.QP8.BackendBreadCrumbsManager.getInstance = function (breadCrumbsContainerElementId, options) {
  if (Quantumart.QP8.BackendBreadCrumbsManager._instance == null) {
    Quantumart.QP8.BackendBreadCrumbsManager._instance = new Quantumart.QP8.BackendBreadCrumbsManager(breadCrumbsContainerElementId, options);
  }

  return Quantumart.QP8.BackendBreadCrumbsManager._instance;
};

Quantumart.QP8.BackendBreadCrumbsManager.destroyInstance = function () {
  if (Quantumart.QP8.BackendBreadCrumbsManager._instance) {
    Quantumart.QP8.BackendBreadCrumbsManager._instance.dispose();
  }
};

Quantumart.QP8.BackendBreadCrumbsManager.registerClass('Quantumart.QP8.BackendBreadCrumbsManager', Quantumart.QP8.Observable);
