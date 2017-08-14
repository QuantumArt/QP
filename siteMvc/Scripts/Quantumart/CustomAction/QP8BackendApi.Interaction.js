window.Quantumart = window.Quantumart || {};
window.Quantumart.QP8 = window.Quantumart.QP8 || {};
window.Quantumart.QP8.Interaction = window.Quantumart.QP8.Interaction || {};
window.Quantumart.QP8.Interaction = (function Interaction() {
  let BackendEventObserver;

  /* eslint-disable no-empty-function */
  let ArticleFormState = function () {};
  let ExecuteActionOptions = function () {};
  let BackendExternalMessage = function () {};
  let OpenSelectWindowOptions = function () {};

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

  BackendEventObserver = function (callbackProcName, callback) {
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
    dispose: function () {
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
    BackendEventObserver: BackendEventObserver,
    ExecuteActionOptions: ExecuteActionOptions,
    ExecuteActionOtions: ExecuteActionOptions,
    ArticleFormState: ArticleFormState,
    OpenSelectWindowOptions: OpenSelectWindowOptions,
    ExternalMessageTypes: BackendExternalMessage.Types,
    BackendEventTypes: BackendEventObserver.EventType,
    executeBackendAction: function (executeOtions, hostUID, destination) {
      let message = new BackendExternalMessage();
      message.type = BackendExternalMessage.Types.ExecuteAction;
      message.hostUID = hostUID;
      message.data = executeOtions;
      pmrpc.call({
        destination: destination,
        publicProcedureName: message.hostUID,
        params: [message]
      });
    },

    closeBackendHost: function (actionUID, hostUID, destination) {
      let message = new BackendExternalMessage();
      message.type = BackendExternalMessage.Types.CloseBackendHost;
      message.hostUID = hostUID;
      message.data = { actionUID: actionUID };
      pmrpc.call({
        destination: destination,
        publicProcedureName: message.hostUID,
        params: [message]
      });
    },

    openSelectWindow: function (openSelectWindowOptions, hostUID, destination) {
      let message = new BackendExternalMessage();
      message.type = BackendExternalMessage.Types.OpenSelectWindow;
      message.hostUID = hostUID;
      message.data = openSelectWindowOptions;
      pmrpc.call({
        destination: destination,
        publicProcedureName: message.hostUID,
        params: [message]
      });
    },

    checkHost: function (hostUID, destination, callback) {
      let prmpcObject;
      let callbackIsCalled = false;
      let message = new BackendExternalMessage();
      message.type = BackendExternalMessage.Types.CheckHost;
      message.hostUID = hostUID;

      prmpcObject = {
        destination: destination,
        publicProcedureName: message.hostUID,
        params: [message],
        onSuccess: function (args) {
          if (callbackIsCalled === false) {
            callbackIsCalled = true;
            callback({ success: true, hostVersion: args.returnValue });
          }
        },
        onError: function (args) {
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
