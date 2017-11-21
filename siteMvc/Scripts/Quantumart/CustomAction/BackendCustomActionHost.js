window.EVENT_TYPE_EXTERNAL_ACTION_EXECUTING = 'OnExternalActionExecuting';

Quantumart.QP8.BackendCustomActionHost = function (hostId, options, manager) {
  Quantumart.QP8.BackendCustomActionHost.initializeBase(this);

  this._hostId = hostId;
  this._options = options;
  this._manager = manager;
};

Quantumart.QP8.BackendCustomActionHost.prototype = {
  _options: null,
  _hostId: '',
  _manager: null,

  initialize() {
    pmrpc.register({
      publicProcedureName: this._getExecuteActionProcedureName(),
      procedure: $.proxy(this._onExternalMessageReceived, this),
      isAsynchronous: true
    });

    $(`#${this._options.iframeElementId}`).attr('src', this._generateActionUrl());
  },

  _onExternalMessageReceived(message, successCallback) {
    if (message.type === Quantumart.QP8.Interaction.ExternalMessageTypes.ExecuteAction) {
      this._onExecuteActionMessageReceived(message);
      successCallback(0);
    } else if (message.type === Quantumart.QP8.Interaction.ExternalMessageTypes.CloseBackendHost) {
      this._onCloseHostMessageReceived(message);
      successCallback(0);
    } else if (message.type === Quantumart.QP8.Interaction.ExternalMessageTypes.OpenSelectWindow) {
      this._onOpenSelectWindowMessageReceived(message);
      successCallback(0);
    } else if (message.type === Quantumart.QP8.Interaction.ExternalMessageTypes.CheckHost) {
      successCallback(this._onCheckHostMessageReceived(message));
    }
  },

  onSelect() {
    const id = this._options.iframeElementId;
    $(`#${id}`).css('marginLeft', '1px');
    setTimeout(() => {
      $(`#${id}`).css('marginLeft', '0');
    }, 0);
  },

  _onCheckHostMessageReceived() {
    return window.BACKEND_VERSION;
  },

  _onCloseHostMessageReceived(message) {
    this._manager.onCloseHostMessageReceived(message);
  },

  _onExecuteActionMessageReceived(message) {
    const action = $a.getBackendActionByCode(message.data.actionCode);
    const params = new Quantumart.QP8.BackendActionParameters({
      entityTypeCode: message.data.entityTypeCode,
      entityId: message.data.entityId,
      entityName: $o.getEntityName(message.data.entityTypeCode, message.data.entityId, message.data.parentEntityId),
      parentEntityId: message.data.parentEntityId
    });

    params.correct(action);

    const eventArgs = $a.getEventArgsFromActionWithParams(action, params);
    eventArgs.set_externalCallerContext(message);
    if (!$q.toBoolean(message.data.changeCurrentTab, false) && !$q.isNull(message.data.isWindow)) {
      eventArgs.set_isWindow(
        $q.toBoolean(
          message.data.isWindow,
          eventArgs.get_isWindow()
        )
      );
    }

    if (message.data.options && message.data.options.currentContext) {
      // eslint-disable-next-line no-param-reassign
      message.data.options.contextQuery = JSON.stringify(
        $o.getContextQuery(message.data.parentEntityId, message.data.options.currentContext)
      );
    }

    eventArgs.set_additionalData(message.data.options);
    eventArgs.set_startedByExternal(true);
    this.notify(window.EVENT_TYPE_EXTERNAL_ACTION_EXECUTING, eventArgs);
  },

  _onOpenSelectWindowMessageReceived(message) {
    const eventArgs = new Quantumart.QP8.BackendEventArgs();
    eventArgs.set_isMultipleEntities(message.data.isMultiple);
    eventArgs.set_parentEntityId(message.data.parentEntityId);
    eventArgs.set_entityTypeCode(message.data.entityTypeCode);
    eventArgs.set_actionCode(message.data.selectActionCode);

    if ($q.isArray(message.data.selectedEntityIDs) && !$q.isNullOrEmpty(message.data.selectedEntityIDs)) {
      const selectedEntities = message.data.selectedEntityIDs.map(id => ({ Id: id }));
      if (message.data.isMultiple) {
        eventArgs.set_entities(selectedEntities);
      } else {
        eventArgs.set_entityId(selectedEntities[0].Id);
      }
    }

    const selectPopupWindowComponent = new Quantumart.QP8.BackendSelectPopupWindow(eventArgs, message.data.options);
    selectPopupWindowComponent.callerCallback = message.data.callerCallback;
    selectPopupWindowComponent.selectWindowUID = message.data.selectWindowUID;
    selectPopupWindowComponent.attachObserver(
      window.EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED,
      $.proxy(this._popupWindowSelectedHandler, this)
    );

    selectPopupWindowComponent.attachObserver(
      window.EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED,
      $.proxy(this._popupWindowClosedHandler, this)
    );

    selectPopupWindowComponent.openWindow();
  },

  _popupWindowSelectedHandler(eventType, sender, args) {
    let { entities: selectedEntities } = args;
    let selectedEntityIDs = selectedEntities.map(el => el.Id);
    if ($o.checkEntitiesForPresenceEmptyNames(selectedEntities)) {
      if (args.entityTypeCode && args.parentEntityId) {
        const dataItems = $o.getSimpleEntityList(
          args.entityTypeCode,
          args.parentEntityId,
          0,
          0,
          window.$e.ListSelectionMode.OnlySelectedItems,
          selectedEntityIDs
        );

        selectedEntities = $c.getEntitiesFromListItemCollection(dataItems);
        selectedEntityIDs = selectedEntities.map(el => el.Id);
      }
    }

    this._invokeCallback(Quantumart.QP8.Interaction.BackendEventTypes.EntitiesSelected, {
      selectedEntityIDs,
      selectedEntities,
      callerCallback: sender.callerCallback,
      selectWindowUID: sender.selectWindowUID
    });

    this._destroySelectPopupWindow(sender);
  },

  _popupWindowClosedHandler(eventType, sender) {
    this._destroySelectPopupWindow(sender);
  },

  _destroySelectPopupWindow(popupWindows) {
    if (!$q.isNull(popupWindows)) {
      popupWindows.detachObserver(window.EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED);
      popupWindows.detachObserver(window.EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED);
      popupWindows.closeWindow();
      popupWindows.dispose();

      this._invokeCallback(Quantumart.QP8.Interaction.BackendEventTypes.SelectWindowClosed, {
        callerCallback: popupWindows.callerCallback,
        selectWindowUID: popupWindows.selectWindowUID
      });
    }
  },

  // eslint-disable-next-line camelcase
  get_hostUID() {
    return this._options.hostUID;
  },

  onExternalCallerContextsUnbinded(message) {
    this._invokeCallback(Quantumart.QP8.Interaction.BackendEventTypes.HostUnbinded, message);
  },

  onChildHostActionExecuted(message) {
    this._invokeCallback(Quantumart.QP8.Interaction.BackendEventTypes.ActionExecuted, message);
  },

  _invokeCallback(type, message) {
    const iframe = window.document.getElementById(this._options.iframeElementId);
    if (iframe && iframe.contentWindow) {
      const args = {};
      Object.assign(args, message);
      delete args.callerCallback;
      pmrpc.call({
        destination: iframe.contentWindow,
        publicProcedureName: message.callerCallback,
        params: [type, args]
      });
    }
  },

  _getExecuteActionProcedureName() {
    return this._options.hostUID;
  },

  _generateActionUrl() {
    let resultUrl = $q.updateQueryStringParameter(this._options.actionBaseUrl, 'hostUID', this._options.hostUID);
    if (this._options.additionalParams) {
      resultUrl += `&${$.param(this._options.additionalParams)}`;
    }
    return resultUrl;
  },

  dispose() {
    pmrpc.unregister(this._getExecuteActionProcedureName());
    this._manager.removeComponent(this);
  }
};

Quantumart.QP8.BackendCustomActionHost.registerClass(
  'Quantumart.QP8.BackendCustomActionHost',
  Quantumart.QP8.Observable
);
