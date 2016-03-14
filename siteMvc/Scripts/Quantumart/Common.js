// ****************************************************************************************
// *** Объявляем пространства имен														***
// ****************************************************************************************

if (typeof (Quantumart) == 'undefined') {
	Type.registerNamespace("Quantumart");
};
if (typeof (Quantumart.QP8) == 'undefined') {
	Type.registerNamespace("Quantumart.QP8");
};
if (typeof (Quantumart.QP8.Constants) == 'undefined') {
	Type.registerNamespace("Quantumart.QP8.Constants");
};

// ********************************************************************************************
// *** Интерфейсы и классы для реализации паттернов "Наблюдатель" и "Посредник"				***
// ********************************************************************************************

//#region interface IObserver
// === Интерфейс "Наблюдатель" ===
Quantumart.QP8.IObserver = function () { };

Quantumart.QP8.IObserver.prototype = {
	update: function (eventType, sender, eventArgs) { }
};

Quantumart.QP8.IObserver.registerInterface("Quantumart.QP8.IObserver");
//#endregion

//#region interface IObservable
// === Интерфейс "Наблюдаемый" ===
Quantumart.QP8.IObservable = function () { };

Quantumart.QP8.IObservable.prototype = {
	attachObserver: function (eventType, observer) { },
	detachObserver: function (eventType, observer) { },
	notify: function (eventType, eventArgs) { }
};

Quantumart.QP8.IObservable.registerInterface("Quantumart.QP8.IObservable");
//#endregion

//#region class Observable
// === Класс "Наблюдаемый" ===
Quantumart.QP8.Observable = function () {
	this._observerInfos = [];
};

Quantumart.QP8.Observable.prototype = {
	_observerInfos: null,

	_getObserverInfo: function (eventType, observer) {
		var observerInfo = null;
	
		var observerInfos = jQuery.grep(this._observerInfos[eventType],
			function (observerInfo) {
				if (observerInfo.observer) {
					return (observerInfo.observer == observer);
				}
				else {
					return false;
				}
			}
		);
	
		if (observerInfos.length > 0) {
			observerInfo = observerInfos[0];
		}
	
		return observerInfo;
	},

	_checkObserver: function (observer) {
		var result = false;
	
		if ($q.isObject(observer) || $q.isFunction(observer)) {
			result = true;
		}
		else {
			throw new Error($l.Common.observerIsNotFunctionOrObject);
		}
	
		if (result) {
			if (!$q.isFunction(observer)) {
				var isObserver = true;
				try {
					isObserver = Object.getType(observer).implementsInterface(Quantumart.QP8.IObserver);
				}
				catch (e) {
				}
	
				if (isObserver) {
					result = true;
				}
				else {
					throw new Error($l.Common.observerIsNotImplementedInterface);
				}
			}
		}
	
		return result;
	},

	attachObserver: function (eventType, observer, times) {
		if (!this._checkObserver(observer)) {
			return;
		}
	
		if (times == null) {
			times = -1;
		}
	
		if (!this._observerInfos[eventType]) {
			this._observerInfos[eventType] = [];
		}
	
		var observerInfo = this._getObserverInfo(eventType, observer);
		if (!$q.isNull(observerInfo)) {
			observerInfo.times = times;
		}
		else {
			Array.add(this._observerInfos[eventType], { "observer": observer, "times": times });
		}
	},

	detachObserver: function (eventType, observer) {
		if (!$q.isNull(this._observerInfos)
			&& this._observerInfos[eventType]) {
	
			if (!$q.isNull(observer)) {
				var observerInfo = this._getObserverInfo(eventType, observer);
				if (!$q.isNull(observerInfo)) {
					Array.remove(this._observerInfos[eventType], observerInfo);
				}
			}
			else
				$q.removeProperty(this._observerInfos, eventType);
		}
	},

	oneTimeObserver: function (eventType, observer) {
		this.attachObserver(eventType, observer, 1);
	},

	notify: function (eventType, eventArgs) {
		if ($q.isNullOrWhiteSpace(eventType)) {
			throw new Error($l.Common.eventTypeNotSpecified);
		}
	
		if (!$q.isObject(eventArgs)) {
			throw new Error(String.format($l.Common.eventArgsNotSpecified, eventType));
		}
	
		if ($q.isNull(this._observerInfos)) {
			return;
		}
	
		var observerInfos = this._observerInfos[eventType];
		var observerInfoCount = 0;
		if (!$q.isNull(observerInfos)) {
			observerInfoCount = observerInfos.length;
		}
	
		if (observerInfoCount > 0) {
			for (var observerInfoIndex = observerInfoCount - 1; observerInfoIndex >= 0; observerInfoIndex--) {
				var observerInfo = observerInfos[observerInfoIndex];
				if (observerInfo) {
					var observer = observerInfo.observer;
	
					if (observerInfo.times == -1) {
						this._updateObserver(eventType, eventArgs, observer);
					}
					else {
						if (observerInfo.times > 0) {
							observerInfo.times--;
							if (observerInfo.times == 0) {
								this.detachObserver(eventType, observer);
							}
	
							this._updateObserver(eventType, eventArgs, observer);
						}
					}
				}
			}
		}
	},

	_updateObserver: function (eventType, eventArgs, observer) {
		if ($q.isObject(observer)) {
			var isObserver = false;
			try {
				isObserver = Object.getType(observer).implementsInterface(Quantumart.QP8.IObserver);
			}
			catch (e) {
				isObserver = true;
			}
	
			if (isObserver) {
				observer.update(eventType, this, eventArgs);
			}
		}
		else if ($q.isFunction(observer)) {
			observer(eventType, this, eventArgs);
		}
	},

	dispose: function () {
		$q.clearArray(this._observerInfos);
	}
};

