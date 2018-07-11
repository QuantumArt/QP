/* eslint-disable no-empty-function, line-comment-position */
window.Quantumart = window.Quantumart || {};
window.Quantumart.QP8 = window.Quantumart.QP8 || {};
window.Quantumart.QP8.Interaction = window.Quantumart.QP8.Interaction || (function Interaction() {
  // class BackendExternalMessage (сообщения для передачи в Backend)
  const BackendExternalMessage = function () { };

  BackendExternalMessage.prototype = {
    type: '', // Тип
    hostUID: null, // UID хоста
    data: null // параметры
  };

  BackendExternalMessage.Types = {
    ExecuteAction: 1,
    CloseBackendHost: 2,
    OpenSelectWindow: 3,
    CheckHost: 4
  };

  // class ExecuteActionOptions (Парамеры сообщения на выполнение BackendAction)
  const ExecuteActionOptions = function () { };

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

  // class ArticleFormState (Параметры для инициализации формы статьи)
  const ArticleFormState = function () { };

  ArticleFormState.prototype = {
    initFieldValues: null, // значения для инициализации полей (массив ArticleFormState.InitFieldValue)
    disabledFields: null, // идентификаторы полей который должны быть disable (массив имен полей)
    hideFields: null, // идентификаторы полей которые должны быть скрыты (массив имен полей)
    disabledActionCodes: null, // массив Action Code для которых кнопки на тулбаре будут скрыты
    additionalParams: null // дополнительные параметры для выполнения Custom Action
  };

  // class ArticleFormState.InitFieldValue (значение поля)
  ArticleFormState.InitFieldValue = function () { };

  ArticleFormState.InitFieldValue.prototype = {
    fieldName: '', // имя поля
    value: null // значение (зависит от типа)
  };

  // class OpenSelectWindowOtions (Парамеры сообщения на открытие окна выбора из списка)
  const OpenSelectWindowOptions = function () { };

  OpenSelectWindowOptions.prototype = {
    selectActionCode: '',
    entityTypeCode: '',
    parentEntityId: 0,
    isMultiple: false,
    selectedEntityIDs: null,
    selectWindowUID: null, // ID для идентификации окна со списком
    callerCallback: '',
    options: null
  };

  // class BackendEventObserver (Observer сообщений от хоста)
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
    dispose() {
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
    BackendEventObserver, // Observer сообщений от хоста
    ExecuteActionOptions, // Парамеры сообщения на выполнение BackendAction
    ExecuteActionOtions: ExecuteActionOptions,
    ArticleFormState, // Параметры для инициализации формы статьи
    OpenSelectWindowOptions, // Параметры открытия окна выбора из списка
    ExternalMessageTypes: BackendExternalMessage.Types, // Типы сообщений backend'у
    BackendEventTypes: BackendEventObserver.EventType, // Типы событий backend'а

    // Выполнить BackendAction
    executeBackendAction(executeOtions, hostUID, destination) {
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

    // Закрыть Backend хост
    closeBackendHost(actionUID, hostUID, destination) {
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

    // Открытие всплывающего окна для выбора значения
    openSelectWindow(openSelectWindowOptions, hostUID, destination) {
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

    // Проверка, что веб-приложение выполняется внутри бекэнда
    checkHost(hostUID, destination, callback) {
      let callbackIsCalled = false;
      const message = new BackendExternalMessage();
      message.type = BackendExternalMessage.Types.CheckHost;
      message.hostUID = hostUID;

      pmrpc.call({
        destination,
        publicProcedureName: message.hostUID,
        params: [message],
        onSuccess(args) {
          if (!callbackIsCalled) {
            callbackIsCalled = true;
            callback({ success: true, hostVersion: args.returnValue });
          }
        },
        onError(args) {
          if (!callbackIsCalled) {
            callbackIsCalled = true;
            callback({ success: false, error: args.description });
          }
        }
      });
    }
  };
}());
