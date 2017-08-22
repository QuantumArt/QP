class BackendBreadCrumbsManager extends Quantumart.QP8.Observable {
  static getInstance(breadCrumbsContainerElementId, options) {
    if (!BackendBreadCrumbsManager._instance) {
      BackendBreadCrumbsManager._instance = new BackendBreadCrumbsManager(breadCrumbsContainerElementId, options);
    }

    return BackendBreadCrumbsManager._instance;
  }

  static destroyInstance() {
    if (BackendBreadCrumbsManager._instance) {
      BackendBreadCrumbsManager._instance.dispose();
      BackendBreadCrumbsManager._instance = null;
    }
  }

  static generateItemCode(entityTypeCode, parentEntityId, entityId) {
    return `${entityTypeCode}_${+parentEntityId || 0}_${entityId}`;
  }

  static getBreadCrumbsElementIdByItem(itemElem) {
    const $breadCrumbs = $q.toJQuery(itemElem).parent().parent();
    return $breadCrumbs ? $breadCrumbs.attr('id') : '';
  }

  constructor(breadCrumbsContainerElementId, options) {
    super(breadCrumbsContainerElementId, options);
    this._breadCrumbsContainerElementId = breadCrumbsContainerElementId;

    if (options) {
      this._contextMenuManager = options.contextMenuManager;
    }

    this._breadCrumbs = {};
    this.ITEM_CLASS_NAME = 'item';
    this.validate();
  }

  validate() {
    if (!this._breadCrumbsContainerElementId) {
      throw new Error($l.BreadCrumbs.breadCrumbsContainerElementIdNotSpecified);
    }
  }

  getBreadCrumbs(breadCrumbsElementId) {
    return this._breadCrumbs[breadCrumbsElementId];
  }

  createBreadCrumbs(breadCrumbsElementId, options) {
    const breadCrumbs = new Quantumart.QP8.BackendBreadCrumbs(breadCrumbsElementId, options);
    breadCrumbs.set_manager(this);
    breadCrumbs.initialize();

    this._breadCrumbs[breadCrumbsElementId] = breadCrumbs;
    return breadCrumbs;
  }

  refreshBreadCrumbs(breadCrumbsElementId, callback) {
    const breadCrumbs = this.getBreadCrumbs(breadCrumbsElementId);
    if (breadCrumbs) {
      breadCrumbs.addItemsToBreadCrumbs(callback);
    }
  }

  getItems(itemCode) {
    return $(`#${this._breadCrumbsContainerElementId}`).find(`LI[code = '${itemCode}'].${this.ITEM_CLASS_NAME}`);
  }

  getBreadCrumbsListByItemCode(itemCode) {
    const breadCrumbsList = [];
    const $items = this.getItems(itemCode);

    for (let itemIndex = 0; itemIndex < $items.length; itemIndex++) {
      const breadCrumbsElementId = BackendBreadCrumbsManager.getBreadCrumbsElementIdByItem($items.eq(itemIndex));
      const breadCrumbs = this.getBreadCrumbs(breadCrumbsElementId);
      if (breadCrumbs) {
        Array.add(breadCrumbsList, breadCrumbs);
      }
    }

    return breadCrumbsList;
  }

  refreshBreadCrumbsList(entityTypeCode, parentEntityId, entityId) {
    const breadCrumbsList = this.getBreadCrumbsListByItemCode(
      BackendBreadCrumbsManager.generateItemCode(entityTypeCode, parentEntityId, entityId)
    );

    for (let breadCrumbsIndex = 0; breadCrumbsIndex < breadCrumbsList.length; breadCrumbsIndex++) {
      const breadCrumbs = breadCrumbsList[breadCrumbsIndex];
      if (breadCrumbs) {
        breadCrumbs.addItemsToBreadCrumbs();
      }
    }
  }

  onActionExecuted(eventArgs) {
    const entityTypeCode = eventArgs.get_entityTypeCode();
    const parentEntityId = eventArgs.get_parentEntityId();
    const actionTypeCode = eventArgs.get_actionTypeCode();
    const entityId = eventArgs.get_entityId();
    const entities = eventArgs.get_entities();

    if (actionTypeCode === window.ACTION_TYPE_CODE_RESTORE || eventArgs.get_isUpdated()) {
      this.refreshBreadCrumbsList(entityTypeCode, parentEntityId, entityId);
    } else if (actionTypeCode === window.ACTION_TYPE_CODE_MULTIPLE_RESTORE) {
      for (let entityIndex = 0; entityIndex < entities.length; entityIndex++) {
        this.refreshBreadCrumbsList(entityTypeCode, parentEntityId, entities[entityIndex].Id);
      }
    }

    if (actionTypeCode === window.ACTION_TYPE_CODE_RESTORE
      && entityTypeCode === window.ENTITY_TYPE_CODE_ARTICLE_VERSION) {
      const newParentEntityId = +$o.getParentEntityId(window.ENTITY_TYPE_CODE_ARTICLE, parentEntityId) || 0;
      this.refreshBreadCrumbsList(window.ENTITY_TYPE_CODE_ARTICLE, newParentEntityId, parentEntityId);
    }
  }

  destroyBreadCrumbs(breadCrumbsElementId) {
    const breadCrumbs = this.getBreadCrumbs(breadCrumbsElementId);
    if (breadCrumbs) {
      $q.removeProperty(this._breadCrumbs, breadCrumbsElementId);
      if (breadCrumbs.dispose) {
        breadCrumbs.dispose();
      }
    }
  }

  dispose() {
    super.dispose();
    if (this._breadCrumbs) {
      Object.keys(this._breadCrumbs).forEach(this.destroyBreadCrumbs, this);
    }

    this._breadCrumbs = null;
    $q.collectGarbageInIE();
  }
}

Quantumart.QP8.BackendBreadCrumbsManager = BackendBreadCrumbsManager;