Quantumart.QP8.Observable.registerClass("Quantumart.QP8.Observable", null, Quantumart.QP8.IObservable, Sys.IDisposable);
//#endregion

//#region interface IMediator
// === Интерфейс "Посредник" ===
Quantumart.QP8.IMediator = function () { };

Quantumart.QP8.IMediator.prototype = {
	introduce: function (firstComponent, secondComponent) { }
};

Quantumart.QP8.IMediator.registerInterface("Quantumart.QP8.IMediator");
//#endregion

//#region class Mediator
// === Класс "Посредник" ===
Quantumart.QP8.Mediator = function () {
	this._firstComponent = null;
	this._secondComponent = null;
};


Quantumart.QP8.Mediator.prototype = {
	_firstComponent: null, // первый компонент
	_secondComponent: null, // второй компонент

	introduce: function (firstComponent, secondComponent) {
		if (!$q.isObject(firstComponent)) {
			throw new Error($l.Common.firstComponentInMediatorNotSpecified);
			return;
		}
	
		if (!$q.isObject(secondComponent)) {
			throw new Error($l.Common.secondComponentInMediatorNotSpecified);
			return;
		}
	
		this._firstComponent = firstComponent;
		this._secondComponent = secondComponent;
	},

	update: function (eventType, sender, eventArgs) {
	},

	dispose: function () {
	}
};

Quantumart.QP8.Mediator.registerClass("Quantumart.QP8.Mediator", null,
	Quantumart.QP8.IMediator, Quantumart.QP8.IObserver, Sys.IDisposable);
//#endregion

// ********************************************************************************************
// *** Классы аргументов событий															***
// ********************************************************************************************

//#region class BackendPreviousAction
// === Класс "Предыдущее действие" ===
Quantumart.QP8.BackendPreviousAction = function (options) {
	if ($q.isObject(options)) {
		if (options.entityTypeCode) {
			this._entityTypeCode = options.entityTypeCode;
		}
		if (options.actionTypeCode) {
			this._actionTypeCode = options.actionTypeCode;
		}
		if (options.actionCode) {
			this._actionCode = options.actionCode;
		}
		if (!$q.isNull(options.isSuccessfullyExecuted)) {
			this._isSuccessfullyExecuted = options.isSuccessfullyExecuted;
		}
	}
};

