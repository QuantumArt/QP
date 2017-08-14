window.EVENT_TYPE_CUSTOM_ACTION_CHANGED = "OnCustomActionChanged";

Quantumart.QP8.BackendContextMenuManager = function () {
  Quantumart.QP8.BackendContextMenuManager.initializeBase(this);
};

Quantumart.QP8.BackendContextMenuManager.prototype = {
  dispose: function () {
    Quantumart.QP8.BackendContextMenuManager.callBaseMethod(this, "dispose");
    Quantumart.QP8.BackendContextMenuManager._instance = null;
    $q.collectGarbageInIE();
  },

  onActionExecuted: function (eventArgs) {
    if (eventArgs && eventArgs.get_entityTypeCode() === window.ENTITY_TYPE_CODE_CUSTOM_ACTION && (eventArgs.get_isSaved() || eventArgs.get_isUpdated() || eventArgs.get_isRemoving())) {
      this.notify(window.EVENT_TYPE_CUSTOM_ACTION_CHANGED, {});
    }
  }
};

Quantumart.QP8.BackendContextMenuManager._instance = null;
Quantumart.QP8.BackendContextMenuManager.getInstance = function () {
  if (Quantumart.QP8.BackendContextMenuManager._instance === null) {
    Quantumart.QP8.BackendContextMenuManager._instance = new Quantumart.QP8.BackendContextMenuManager();
  }

  return Quantumart.QP8.BackendContextMenuManager._instance;
};

Quantumart.QP8.BackendContextMenuManager.destroyInstance = function () {
  if (Quantumart.QP8.BackendContextMenuManager._instance) {
    Quantumart.QP8.BackendContextMenuManager._instance.dispose();
  }
};

Quantumart.QP8.BackendContextMenuManager.registerClass("Quantumart.QP8.BackendContextMenuManager", Quantumart.QP8.Observable);
