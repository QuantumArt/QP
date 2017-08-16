window.EVENT_TYPE_CLOSE_HOST_MESSAGE_RECEIVED = 'onCloseHostMessageReceived';

Quantumart.QP8.BackendCustomActionHostManager = function () {
  Quantumart.QP8.BackendCustomActionHostManager.initializeBase(this);
  this._components = {};
};

Quantumart.QP8.BackendCustomActionHostManager.prototype = {
  _components: null,

  createComponent(hostId, options) {
    if (options && options.hostUID) {
      if ($q.isNull(this._components[options.hostUID])) {
        this._components[options.hostUID] = new Quantumart.QP8.BackendCustomActionHost(hostId, options, this);
      }
      return this._components[options.hostUID];
    }
  },

  removeComponent(component) {
    if (component) {
      $q.removeProperty(this._components, component.get_hostUID());
    }
  },

  onCloseHostMessageReceived(message) {
    this.notify(window.EVENT_TYPE_CLOSE_HOST_MESSAGE_RECEIVED, message);
  },

  onExternalCallerContextsUnbinded(message) {
    const self = this;
    $(message.externalCallerContexts).each((i, c) => {
      const component = self._components[c.hostUID];
      if (component) {
        component.onExternalCallerContextsUnbinded({
          reason: message.reason,
          actionUID: c.data.actionUID,
          callerCallback: c.data.callerCallback
        });
      }
    });
  },

  onActionExecuted(eventArgs) {
    let actionInfo = eventArgs;
    if (eventArgs.get_previousAction()) {
      actionInfo = eventArgs.get_previousAction();
    }

    const self = this;
    let callerContexts = [];

    $.merge(callerContexts, [eventArgs.get_externalCallerContext()]);

    let hosts = [];
    $.merge(hosts, Quantumart.QP8.BackendPopupWindowManager.getInstance().getPopupWindowByEventArgs(eventArgs));
    $.merge(hosts, [Quantumart.QP8.BackendEditingArea.getInstance().getDocumentByEventArgs(eventArgs)]);
    if (eventArgs.get_callerContext()) {
      $.merge(hosts, Quantumart.QP8.BackendPopupWindowManager.getInstance().getPopupWindowByEventArgs(eventArgs.get_callerContext().eventArgs));
      $.merge(hosts, [Quantumart.QP8.BackendEditingArea.getInstance().getDocumentByEventArgs(eventArgs.get_callerContext().eventArgs)]);
    }
    hosts = $.grep(hosts, h => !$q.isNull(h));
    if (!$q.isNullOrEmpty(hosts)) {
      $.each(hosts, (k, host) => {
        $.merge(callerContexts, host.get_externalCallerContexts());
      });
    }

    callerContexts = $.grep(callerContexts, c => !$q.isNull(c));
    $(callerContexts).each((i, c) => {
      const component = self._components[c.hostUID];
      if (component) {
        const message = {
          actionCode: actionInfo.get_actionCode(),
          actionTypeCode: actionInfo.get_actionTypeCode(),
          entityTypeCode: actionInfo.get_entityTypeCode(),
          parentEntityId: eventArgs.get_parentEntityId(),
          actionUID: c.data.actionUID,
          callerCallback: c.data.callerCallback
        };
        if (eventArgs.get_isMultipleEntities()) {
          message.isMultipleAction = true;
          message.entityIDs = $o.getEntityIDsFromEntities(eventArgs.get_entities());
        } else {
          message.isMultipleAction = false;
          message.entityId = eventArgs.get_entityId();
        }

        component.onChildHostActionExecuted(message);
      }
    });
  },

  dispose() {
    Quantumart.QP8.BackendCustomActionHostManager.callBaseMethod(this, 'dispose');
    $q.collectGarbageInIE();
  }
};

Quantumart.QP8.BackendCustomActionHostManager._instance = null;

Quantumart.QP8.BackendCustomActionHostManager.getInstance = function () {
  if (Quantumart.QP8.BackendCustomActionHostManager._instance == null) {
    Quantumart.QP8.BackendCustomActionHostManager._instance = new Quantumart.QP8.BackendCustomActionHostManager();
  }

  return Quantumart.QP8.BackendCustomActionHostManager._instance;
};

Quantumart.QP8.BackendCustomActionHostManager.destroyInstance = function () {
  if (Quantumart.QP8.BackendCustomActionHostManager._instance) {
    Quantumart.QP8.BackendCustomActionHostManager._instance.dispose();
    Quantumart.QP8.BackendCustomActionHostManager._instance = null;
  }
};

Quantumart.QP8.BackendCustomActionHostManager.registerClass('Quantumart.QP8.BackendCustomActionHostManager', Quantumart.QP8.Observable);