Quantumart.QP8.BackendPreviousAction.prototype = {
	_entityTypeCode: "", // код типа сущности
	_actionTypeCode: "", // код типа действия
	_actionCode: "", // код действия
	_isSuccessfullyExecuted: false, // признак успешного выполнения события

	get_entityTypeCode: function () {
		return this._entityTypeCode;
	},

	set_entityTypeCode: function (value) {
		this._entityTypeCode = value;
	},

	get_actionTypeCode: function () {
		return this._actionTypeCode;
	},

	set_actionTypeCode: function (value) {
		this._actionTypeCode = value;
	},

	get_actionCode: function () {
		return this._actionCode;
	},

	set_actionCode: function (value) {
		this._actionCode = value;
	},

	get_isSuccessfullyExecuted: function () {
		return this._isSuccessfullyExecuted;
	},

	set_isSuccessfullyExecuted: function (value) {
		this._isSuccessfullyExecuted = value;
	}
};


Quantumart.QP8.BackendPreviousAction.registerClass("Quantumart.QP8.BackendPreviousAction");
//#endregion

//#region class BackendEventArgs
// === Класс "Аргументы события сущности" ===
Quantumart.QP8.BackendEventArgs = function () {
	Quantumart.QP8.BackendEventArgs.initializeBase(this);
};

