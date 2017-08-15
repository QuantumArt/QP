window.Quantumart = window.Quantumart || {};
window.Quantumart.QP8 = window.Quantumart.QP8 || {};
window.Quantumart.QP8.Interaction = window.Quantumart.QP8.Interaction || {};
window.Quantumart.QP8.Interaction = (function Interaction() {
  /* eslint-disable no-empty-function */
  const ArticleFormState = function () {};
  const ExecuteActionOptions = function () {};
  const BackendExternalMessage = function () {};
  const OpenSelectWindowOptions = function () {};

  /* eslint-enable no-empty-function */
  BackendExternalMessage.prototype = {
    type: '',
    hostUID: null,
    data: null
  };

  BackendExternalMessage.Types = {
    ExecuteAction: 1,
    CloseBackendHost: 2,
    OpenSelectWindow: 3,
    CheckHost: 4
  };

  ExecuteActionOptions.prototype = {
    actionCode: '',
    entityTypeCode: '',
    parentEntityId: 0,
    entityId: 0,

    actionUID: null,
    callerCallback: '',
    changeCurrentTab: false,
    isWindow: false,

    options: null
  };

  ArticleFormState.prototype = {
    initFieldValues: null,
    disabledFields: null,
    hideFields: null,
    disabledActionCodes: null,
    additionalParams: null
  };

  ArticleFormState.InitFieldValue = Object.create({});
  ArticleFormState.InitFieldValue.prototype = {
    fieldName: '',
    value: null
  };

  OpenSelectWindowOptions.prototype = {
    selectActionCode: '',
    entityTypeCode: '',
    parentEntityId: 0,
    isMultiple: false,
    selectedEntityIDs: null,
    selectWindowUID: null,
    callerCallback: '',
    options: null
  };

  const BackendEventObserver = function (callbackProcName, callback) {
    this.callbackProcName = callbackProcName;
    this.callback = callback;
    pmrpc.register({
      publicProcedureName: this.callbackProcName,
      procedure: this.callback,
      isAsynchronous: true
    });
  };

  BackendEventObserver.prototype = {
    callbackProcName: '',
    callback: null,
    dispose () {
      pmrpc.unregister(this.callback);
    }
  };

  BackendEventObserver.EventType = {
    HostUnbinded: 1,
    ActionExecuted: 2,
    EntitiesSelected: 3,
    SelectWindowClosed: 4
  };

  BackendEventObserver.HostUnbindingReason = {
    Closed: 'closed',
    Changed: 'changed'
  };

  return {
    BackendEventObserver,
    ExecuteActionOptions,
    ExecuteActionOtions: ExecuteActionOptions,
    ArticleFormState,
    OpenSelectWindowOptions,
    ExternalMessageTypes: BackendExternalMessage.Types,
    BackendEventTypes: BackendEventObserver.EventType,
    executeBackendAction (executeOtions, hostUID, destination) {
      const message = new BackendExternalMessage();
      message.type = BackendExternalMessage.Types.ExecuteAction;
      message.hostUID = hostUID;
      message.data = executeOtions;
      pmrpc.call({
        destination,
        publicProcedureName: message.hostUID,
        params: [message]
      });
    },

    closeBackendHost (actionUID, hostUID, destination) {
      const message = new BackendExternalMessage();
      message.type = BackendExternalMessage.Types.CloseBackendHost;
      message.hostUID = hostUID;
      message.data = { actionUID };
      pmrpc.call({
        destination,
        publicProcedureName: message.hostUID,
        params: [message]
      });
    },

    openSelectWindow (openSelectWindowOptions, hostUID, destination) {
      const message = new BackendExternalMessage();
      message.type = BackendExternalMessage.Types.OpenSelectWindow;
      message.hostUID = hostUID;
      message.data = openSelectWindowOptions;
      pmrpc.call({
        destination,
        publicProcedureName: message.hostUID,
        params: [message]
      });
    },

    checkHost (hostUID, destination, callback) {
      let callbackIsCalled = false;
      const message = new BackendExternalMessage();
      message.type = BackendExternalMessage.Types.CheckHost;
      message.hostUID = hostUID;

      const prmpcObject = {
        destination,
        publicProcedureName: message.hostUID,
        params: [message],
        onSuccess (args) {
          if (callbackIsCalled === false) {
            callbackIsCalled = true;
            callback({ success: true, hostVersion: args.returnValue });
          }
        },
        onError (args) {
          if (callbackIsCalled === false) {
            callbackIsCalled = true;
            callback({ success: false, error: args.description });
          }
        }
      };

      pmrpc.call(prmpcObject);
    }
  };
}());
