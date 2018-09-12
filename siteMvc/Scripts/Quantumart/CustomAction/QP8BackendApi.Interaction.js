/* global module */
/* eslint-disable prefer-arrow-callback, no-empty-function, line-comment-position, object-shorthand */
// prettier-ignore
(function (factory) {
  window.Quantumart = window.Quantumart || {};
  window.Quantumart.QP8 = window.Quantumart.QP8 || {};
  window.Quantumart.QP8.Interaction = window.Quantumart.QP8.Interaction || factory();
  // @ts-ignore
  if (typeof module === 'object' && module.exports) {
    module.exports = window.Quantumart.QP8.Interaction;
  }
}(function () {
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
    CheckHost: 4,
    PreviewImage: 5,
    DownloadFile: 6,
    OpenFileLibrary: 7
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

  // class OpenSelectWindowOtions (Параметры сообщения на открытие окна выбора из списка)
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

  // class DownloadFileOptions (Параметры скачивания файла)
  const DownloadFileOptions = function () { };

  DownloadFileOptions.prototype = {
    entityId: 0,
    fieldId: 0,
    fileName: ''
  };

  // class PreviewImageOptions (Параметры просмотра изображения)
  const PreviewImageOptions = function () { };

  PreviewImageOptions.prototype = {
    entityId: 0,
    fieldId: 0,
    fileName: ''
  };

  // class OpenFileLibraryOptions (Параметры сообщения на открытие окна библиотеки файлов)
  const OpenFileLibraryOptions = function () { };

  OpenFileLibraryOptions.prototype = {
    isImage: false,
    useSiteLibrary: false,
    subFolder: '',
    libraryEntityId: 0,
    libraryParentEntityId: 0,
    selectWindowUID: null, // ID для идентификации окна со списком
    callerCallback: ''
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
    dispose: function () {
      pmrpc.unregister(this.callback);
    }
  };

  BackendEventObserver.EventType = {
    HostUnbinded: 1,
    ActionExecuted: 2,
    EntitiesSelected: 3,
    SelectWindowClosed: 4,
    FileSelected: 5
  };

  BackendEventObserver.HostUnbindingReason = {
    Closed: 'closed',
    Changed: 'changed'
  };

  return {
    BackendEventObserver: BackendEventObserver, // Observer сообщений от хоста
    ExecuteActionOptions: ExecuteActionOptions, // Парамеры сообщения на выполнение BackendAction
    ExecuteActionOtions: ExecuteActionOptions,
    ArticleFormState: ArticleFormState, // Параметры для инициализации формы статьи
    OpenSelectWindowOptions: OpenSelectWindowOptions, // Параметры открытия окна выбора из списка
    ExternalMessageTypes: BackendExternalMessage.Types, // Типы сообщений backend'у
    BackendEventTypes: BackendEventObserver.EventType, // Типы событий backend'а

    // Выполнить BackendAction
    executeBackendAction: function (executeOtions, hostUID, destination) {
      const message = new BackendExternalMessage();
      message.type = BackendExternalMessage.Types.ExecuteAction;
      message.hostUID = hostUID;
      message.data = executeOtions;
      pmrpc.call({
        destination: destination,
        publicProcedureName: message.hostUID,
        params: [message]
      });
    },

    // Закрыть Backend хост
    closeBackendHost: function (actionUID, hostUID, destination) {
      const message = new BackendExternalMessage();
      message.type = BackendExternalMessage.Types.CloseBackendHost;
      message.hostUID = hostUID;
      message.data = { actionUID: actionUID };
      pmrpc.call({
        destination: destination,
        publicProcedureName: message.hostUID,
        params: [message]
      });
    },

    // Открытие всплывающего окна для выбора значения
    openSelectWindow: function (openSelectWindowOptions, hostUID, destination) {
      const message = new BackendExternalMessage();
      message.type = BackendExternalMessage.Types.OpenSelectWindow;
      message.hostUID = hostUID;
      message.data = openSelectWindowOptions;
      pmrpc.call({
        destination: destination,
        publicProcedureName: message.hostUID,
        params: [message]
      });
    },

    // Проверка, что веб-приложение выполняется внутри бекэнда
    checkHost: function (hostUID, destination, callback) {
      let callbackIsCalled = false;
      const message = new BackendExternalMessage();
      message.type = BackendExternalMessage.Types.CheckHost;
      message.hostUID = hostUID;

      pmrpc.call({
        destination: destination,
        publicProcedureName: message.hostUID,
        params: [message],
        onSuccess: function (args) {
          if (!callbackIsCalled) {
            callbackIsCalled = true;
            callback({ success: true, hostVersion: args.returnValue });
          }
        },
        onError: function (args) {
          if (!callbackIsCalled) {
            callbackIsCalled = true;
            callback({ success: false, error: args.description });
          }
        }
      });
    },

    // Посмотреть изображение
    previewImage: function (previewImageOptions, hostUID, destination) {
      const message = new BackendExternalMessage();
      message.type = BackendExternalMessage.Types.PreviewImage;
      message.hostUID = hostUID;
      message.data = previewImageOptions;
      pmrpc.call({
        destination: destination,
        publicProcedureName: message.hostUID,
        params: [message]
      });
    },

    // Скачать файл
    downloadFile: function (downloadFileOptions, hostUID, destination) {
      const message = new BackendExternalMessage();
      message.type = BackendExternalMessage.Types.DownloadFile;
      message.hostUID = hostUID;
      message.data = downloadFileOptions;
      pmrpc.call({
        destination: destination,
        publicProcedureName: message.hostUID,
        params: [message]
      });
    },

    // Открытие всплывающего окна библиотеки файлов
    openFileLibrary: function (openFileLibraryOptions, hostUID, destination) {
      const message = new BackendExternalMessage();
      message.type = BackendExternalMessage.Types.OpenFileLibrary;
      message.hostUID = hostUID;
      message.data = openFileLibraryOptions;
      pmrpc.call({
        destination: destination,
        publicProcedureName: message.hostUID,
        params: [message]
      });
    }
  };
}));
