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

  getBreadCrumbs: function (breadCrumbsElementId) {
    let breadCrumbs = null;
    if (this._breadCrumbs[breadCrumbsElementId]) {
      breadCrumbs = this._breadCrumbs[breadCrumbsElementId];
    }

    return breadCrumbs;
  },

  createBreadCrumbs: function (breadCrumbsElementId, options) {
    let breadCrumbs = new Quantumart.QP8.BackendBreadCrumbs(breadCrumbsElementId, options);
    breadCrumbs.set_manager(this);
    breadCrumbs.initialize();

    this._breadCrumbs[breadCrumbsElementId] = breadCrumbs;
    return breadCrumbs;
  },

  refreshBreadCrumbs: function (breadCrumbsElementId, callback) {
    let breadCrumbs = this.getBreadCrumbs(breadCrumbsElementId);
    if (breadCrumbs) {
      breadCrumbs.addItemsToBreadCrumbs(callback);
    }
  },

  removeBreadCrumbs: function (breadCrumbsElementId) {
    let breadCrumbs = this.getBreadCrumbs(breadCrumbsElementId);
    if (breadCrumbs) {
      $q.removeProperty(this._breadCrumbs, breadCrumbsElementId);
    }
  },

  destroyBreadCrumbs: function (breadCrumbsElementId) {
    let breadCrumbs = this.getBreadCrumbs(breadCrumbsElementId);
    if (breadCrumbs) {
      this.removeBreadCrumbs(breadCrumbsElementId);
      if (breadCrumbs.dispose) {
        breadCrumbs.dispose();
      }
    }
  },

  generateItemCode: function (entityTypeCode, parentEntityId, entityId) {
    return String.format('{0}_{1}_{2}', entityTypeCode, +parentEntityId || 0, entityId);
  },

  _getBreadCrumbsElementIdByItem: function (itemElem) {
    let breadCrumbsElementId = '';
    let $breadCrumbs = $q.toJQuery(itemElem).parent().parent();
    if (!$q.isNullOrEmpty($breadCrumbs)) {
      breadCrumbsElementId = $breadCrumbs.attr('id');
    }

    $breadCrumbs = null;
    return breadCrumbsElementId;
  },

  getItems: function (itemCode) {
    return $(`#${this._breadCrumbsContainerElementId}`).find(`LI[code = '${itemCode}'].${this.ITEM_CLASS_NAME}`);
  },

  getBreadCrumbsListByItemCode: function (itemCode) {
    let breadCrumbsList = [];
    let $items = this.getItems(itemCode);

    for (let itemIndex = 0, itemCount = $items.length; itemIndex < itemCount; itemIndex++) {
      let breadCrumbsElementId = this._getBreadCrumbsElementIdByItem($items.eq(itemIndex));
      let breadCrumbs = this.getBreadCrumbs(breadCrumbsElementId);
      if (breadCrumbs) {
        Array.add(breadCrumbsList, breadCrumbs);
      }
    }

    return breadCrumbsList;
  },

  refreshBreadCrumbsList: function (entityTypeCode, parentEntityId, entityId) {
    let breadCrumbsList = this.getBreadCrumbsListByItemCode(this.generateItemCode(entityTypeCode, parentEntityId, entityId));

    for (let breadCrumbsIndex = 0, breadCrumbsCount = breadCrumbsList.length; breadCrumbsIndex < breadCrumbsCount; breadCrumbsIndex++) {
      let breadCrumbs = breadCrumbsList[breadCrumbsIndex];
      if (breadCrumbs) {
        breadCrumbs.addItemsToBreadCrumbs();
      }
    }
  },

  onActionExecuted: function (eventArgs) {
    let entityTypeCode = eventArgs.get_entityTypeCode();
    let parentEntityId = eventArgs.get_parentEntityId();
    let actionTypeCode = eventArgs.get_actionTypeCode();
    let entityId = eventArgs.get_entityId();
    let entities = eventArgs.get_entities();

    if (actionTypeCode == window.ACTION_TYPE_CODE_RESTORE || eventArgs.get_isUpdated()) {
      this.refreshBreadCrumbsList(entityTypeCode, parentEntityId, entityId);
    } else if (actionTypeCode == window.ACTION_TYPE_CODE_MULTIPLE_RESTORE) {
      for (let entityIndex = 0, entityCount = entities.length; entityIndex < entityCount; entityIndex++) {
        this.refreshBreadCrumbsList(entityTypeCode, parentEntityId, entities[entityIndex].Id);
      }
    }

    if (actionTypeCode == window.ACTION_TYPE_CODE_RESTORE && entityTypeCode == window.ENTITY_TYPE_CODE_ARTICLE_VERSION) {
      let newEntityTypeCode = window.ENTITY_TYPE_CODE_ARTICLE;
      let newParentEntityId = +$o.getParentEntityId(newEntityTypeCode, parentEntityId) || 0;
      this.refreshBreadCrumbsList(newEntityTypeCode, newParentEntityId, parentEntityId);
    }
  },

  dispose: function () {
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
