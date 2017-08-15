Quantumart.QP8.BackendActionPermissionViewManager = function () {
  Quantumart.QP8.BackendActionPermissionViewManager.initializeBase(this);
};

Quantumart.QP8.BackendActionPermissionViewManager.prototype = {
  _viewComponent: null,

  createView (viewElementId, options, hostOptions) {
    if (this._viewComponent == null) {
      this._viewComponent = new Quantumart.QP8.BackendActionPermissionView(viewElementId, options, hostOptions);
    }
    return this._viewComponent;
  },

  onActionExecuted (eventArgs) {
    const entityTypeCode = eventArgs.get_entityTypeCode();
    const parentEntityId = eventArgs.get_parentEntityId();
    const actionCode = eventArgs.get_actionCode();
    if (eventArgs.get_isSaved() || eventArgs.get_isUpdated() || eventArgs.get_isRemoving()) {
      if (entityTypeCode == window.ENTITY_TYPE_CODE_ENTITY_TYPE_PERMISSION || entityTypeCode == window.ENTITY_TYPE_CODE_ACTION_PERMISSION) {
        this._viewComponent.get_Tree().refreshPermissionNode(entityTypeCode, parentEntityId);
      }
    } else if (actionCode == window.ACTION_CODE_CHANGE_ENTITY_TYPE_PERMISSION_NODE || actionCode == window.ACTION_CODE_CHANGE_ACTION_PERMISSION_NODE) {
      this._viewComponent.get_Tree().refreshPermissionNode(entityTypeCode, parentEntityId);
    }
  },

  destroyView () {
    this._viewComponent = null;
  },

  dispose () {
    this.destroyView();
  }
};


Quantumart.QP8.BackendActionPermissionViewManager._instance = null;

Quantumart.QP8.BackendActionPermissionViewManager.getInstance = function () {
  if (Quantumart.QP8.BackendActionPermissionViewManager._instance == null) {
    Quantumart.QP8.BackendActionPermissionViewManager._instance = new Quantumart.QP8.BackendActionPermissionViewManager();
  }

  return Quantumart.QP8.BackendActionPermissionViewManager._instance;
};

// Уничтожает экземпляр класса "Менеджер библиотек"
Quantumart.QP8.BackendActionPermissionViewManager.destroyInstance = function () {
  if (Quantumart.QP8.BackendActionPermissionViewManager._instance) {
    Quantumart.QP8.BackendActionPermissionViewManager._instance.dispose();
  }
};

Quantumart.QP8.BackendActionPermissionViewManager.registerClass('Quantumart.QP8.BackendActionPermissionViewManager', Quantumart.QP8.Observable);
