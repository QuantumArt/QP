var EVENT_TYPE_EXTERNAL_ACTION_EXECUTING = "OnExternalActionExecuting";

// Компонент-хост. Обеспечивает взаимодействие с внешними системами (Custom Action)
Quantumart.QP8.BackendCustomActionHost = function (hostId, options, manager) {
	Quantumart.QP8.BackendCustomActionHost.initializeBase(this);

	this._hostId = hostId;
	this._options = options;
	this._manager = manager;
};

Quantumart.QP8.BackendCustomActionHost.prototype = {
	_options: null,
	_hostId: "",	
	_manager: null,

	initialize: function () {
		pmrpc.register({
			publicProcedureName: this._getExecuteActionProcedureName(),
			procedure: jQuery.proxy(this._onExternalMessageReceived, this),
			isAsynchronous: true
		});

		jQuery('#' + this._options.iframeElementId).attr("src", this._generateActionUrl());
	},

	_onExternalMessageReceived: function (message, successCallback, errorCallback) {
		if (message.type == Quantumart.QP8.Interaction.ExternalMessageTypes.ExecuteAction) {
			this._onExecuteActionMessageReceived(message);
			successCallback(0);
		}
		else if (message.type == Quantumart.QP8.Interaction.ExternalMessageTypes.CloseBackendHost) {
			this._onCloseHostMessageReceived(message);
			successCallback(0);
		}
		else if (message.type == Quantumart.QP8.Interaction.ExternalMessageTypes.OpenSelectWindow) {
			this._onOpenSelectWindowMessageReceived(message);
			successCallback(0);
		}
		else if (message.type == Quantumart.QP8.Interaction.ExternalMessageTypes.CheckHost) {
			successCallback(this._onCheckHostMessageReceived(message));
		}
	},

	onSelect: function()
	{
		var id = this._options.iframeElementId;

		jQuery('#' + id).css("marginLeft", "1px");
		setTimeout(function() {
			jQuery('#' + id).css("marginLeft", "0px");
		}, 0);
	},

	_onCheckHostMessageReceived: function (message) {
		return BACKEND_VERSION;
	},

	_onCloseHostMessageReceived: function (message) {
		this._manager.onCloseHostMessageReceived(message);		
	},

	_onExecuteActionMessageReceived: function (message) {
		var action = $a.getBackendActionByCode(message.data.actionCode);
		var params = new Quantumart.QP8.BackendActionParameters({
			entityTypeCode: message.data.entityTypeCode,
			entityId: message.data.entityId,
			entityName: $o.getEntityName(message.data.entityTypeCode, message.data.entityId, message.data.parentEntityId),
			parentEntityId: message.data.parentEntityId
		});
		params.correct(action);

		var eventArgs = $a.getEventArgsFromActionWithParams(action, params);
		eventArgs.set_externalCallerContext(message);
		if (!$q.toBoolean(message.data.changeCurrentTab, false) && !$q.isNull(message.data.isWindow)) {
			eventArgs.set_isWindow(
				$q.toBoolean(
					message.data.isWindow,
					eventArgs.get_isWindow()
				)
			);
		}

		if (message.data.options.currentContext) {
		    message.data.options.contextQuery = JSON.stringify($o.getContextQuery(message.data.parentEntityId, message.data.options.currentContext));
		}
		eventArgs.set_additionalData(message.data.options);
		eventArgs.set_startedByExternal(true);

		params = null;
		action = null;

		this.notify(EVENT_TYPE_EXTERNAL_ACTION_EXECUTING, eventArgs);
		eventArgs = null;
	},

	_onOpenSelectWindowMessageReceived: function(message){
		var eventArgs = new Quantumart.QP8.BackendEventArgs();
		eventArgs.set_isMultipleEntities(message.data.isMultiple);
		eventArgs.set_parentEntityId(message.data.parentEntityId);
		eventArgs.set_entityTypeCode(message.data.entityTypeCode);
		eventArgs.set_actionCode(message.data.selectActionCode);
		

		if ($q.isArray(message.data.selectedEntityIDs) && !$q.isNullOrEmpty(message.data.selectedEntityIDs)) {
			var selectedEntities = jQuery.map(message.data.selectedEntityIDs, function (id) { return { Id: id }; });
			if (message.data.isMultiple) {
				eventArgs.set_entities(selectedEntities);
			}
			else {
				eventArgs.set_entityId(selectedEntities[0].Id);
			}
		}

		selectPopupWindowComponent = new Quantumart.QP8.BackendSelectPopupWindow(eventArgs, message.data.options);
		selectPopupWindowComponent.callerCallback = message.data.callerCallback;
		selectPopupWindowComponent.selectWindowUID = message.data.selectWindowUID;
		selectPopupWindowComponent.attachObserver(EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED, jQuery.proxy(this._popupWindowSelectedHandler, this));
		selectPopupWindowComponent.attachObserver(EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED, jQuery.proxy(this._popupWindowClosedHandler, this));
		selectPopupWindowComponent.openWindow();
		eventArgs = null;
	},

	_popupWindowSelectedHandler: function (eventType, sender, args) {
		this._invokeCallback(Quantumart.QP8.Interaction.BackendEventTypes.EntitiesSelected, {
			selectedEntityIDs: jQuery.map(args.entities, function (ent) { return ent.Id; }),
			callerCallback: sender.callerCallback,
			selectWindowUID: sender.selectWindowUID
		});
		this._destroySelectPopupWindow(sender);
	},

	_popupWindowClosedHandler: function (eventType, sender, args) {
		this._destroySelectPopupWindow(sender);
	},

	_destroySelectPopupWindow: function (popupWindows) {
		if (!$q.isNull(popupWindows)) {
			popupWindows.detachObserver(EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED);
			popupWindows.detachObserver(EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED);
			popupWindows.closeWindow();
			popupWindows.dispose();

			this._invokeCallback(Quantumart.QP8.Interaction.BackendEventTypes.SelectWindowClosed, {
				callerCallback: popupWindows.callerCallback,
				selectWindowUID: popupWindows.selectWindowUID
			});
		};
	},

	get_hostUID: function () {
		return this._options.hostUID;
	},

	onExternalCallerContextsUnbinded: function(message){
		this._invokeCallback(Quantumart.QP8.Interaction.BackendEventTypes.HostUnbinded, message);
	},

	onChildHostActionExecuted: function (message) {
		this._invokeCallback(Quantumart.QP8.Interaction.BackendEventTypes.ActionExecuted, message);
	},

	_invokeCallback: function(type, message){
		var iframe = window.document.getElementById(this._options.iframeElementId);
		if (iframe && iframe.contentWindow) {
			var args = {};
			jQuery.extend(args, message);
			delete args["callerCallback"];
			pmrpc.call({
				destination: iframe.contentWindow,
				publicProcedureName: message.callerCallback,
				params: [type, args]
			});
		}
		iframe = null;
	},

	_getExecuteActionProcedureName: function () {
		return this._options.hostUID;
	},

	_generateActionUrl: function () {
		var result_url = $q.updateQueryStringParameter(this._options.actionBaseUrl, "hostUID", this._options.hostUID);
		if (this._options.additionalParams) {
			result_url += '&' + jQuery.param(this._options.additionalParams);
		}
		return result_url;
	},

	dispose: function () {
		pmrpc.unregister(this._getExecuteActionProcedureName());		
		this._manager.removeComponent(this);
	}
};

Quantumart.QP8.BackendCustomActionHost.registerClass("Quantumart.QP8.BackendCustomActionHost", Quantumart.QP8.Observable);