Quantumart.QP8.BackendEventArgs.prototype = {
	_entityTypeCode: "", // код типа сущности
	_entityTypeName: "", // название типа сущности
	_parentEntityId: 0, // идентификатор родительской сущности
	_actionTypeCode: "", // код типа действия
	_actionCode: "", // код действия
	_actionName: "", // название действия
	_isInterface: false, // признак того, что для выполнения действия нужен интерфейс
	_isCustomAction: false, // является ли действие пользовательским
	_isMultistepAction: false, // является ли действие многошаговым
	_isWindow: false, // признак того, что для выполнения действия требуется всплывающее окно
	_windowWidth: null, // ширина окна
	_windowHeight: null, // высота окна
	_confirmPhrase: "", // текст подтверждения, которое запрашивается у пользователя перед выполнением операции
	_previousAction: null, // информация о предыдущем событии
	_nextSuccessfulActionCode: null, // код действия, которое должно быть произведено после успешного выполнения текущего действия
	_nextFailedActionCode: null, // код действия, которое должно быть произведено после неуспешного выполнения текущего действия
	_context: null, // контекст события (если необходимо)	
	_additionalData: null, // дополнительные данные (например, предустановленные значения полей)

	_entityId: 0, // идентификатор сущности
	_entityName: "", // название сущности
	_entities: [],
	_isMultipleEntities: false,    

	_callerContext: null,
	_externalCallerContext: null,
	_startedByExternal: false,

	get_entityId: function () { return this._entityId; },
	set_entityId: function (value) { this._entityId = value; },
	get_entityName: function () { return this._entityName; },
	set_entityName: function (value) { this._entityName = value; },
	get_entities: function () { return this._entities; },
	set_entities: function (value) { this._entities = value; },
	get_isMultipleEntities: function () { return this._isMultipleEntities; },
	set_isMultipleEntities: function (value) { this._isMultipleEntities = value; },
	get_entityTypeCode: function () {
		return this._entityTypeCode;
	},

	set_entityTypeCode: function (value) {
		this._entityTypeCode = value;
	},

	get_entityTypeName: function () {
		return this._entityTypeName;
	},

	set_entityTypeName: function (value) {
		this._entityTypeName = value;
	},

	get_parentEntityId: function () {
		return this._parentEntityId;
	},

	set_parentEntityId: function (value) {
		this._parentEntityId = value;
	},

	get_actionCode: function () {
		return this._actionCode;
	},

	set_actionCode: function (value) {
		this._actionCode = value;
	},

	get_actionName: function () {
		return this._actionName;
	},

	set_actionName: function (value) {
		this._actionName = value;
	},

	get_actionTypeCode: function () {
		return this._actionTypeCode;
	},

	set_actionTypeCode: function (value) {
		this._actionTypeCode = value;
	},

	get_isInterface: function () {
		return this._isInterface;
	},

	set_isInterface: function (value) {
		this._isInterface = value;
	},

	get_isCustomAction: function () {
		return this._isCustomAction;
	},

	set_isCustomAction: function (value) {
		this._isCustomAction = value;
	},

	get_isMultistepAction: function () {
		return this._isMultistepAction;
	},

	set_isMultistepAction: function (value) {
		this._isMultistepAction = value;
	},

	get_isWindow: function () {
		return this._isWindow;
	},

	set_isWindow: function (value) {
		this._isWindow = value;
	},

	get_windowWidth: function () {
		return this._windowWidth;
	},

	set_windowWidth: function (value) {
		this._windowWidth = value;
	},

	get_windowHeight: function () {
		return this._windowHeight;
	},

	set_windowHeight: function (value) {
		this._windowHeight = value;
	},

	get_confirmPhrase: function () {
		return this._confirmPhrase;
	},

	set_confirmPhrase: function (value) {
		this._confirmPhrase = value;
	},

	get_previousAction: function () {
		return this._previousAction;
	},

	set_previousAction: function (value) {
		this._previousAction = value;
	},

	get_nextSuccessfulActionCode: function () {
		return this._nextSuccessfulActionCode;
	},

	set_nextSuccessfulActionCode: function (value) {
		this._nextSuccessfulActionCode = value;
	},

	get_nextFailedActionCode: function () {
		return this._nextFailedActionCode;
	},

	set_nextFailedActionCode: function (value) {
		this._nextFailedActionCode = value;
	},

	get_context: function () { return this._context; },
	set_context: function (value) { this._context = value; },

	get_isLoaded: function () {
		return this._actionTypeCode == ACTION_TYPE_CODE_READ && this.get_previousActionTypeCode() == ACTION_TYPE_CODE_NONE;
	},

	get_isSaved: function () {
		return this._actionTypeCode == ACTION_TYPE_CODE_READ && (this.get_previousActionTypeCode() == ACTION_TYPE_CODE_SAVE || this.get_previousActionTypeCode() == ACTION_TYPE_CODE_SAVE_AND_UP);
	},

	get_isUpdated: function () {
		return this._actionTypeCode == ACTION_TYPE_CODE_READ && (this.get_previousActionTypeCode() == ACTION_TYPE_CODE_UPDATE || this.get_previousActionTypeCode() == ACTION_TYPE_CODE_UPDATE_AND_UP);
	},

	get_isRestored: function () {
	    return this._actionTypeCode == ACTION_TYPE_CODE_READ && this.get_previousActionTypeCode() == ACTION_TYPE_CODE_RESTORE;
	},

	get_needUp: function () {
		return this.get_previousActionTypeCode() == ACTION_TYPE_CODE_SAVE_AND_UP || this.get_previousActionTypeCode() == ACTION_TYPE_CODE_UPDATE_AND_UP;
	},

	get_previousActionTypeCode: function () {
		return (this._previousAction && this._previousAction.get_isSuccessfullyExecuted()) ? this._previousAction.get_actionTypeCode() : ACTION_TYPE_CODE_NONE;
	},

	get_isArchiving: function () {
		return this._actionTypeCode == ACTION_TYPE_CODE_ARCHIVE || this._actionTypeCode == ACTION_TYPE_CODE_MULTIPLE_ARCHIVE;
	},

	get_isRemoving: function () {
		return this._actionTypeCode == ACTION_TYPE_CODE_REMOVE || this._actionTypeCode == ACTION_TYPE_CODE_MULTIPLE_REMOVE;
	},

	get_isRestoring: function () {
		return this._actionTypeCode == ACTION_TYPE_CODE_RESTORE || this._actionTypeCode == ACTION_TYPE_CODE_MULTIPLE_RESTORE;
	},

	get_callerContext: function () {
		return this._callerContext;
	},

	set_callerContext: function (value) {
		this._callerContext = value;
	},

	get_externalCallerContext: function () {
	    return this._externalCallerContext;
	},

	set_externalCallerContext: function (value) {
	    this._externalCallerContext = value;
	},

	get_additionalData: function () {
		return this._additionalData;
	},
	set_additionalData: function (value) {
		this._additionalData = value;
	},

	get_startedByExternal: function(){
		return this._startedByExternal;
	},
	set_startedByExternal: function(value){
		this._startedByExternal = value;
	},

	init: function (entityTypeCode, entities, parentEntityId, action, options, actionCode) {
		this._entityTypeCode = entityTypeCode;
		this._parentEntityId = parentEntityId;
		if (action.ActionType.IsMultiple) {
			this._entities = entities;
		}
		else {
			this._entityId = entities[0].Id;
			this._entityName = entities[0].Name;
		}
		if (options) {
			if (options.previousAction)
				this._previousAction = options.previousAction;
			if (options.forceOpenWindow)
			    this._isWindow = true;
			if (options.context)
			    this._context = options.context;
		}
		if ($q.isString(actionCode)) {
			this.set_actionCode(actionCode);
		}
	}
};


