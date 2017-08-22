window.EVENT_TYPE_CLOSE_HOST_MESSAGE_RECEIVED = 'onCloseHostMessageReceived';
class BackendCustomActionHostManager extends Quantumart.QP8.Observable {
  static getInstance() {
    if (!BackendCustomActionHostManager._instance) {
      BackendCustomActionHostManager._instance = new BackendCustomActionHostManager();
    }

    return BackendCustomActionHostManager._instance;
  }

  static destroyInstance() {
    if (BackendCustomActionHostManager._instance) {
      BackendCustomActionHostManager._instance.dispose();
      BackendCustomActionHostManager._instance = null;
    }
  }

  constructor() {
    super();
    this._components = {};
  }

  createComponent(hostId, options) {
    if (options && options.hostUID) {
      if ($q.isNull(this._components[options.hostUID])) {
        this._components[options.hostUID] = new Quantumart.QP8.BackendCustomActionHost(hostId, options, this);
      }

      return this._components[options.hostUID];
    }

    return undefined;
  }

  removeComponent(component) {
    if (component) {
      $q.removeProperty(this._components, component.get_hostUID());
    }
  }

  onCloseHostMessageReceived(message) {
    this.notify(window.EVENT_TYPE_CLOSE_HOST_MESSAGE_RECEIVED, message);
  }

  onExternalCallerContextsUnbinded(message) {
    const that = this;
    $(message.externalCallerContexts).each((i, ctx) => {
      const component = that._components[ctx.hostUID];
      if (component) {
        component.onExternalCallerContextsUnbinded({
          reason: message.reason,
          actionUID: ctx.data.actionUID,
          callerCallback: ctx.data.callerCallback
        });
      }
    });
  }

  onActionExecuted(eventArgs) {
    let actionInfo = eventArgs;
    if (eventArgs.get_previousAction()) {
      actionInfo = eventArgs.get_previousAction();
    }

    const that = this;
    let callerContexts = [];
    $.merge(callerContexts, [eventArgs.get_externalCallerContext()]);

    let hosts = [];
    $.merge(hosts, Quantumart.QP8.BackendPopupWindowManager.getInstance().getPopupWindowByEventArgs(eventArgs));
    $.merge(hosts, [Quantumart.QP8.BackendEditingArea.getInstance().getDocumentByEventArgs(eventArgs)]);
    if (eventArgs.get_callerContext()) {
      $.merge(hosts, Quantumart.QP8.BackendPopupWindowManager.getInstance().getPopupWindowByEventArgs(
        eventArgs.get_callerContext().eventArgs
      ));

      $.merge(hosts, [Quantumart.QP8.BackendEditingArea.getInstance().getDocumentByEventArgs(
        eventArgs.get_callerContext().eventArgs
      )]);
    }

    hosts = $.grep(hosts, host => !$q.isNull(host));
    if (!$q.isNullOrEmpty(hosts)) {
      $.each(hosts, (i, host) => {
        $.merge(callerContexts, host.get_externalCallerContexts());
      });
    }

    callerContexts = $.grep(callerContexts, ctx => !$q.isNull(ctx));
    $(callerContexts).each((i, ctx) => {
      const component = that._components[ctx.hostUID];
      if (component) {
        const message = {
          actionCode: actionInfo.get_actionCode(),
          actionTypeCode: actionInfo.get_actionTypeCode(),
          entityTypeCode: actionInfo.get_entityTypeCode(),
          parentEntityId: eventArgs.get_parentEntityId(),
          actionUID: ctx.data.actionUID,
          callerCallback: ctx.data.callerCallback
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
  }

  dispose() {
    super.dispose();
    $q.collectGarbageInIE();
  }
}

Quantumart.QP8.BackendCustomActionHostManager = BackendCustomActionHostManager;
