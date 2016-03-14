var EVENT_TYPE_DIRECT_LINK_ACTION_EXECUTING = "OnDirectLinkActionExecuting";

//#region class DirectLinkExecutor
Quantumart.QP8.DirectLinkExecutor = function (currentCustomerCode, directLinkOptions) {
	Quantumart.QP8.DirectLinkExecutor.initializeBase(this);

	this._currentCustomerCode = currentCustomerCode;
	this._urlLinkParams = directLinkOptions;
	this._uid = new Date().getTime();
};

Quantumart.QP8.DirectLinkExecutor.prototype = {
	LOCAL_STORAGE_KEY_INSTANCE_EXISTING_FLAG: "Quantumart.QP8.DirectLinkExecutor.BackendIsAllreadyExists",
	LOCAL_STORAGE_KEY_OBSERVABLE_ITEM: "Quantumart.QP8.DirectLinkExecutor.DirectLinkData",
	LOCAL_STORAGE_KEY_INSTANCE_EXISTING_RESPONSE: "Quantumart.QP8.DirectLinkExecutor.ExistingRespose",
	LOCAL_STORAGE_KEY_SENT_KEY_NAME: "Quantumart.QP8.DirectLinkExecutor.LastKeyName",

	_currentCustomerCode: null,
	_urlLinkParams: null, // параметры прямой ссылки переданные в URL
	_imFirst: false,
	_uid: null,

	// Обработчик запроса на открытие прямой ссылки в первом инстансе (запрос пришел он нового инстанса)
	_onDirectLinkOpenRequested: function (e) {
		if ($q.isNullOrEmpty(e.key)) {
			var key = window.localStorage.getItem(this.LOCAL_STORAGE_KEY_SENT_KEY_NAME);
			if (key) {
				e.key = key;
				e.newValue = window.localStorage.getItem(key);
			}
		}

		if (e.key == this.LOCAL_STORAGE_KEY_OBSERVABLE_ITEM && $q.isString(e.newValue)) {
			var actionParams = jQuery.parseJSON(e.newValue);
			this._executeAction(actionParams, true);
		}
		else if (e.key == this.LOCAL_STORAGE_KEY_INSTANCE_EXISTING_FLAG && $q.isInt(e.newValue)) {			
			// если запрос от другого сервера то пишем в ответ true, сигнализируя что есть работающий инстанс
			if (e.newValue != this._uid) {
				this._send(this.LOCAL_STORAGE_KEY_INSTANCE_EXISTING_RESPONSE, "true");
			}
		}		
	},
	_onDirectLinkOpenRequestedHandler: null,

	// Выполняет действие
	_executeAction: function (actionParams, byRequest) {
		if (actionParams) {
			if ($q.isNullOrEmpty(actionParams.customerCode))
				actionParams.customerCode = this._currentCustomerCode;
			if (actionParams.customerCode.toLowerCase() == this._currentCustomerCode.toLowerCase()) // customer code тот же ?
			{
				// сообщение о запросе на выполнение 
				if (!byRequest || confirm($l.BackendDirectLinkExecutor.OpenDirectLinkConfirmation)) {

					var action = $a.getBackendActionByCode(actionParams.actionCode);
					if (!action) {
						alert($l.Common.ajaxDataReceivingErrorMessage);
					}
					else {
						var actionTypeCode = action.ActionType.Code;
						var params = new Quantumart.QP8.BackendActionParameters({
							entityTypeCode: actionParams.entityTypeCode,
							entityId: actionParams.entityId,
							parentEntityId: actionParams.parentEntityId
						});
						params.correct(action);

						var eventArgs = $a.getEventArgsFromActionWithParams(action, params);
						this.notify(EVENT_TYPE_DIRECT_LINK_ACTION_EXECUTING, eventArgs);

						params = null;
						eventArgs = null;
					}
				}
			}
			else { // customer code изменился
				// сообщение что нужно перелогиниться
				if (confirm($l.BackendDirectLinkExecutor.ReloginRequestConfirmation)) {
					// разлогиниться
					window.location.href = CONTROLLER_URL_LOGON + "LogOut/?" + jQuery.param(actionParams);
				}
			}
			if (byRequest)
				window.localStorage.removeItem(this.LOCAL_STORAGE_KEY_OBSERVABLE_ITEM);
		}
	},

	_send: function (type, message) {
		window.localStorage.removeItem(this.LOCAL_STORAGE_KEY_SENT_KEY_NAME);
		window.localStorage.setItem(type, message);
		if ("onstorage" in document) {
			window.localStorage.setItem(this.LOCAL_STORAGE_KEY_SENT_KEY_NAME, type);			
		}
	},

	// Открыт ли уже другой инстанс?
	_instanceExistenceCheck: function () {
		var dfr = new jQuery.Deferred();
		
		var instanceExists = !$q.isNullOrEmpty(window.localStorage.getItem(this.LOCAL_STORAGE_KEY_INSTANCE_EXISTING_FLAG));
		// если есть флаг, то необходимо послать запрос для проверки существования другого инстанса
		if (instanceExists === true) {			
			this._send(this.LOCAL_STORAGE_KEY_INSTANCE_EXISTING_FLAG, this._uid);
			var that = this;
			setTimeout(function () {
				// проверяем есть ли что либо в ответе 
				var testResponse = $q.toBoolean(window.localStorage.getItem(that.LOCAL_STORAGE_KEY_INSTANCE_EXISTING_RESPONSE), false);
				window.localStorage.removeItem(that.LOCAL_STORAGE_KEY_INSTANCE_EXISTING_RESPONSE);
				// если в ответе true значит другой инстанс существует
				dfr.resolveWith(that, [testResponse]);
			}, 500);
		}
		else {
			dfr.resolveWith(this, [false])
		}
		
		return dfr.promise();
	},

	_registerInstance: function(){
		this._send(this.LOCAL_STORAGE_KEY_INSTANCE_EXISTING_FLAG, "true");		
	},

	_unregisterInstance: function () {
		window.localStorage.removeItem(this.LOCAL_STORAGE_KEY_INSTANCE_EXISTING_FLAG);
	},

	// инициализация
	ready: function (callback) {
		// Это первый инстанс?
		this._instanceExistenceCheck().done(function (instanceExists) {
			if (!instanceExists) {
				this._imFirst = true;
				this._registerInstance();
				this._send(this.LOCAL_STORAGE_KEY_OBSERVABLE_ITEM, "");

				this._onDirectLinkOpenRequestedHandler = jQuery.proxy(this._onDirectLinkOpenRequested, this);
				if (window.addEventListener) {
					window.addEventListener("storage", this._onDirectLinkOpenRequestedHandler, false);
				}
				else if (document.attachEvent) {
					document.attachEvent("onstorage", this._onDirectLinkOpenRequestedHandler);
				}				
				
				var openByDirectLink = false;
				if (this._urlLinkParams)
					openByDirectLink = true;
				// продолжить загрузку страницы
				if ($q.isFunction(callback))
					callback(openByDirectLink);
				// если инстанс был открыт по прямой ссылке, то выполнить действие
				if (openByDirectLink === true)
					this._executeAction(this._urlLinkParams, false);
			}
			else {
				this._imFirst = false;
				// послать запрос на выполнение действие (url) существующему инcтансу
				if (this._urlLinkParams) {
					// сообщение что будет выполнено в первом инстансе
					if (confirm($l.BackendDirectLinkExecutor.WillBeRunInFirstInstanceConfirmation)) {						
						this._send(this.LOCAL_STORAGE_KEY_OBSERVABLE_ITEM, JSON.stringify(this._urlLinkParams));
					}
				}
				else {					
					// сообщаем что инстанс уже открыт
					alert($l.BackendDirectLinkExecutor.InstanceIsAllreadyOpen);
				}
				//TODO: закрыть окно 
			}
		});
	},

	dispose: function () {
		if (this._imFirst) {
			if (!("onstorage" in document)){
				this._unregisterInstance();
			}
			window.localStorage.removeItem(this.LOCAL_STORAGE_KEY_OBSERVABLE_ITEM);
			if (window.removeEventListener) {
				window.removeEventListener("storage", this._onDirectLinkOpenRequestedHandler);
			}
			else if (document.detachEvent) {
				document.detachEvent("onstorage", this._onDirectLinkOpenRequestedHandler);
			}			
		}
	}
};

Quantumart.QP8.DirectLinkExecutor.registerClass("Quantumart.QP8.DirectLinkExecutor", Quantumart.QP8.Observable);
//#endregion