// Возвращает агрументы события на основе агрументов другого события
Quantumart.QP8.BackendEventArgs.getEventArgsFromOtherEventArgs = function (sourceArgs) {
	if (!$q.isObject(sourceArgs)) {
		throw new Error($l.Common.sourceEventArgsNotSpecified);
	}

	var targetArgs = new Quantumart.QP8.BackendEventArgs();
	Quantumart.QP8.BackendEventArgs.fillEventArgsFromOtherEventArgs(targetArgs, sourceArgs);

	return targetArgs;
};

// Заполняет агрументы события на основе агрументов другого события
Quantumart.QP8.BackendEventArgs.fillEventArgsFromOtherEventArgs = function (targetArgs, sourceArgs) {
	if (!$q.isObject(targetArgs)) {
		throw new Error($l.Common.targetEventArgsNotSpecified);
	}

	if (!$q.isObject(sourceArgs)) {
		throw new Error($l.Common.sourceEventArgsNotSpecified);
	}

	if (Quantumart.QP8.BackendEventArgs.isInstanceOfType(sourceArgs)
		|| Object.getType(sourceArgs).inheritsFrom(Quantumart.QP8.BackendEventArgs)) {

		targetArgs.set_entityTypeCode(sourceArgs.get_entityTypeCode());
		targetArgs.set_entityTypeName(sourceArgs.get_entityTypeName());
		targetArgs.set_parentEntityId(sourceArgs.get_parentEntityId());
		targetArgs.set_actionCode(sourceArgs.get_actionCode());
		targetArgs.set_actionName(sourceArgs.get_actionName());
		targetArgs.set_actionTypeCode(sourceArgs.get_actionTypeCode());
		targetArgs.set_isInterface(sourceArgs.get_isInterface());
		targetArgs.set_isWindow(sourceArgs.get_isWindow());
		targetArgs.set_windowWidth(sourceArgs.get_windowWidth());
		targetArgs.set_windowHeight(sourceArgs.get_windowHeight());
		targetArgs.set_confirmPhrase(sourceArgs.get_confirmPhrase());
		targetArgs.set_previousAction(sourceArgs.get_previousAction());
		targetArgs.set_nextSuccessfulActionCode(sourceArgs.get_nextSuccessfulActionCode());
		targetArgs.set_nextFailedActionCode(sourceArgs.get_nextFailedActionCode());
		targetArgs.set_context(sourceArgs.get_context());
		targetArgs.set_entityId(sourceArgs.get_entityId());
		targetArgs.set_entityName(sourceArgs.get_entityName());
		targetArgs.set_entities(sourceArgs.get_entities());
		targetArgs.set_isMultipleEntities(sourceArgs.get_isMultipleEntities());
	}
	else {
		throw new Error($l.Common.sourceEventArgsIvalidType);
	}
};

Quantumart.QP8.BackendEventArgs.registerClass("Quantumart.QP8.BackendEventArgs");
//#endregion

