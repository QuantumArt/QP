class BackendActionPermissionViewManager extends Quantumart.QP8.Observable {
  static getInstance() {
    if (!BackendActionPermissionViewManager._instance) {
      BackendActionPermissionViewManager._instance = new BackendActionPermissionViewManager();
    }

    return BackendActionPermissionViewManager._instance;
  }

  static destroyInstance() {
    if (BackendActionPermissionViewManager._instance) {
      BackendActionPermissionViewManager._instance.dispose();
      BackendActionPermissionViewManager._instance = null;
    }
  }

  createView(viewElementId, options, hostOptions) {
    if (!this._viewComponent) {
      this._viewComponent = new Quantumart.QP8.BackendActionPermissionView(viewElementId, options, hostOptions);
    }

    return this._viewComponent;
  }

  onActionExecuted(eventArgs) {
    const entityTypeCode = eventArgs.get_entityTypeCode();
    const parentEntityId = eventArgs.get_parentEntityId();
    const actionCode = eventArgs.get_actionCode();
    if (eventArgs.get_isSaved() || eventArgs.get_isUpdated() || eventArgs.get_isRemoving()) {
      if (entityTypeCode === window.ENTITY_TYPE_CODE_ENTITY_TYPE_PERMISSION
        || entityTypeCode === window.ENTITY_TYPE_CODE_ACTION_PERMISSION
      ) {
        this._viewComponent.getTree().refreshPermissionNode(entityTypeCode, parentEntityId);
      }
    } else if (actionCode === window.ACTION_CODE_CHANGE_ENTITY_TYPE_PERMISSION_NODE
      || actionCode === window.ACTION_CODE_CHANGE_ACTION_PERMISSION_NODE
    ) {
      this._viewComponent.getTree().refreshPermissionNode(entityTypeCode, parentEntityId);
    }
  }

  dispose() {
    super.dispose();
    this._viewComponent = null;
    $q.collectGarbageInIE();
  }
}

Quantumart.QP8.BackendActionPermissionViewManager = BackendActionPermissionViewManager;
