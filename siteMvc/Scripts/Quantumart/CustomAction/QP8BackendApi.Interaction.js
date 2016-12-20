(function (global) {
	if (jQuery.type(global.Quantumart) == "undefined") {
		global.Quantumart = {};
	}
	if (jQuery.type(global.Quantumart.QP8) == "undefined") {
		global.Quantumart.QP8 = {};
	}
	if (jQuery.type(global.Quantumart.QP8.Interaction) == "undefined") {
		global.Quantumart.QP8.Interaction = (function () {

			//#region class BackendExternalMessage (сообщения для передачи в Backend)
			var BackendExternalMessage = function () {

			};
			BackendExternalMessage.prototype = {
				// Тип
				type: "",
				// UID хоста
				hostUID: null,
				// параметры
				data: null
			};
			BackendExternalMessage.Types = {
				ExecuteAction: 1,
				CloseBackendHost: 2,
				OpenSelectWindow: 3,
				CheckHost: 4
			};
			//#endregion

			//#region class ExecuteActionOptions (Парамеры сообщения на выполнение BackendAction)
			var ExecuteActionOptions = function () { };
			ExecuteActionOptions.prototype = {
				actionCode: "",
				entityTypeCode: "",
				parentEntityId: 0,
				entityId: 0,

				actionUID: null,
				callerCallback: "",
				changeCurrentTab: false,
				isWindow: false,

				options: null
			};
			//#endregion

			//#region class ArticleFormState (Параметры для инициализации формы статьи)
			var ArticleFormState = function () {
			};
			ArticleFormState.prototype = {
				initFieldValues: null, // значения для инициализации полей (массив ArticleFormState.InitFieldValue)
				disabledFields: null, // идентификаторы полей который должны быть disable (массив имен полей)
				hideFields: null, // идентификаторы полей которые должны быть скрыты (массив имен полей)

				disabledActionCodes: null, // массив Action Code для которых кнопки на тулбаре будут скрыты
				additionalParams: null // дополнительные параметры для выполнения Custom Action
			};
			// #region class ArticleFormState.InitFieldValue (значение поля)
			ArticleFormState.InitFieldValue = function () {
			};
			ArticleFormState.InitFieldValue.prototype = {
				fieldName: "", //имя поля
				value: null // значение (зависит от типа)
			};
			//#endregion
			//#endregion

			//#region class OpenSelectWindowOtions (Парамеры сообщения на открытие окна выбора из списка)
			var OpenSelectWindowOptions = function () { };
			OpenSelectWindowOptions.prototype = {
				selectActionCode: "",
				entityTypeCode: "",
				parentEntityId: 0,
				isMultiple: false,
				selectedEntityIDs: null,

				selectWindowUID: null, //ID для идентификации окна со списком
				callerCallback: "",

				options: null
			};
			//#endregion

			//#region class BackendEventObserver (Observer сообщений от хоста)
			var BackendEventObserver = function (callbackProcName, callback) {
				this.callbackProcName = callbackProcName;
				this.callback = callback;

				pmrpc.register({
					publicProcedureName: this.callbackProcName,
					procedure: this.callback,
					isAsynchronous: true
				});
			};
			BackendEventObserver.prototype = {
				callbackProcName: "",
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
				Closed: "closed",
				Changed: "changed"
			}
			//#endregion

			return {
				// Observer сообщений от хоста
				BackendEventObserver: BackendEventObserver,
				// Парамеры сообщения на выполнение BackendAction
				ExecuteActionOptions: ExecuteActionOptions,
				ExecuteActionOtions: ExecuteActionOptions,
				// Параметры для инициализации формы статьи
				ArticleFormState: ArticleFormState,
				// Параметры открытия окна выбора из списка
				OpenSelectWindowOptions: OpenSelectWindowOptions,
				// Типы сообщений backend'у
				ExternalMessageTypes: BackendExternalMessage.Types,
				// Типы событий backend'а
				BackendEventTypes: BackendEventObserver.EventType,

				// Выполнить BackendAction
				executeBackendAction: function (executeOtions, hostUID, destination) {
					var message = new BackendExternalMessage();
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
					var message = new BackendExternalMessage();
					message.type = BackendExternalMessage.Types.CloseBackendHost;
					message.hostUID = hostUID;
					message.data = { "actionUID": actionUID };
					pmrpc.call({
						destination: destination,
						publicProcedureName: message.hostUID,
						params: [message]
					});
				},

				openSelectWindow: function (openSelectWindowOptions, hostUID, destination) {
					var message = new BackendExternalMessage();
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
					var callbackIsCalled = false;
					var message = new BackendExternalMessage();
					message.type = BackendExternalMessage.Types.CheckHost;
					message.hostUID = hostUID;
					pmrpc.call({
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
					});
				}
			};
		})();
	};
})(this);
