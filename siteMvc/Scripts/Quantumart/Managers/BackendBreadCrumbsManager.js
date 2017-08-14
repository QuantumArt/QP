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
    var breadCrumbs = null;
    if (this._breadCrumbs[breadCrumbsElementId]) {
      breadCrumbs = this._breadCrumbs[breadCrumbsElementId];
    }

    return breadCrumbs;
  },

  createBreadCrumbs: function (breadCrumbsElementId, options) {
    var breadCrumbs = new Quantumart.QP8.BackendBreadCrumbs(breadCrumbsElementId, options);
    breadCrumbs.set_manager(this);
    breadCrumbs.initialize();

    this._breadCrumbs[breadCrumbsElementId] = breadCrumbs;
    return breadCrumbs;
  },

  refreshBreadCrumbs: function (breadCrumbsElementId, callback) {
    var breadCrumbs = this.getBreadCrumbs(breadCrumbsElementId);
    if (breadCrumbs) {
      breadCrumbs.addItemsToBreadCrumbs(callback);
    }
  },

  removeBreadCrumbs: function (breadCrumbsElementId) {
    var breadCrumbs = this.getBreadCrumbs(breadCrumbsElementId);
    if (breadCrumbs) {
      $q.removeProperty(this._breadCrumbs, breadCrumbsElementId);
    }
  },

  destroyBreadCrumbs: function (breadCrumbsElementId) {
    var breadCrumbs = this.getBreadCrumbs(breadCrumbsElementId);
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
    var breadCrumbsElementId = '';
    var $breadCrumbs = $q.toJQuery(itemElem).parent().parent();
    if (!$q.isNullOrEmpty($breadCrumbs)) {
      breadCrumbsElementId = $breadCrumbs.attr('id');
    }

    $breadCrumbs = null;
    return breadCrumbsElementId;
  },

  getItems: function (itemCode) {
    return $('#' + this._breadCrumbsContainerElementId).find("LI[code = '" + itemCode + "']." + this.ITEM_CLASS_NAME);
  },

  getBreadCrumbsListByItemCode: function (itemCode) {
    var breadCrumbsList = [];
    var $items = this.getItems(itemCode);

    for (var itemIndex = 0, itemCount = $items.length; itemIndex < itemCount; itemIndex++) {
      var breadCrumbsElementId = this._getBreadCrumbsElementIdByItem($items.eq(itemIndex));
      var breadCrumbs = this.getBreadCrumbs(breadCrumbsElementId);
      if (breadCrumbs) {
        Array.add(breadCrumbsList, breadCrumbs);
      }
    }

    return breadCrumbsList;
  },

  refreshBreadCrumbsList: function (entityTypeCode, parentEntityId, entityId) {
    var breadCrumbsList = this.getBreadCrumbsListByItemCode(this.generateItemCode(entityTypeCode, parentEntityId, entityId));

    for (var breadCrumbsIndex = 0, breadCrumbsCount = breadCrumbsList.length; breadCrumbsIndex < breadCrumbsCount; breadCrumbsIndex++) {
      var breadCrumbs = breadCrumbsList[breadCrumbsIndex];
      if (breadCrumbs) {
        breadCrumbs.addItemsToBreadCrumbs();
      }
    }
  },

  onActionExecuted: function (eventArgs) {
    var entityTypeCode = eventArgs.get_entityTypeCode();
    var parentEntityId = eventArgs.get_parentEntityId();
    var actionTypeCode = eventArgs.get_actionTypeCode();
    var entityId = eventArgs.get_entityId();
    var entities = eventArgs.get_entities();

    if (actionTypeCode == window.ACTION_TYPE_CODE_RESTORE || eventArgs.get_isUpdated()) {
      this.refreshBreadCrumbsList(entityTypeCode, parentEntityId, entityId);
    } else if (actionTypeCode == window.ACTION_TYPE_CODE_MULTIPLE_RESTORE) {
      for (var entityIndex = 0, entityCount = entities.length; entityIndex < entityCount; entityIndex++) {
        this.refreshBreadCrumbsList(entityTypeCode, parentEntityId, entities[entityIndex].Id);
      }
    }

    if (actionTypeCode == window.ACTION_TYPE_CODE_RESTORE && entityTypeCode == window.ENTITY_TYPE_CODE_ARTICLE_VERSION) {
      var newEntityTypeCode = window.ENTITY_TYPE_CODE_ARTICLE;
      var newParentEntityId = +$o.getParentEntityId(newEntityTypeCode, parentEntityId) || 0;
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
