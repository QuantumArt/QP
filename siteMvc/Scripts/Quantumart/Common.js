// ****************************************************************************************
// *** Объявляем пространства имен                            ***
// ****************************************************************************************

if (typeof Quantumart === 'undefined') {
  Type.registerNamespace('Quantumart');
}

if (typeof Quantumart.QP8 === 'undefined') {
  Type.registerNamespace('Quantumart.QP8');
}

if (typeof Quantumart.QP8.Constants === 'undefined') {
  Type.registerNamespace('Quantumart.QP8.Constants');
}

// ********************************************************************************************
// *** Интерфейсы и классы для реализации паттернов "Наблюдатель" и "Посредник"       ***
// ********************************************************************************************

// #region interface IObserver
// === Интерфейс "Наблюдатель" ===
Quantumart.QP8.IObserver = function () { };

Quantumart.QP8.IObserver.prototype = {
  update() { }
};

Quantumart.QP8.IObserver.registerInterface('Quantumart.QP8.IObserver');

// #endregion

// #region interface IObservable
// === Интерфейс "Наблюдаемый" ===
Quantumart.QP8.IObservable = function () { };

Quantumart.QP8.IObservable.prototype = {
  attachObserver() { },
  detachObserver() { },
  notify() { }
};

Quantumart.QP8.IObservable.registerInterface('Quantumart.QP8.IObservable');

// #endregion

// #region class Observable
// === Класс "Наблюдаемый" ===
Quantumart.QP8.Observable = function () {
  this._observerInfos = [];
};

Quantumart.QP8.Observable.prototype = {
  _observerInfos: null,

  _getObserverInfo(eventType, observer) {
    let observerInfo = null;

    const observerInfos = jQuery.grep(this._observerInfos[eventType], observerInfo => {
      if (observerInfo.observer) {
        return observerInfo.observer == observer;
      }
      return false;
    });

    if (observerInfos && observerInfos.length > 0) {
      observerInfo = observerInfos[0];
    }

    return observerInfo;
  },

  _checkObserver(observer) {
    let result = false;
    if ($q.isObject(observer) || $q.isFunction(observer)) {
      result = true;
    } else {
      throw new Error($l.Common.observerIsNotFunctionOrObject);
    }

    if (result) {
      if (!$q.isFunction(observer)) {
        let isObserver = true;

        try {
          isObserver = Object.getType(observer).implementsInterface(Quantumart.QP8.IObserver);
        } catch (e) {
          $q.trace('Exception was catched', e);
        }

        if (isObserver) {
          result = true;
        } else {
          throw new Error($l.Common.observerIsNotImplementedInterface);
        }
      }
    }

    return result;
  },

  attachObserver(eventType, observer, times) {
    if (!this._checkObserver(observer)) {
      return;
    }

    if (times == null) {
      times = -1;
    }

    if (!this._observerInfos[eventType]) {
      this._observerInfos[eventType] = [];
    }

    const observerInfo = this._getObserverInfo(eventType, observer);

    if (!$q.isNull(observerInfo)) {
      observerInfo.times = times;
    } else {
      Array.add(this._observerInfos[eventType], { observer, times });
    }
  },

  detachObserver(eventType, observer) {
    if (!$q.isNull(this._observerInfos)
    && this._observerInfos[eventType]) {
      if (!$q.isNull(observer)) {
        const observerInfo = this._getObserverInfo(eventType, observer);

        if (!$q.isNull(observerInfo)) {
          Array.remove(this._observerInfos[eventType], observerInfo);
        }
      } else {
        $q.removeProperty(this._observerInfos, eventType);
      }
    }
  },

  oneTimeObserver(eventType, observer) {
    this.attachObserver(eventType, observer, 1);
  },

  notify(eventType, eventArgs) {
    if ($q.isNullOrWhiteSpace(eventType)) {
      throw new Error($l.Common.eventTypeNotSpecified);
    }

    if (!$q.isObject(eventArgs)) {
      throw new Error(String.format($l.Common.eventArgsNotSpecified, eventType));
    }

    if ($q.isNull(this._observerInfos)) {
      return;
    }

    const observerInfos = this._observerInfos[eventType];
    let observerInfoCount = 0;

    if (!$q.isNull(observerInfos)) {
      observerInfoCount = observerInfos.length;
    }

    if (observerInfoCount > 0) {
      for (let observerInfoIndex = observerInfoCount - 1; observerInfoIndex >= 0; observerInfoIndex--) {
        const observerInfo = observerInfos[observerInfoIndex];

        if (observerInfo) {
          const observer = observerInfo.observer;

          if (observerInfo.times == -1) {
            this._updateObserver(eventType, eventArgs, observer);
          } else if (observerInfo.times > 0) {
            observerInfo.times--;
            if (observerInfo.times == 0) {
              this.detachObserver(eventType, observer);
            }

            this._updateObserver(eventType, eventArgs, observer);
          }
        }
      }
    }
  },

  _updateObserver(eventType, eventArgs, observer) {
    if ($q.isObject(observer)) {
      let isObserver = false;

      try {
        isObserver = Object.getType(observer).implementsInterface(Quantumart.QP8.IObserver);
      } catch (e) {
        isObserver = true;
      }

      if (isObserver) {
        observer.update(eventType, this, eventArgs);
      }
    } else if ($q.isFunction(observer)) {
      observer(eventType, this, eventArgs);
    }
  },

  dispose() {
    $q.clearArray(this._observerInfos);
  }
};

Quantumart.QP8.Observable.registerClass('Quantumart.QP8.Observable', null, Quantumart.QP8.IObservable, Sys.IDisposable);

// #endregion

// #region interface IMediator
// === Интерфейс "Посредник" ===
Quantumart.QP8.IMediator = function () { };

Quantumart.QP8.IMediator.prototype = {
  introduce() { }
};

Quantumart.QP8.IMediator.registerInterface('Quantumart.QP8.IMediator');

// #endregion

// #region class Mediator
// === Класс "Посредник" ===
Quantumart.QP8.Mediator = function () {
  this._firstComponent = null;
  this._secondComponent = null;
};

Quantumart.QP8.Mediator.prototype = {
  _firstComponent: null,
  _secondComponent: null,

  introduce(firstComponent, secondComponent) {
    if (!$q.isObject(firstComponent)) {
      throw new Error($l.Common.firstComponentInMediatorNotSpecified);
    }

    if (!$q.isObject(secondComponent)) {
      throw new Error($l.Common.secondComponentInMediatorNotSpecified);
    }

    this._firstComponent = firstComponent;
    this._secondComponent = secondComponent;
  },

  update() {
  },

  dispose() {
  }
};

Quantumart.QP8.Mediator.registerClass('Quantumart.QP8.Mediator', null,
  Quantumart.QP8.IMediator, Quantumart.QP8.IObserver, Sys.IDisposable);

// #endregion

// ********************************************************************************************
// *** Классы аргументов событий                              ***
// ********************************************************************************************

// #region class BackendPreviousAction
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
  _entityTypeCode: '',
  _actionTypeCode: '',
  _actionCode: '',
  _isSuccessfullyExecuted: false,

  get_entityTypeCode() {
    return this._entityTypeCode;
  },

  set_entityTypeCode(value) {
    this._entityTypeCode = value;
  },

  get_actionTypeCode() {
    return this._actionTypeCode;
  },

  set_actionTypeCode(value) {
    this._actionTypeCode = value;
  },

  get_actionCode() {
    return this._actionCode;
  },

  set_actionCode(value) {
    this._actionCode = value;
  },

  get_isSuccessfullyExecuted() {
    return this._isSuccessfullyExecuted;
  },

  set_isSuccessfullyExecuted(value) {
    this._isSuccessfullyExecuted = value;
  }
};

Quantumart.QP8.BackendPreviousAction.registerClass('Quantumart.QP8.BackendPreviousAction');

// #endregion

// #region class BackendEventArgs
// === Класс "Аргументы события сущности" ===
Quantumart.QP8.BackendEventArgs = function () {
  Quantumart.QP8.BackendEventArgs.initializeBase(this);
};

Quantumart.QP8.BackendEventArgs.prototype = {
  _entityTypeCode: '',
  _entityTypeName: '',
  _parentEntityId: 0,
  _actionTypeCode: '',
  _actionCode: '',
  _actionName: '',
  _isInterface: false,
  _isCustomAction: false,
  _isMultistepAction: false,
  _isWindow: false,
  _windowWidth: null,
  _windowHeight: null,
  _confirmPhrase: '',
  _previousAction: null,
  _nextSuccessfulActionCode: null,
  _nextFailedActionCode: null,
  _context: null,
  _additionalData: null,

  _entityId: 0,
  _entityName: '',
  _entities: [],
  _isMultipleEntities: false,

  _callerContext: null,
  _externalCallerContext: null,
  _startedByExternal: false,

  get_entityId() {
    return this._entityId;
  },
  set_entityId(value) {
    this._entityId = value;
  },
  get_entityName() {
    return this._entityName;
  },
  set_entityName(value) {
    this._entityName = value;
  },
  get_entities() {
    return this._entities;
  },
  set_entities(value) {
    this._entities = value;
  },
  get_isMultipleEntities() {
    return this._isMultipleEntities;
  },
  set_isMultipleEntities(value) {
    this._isMultipleEntities = value;
  },
  get_entityTypeCode() {
    return this._entityTypeCode;
  },

  set_entityTypeCode(value) {
    this._entityTypeCode = value;
  },

  get_entityTypeName() {
    return this._entityTypeName;
  },

  set_entityTypeName(value) {
    this._entityTypeName = value;
  },

  get_parentEntityId() {
    return this._parentEntityId;
  },

  set_parentEntityId(value) {
    this._parentEntityId = value;
  },

  get_actionCode() {
    return this._actionCode;
  },

  set_actionCode(value) {
    this._actionCode = value;
  },

  get_actionName() {
    return this._actionName;
  },

  set_actionName(value) {
    this._actionName = value;
  },

  get_actionTypeCode() {
    return this._actionTypeCode;
  },

  set_actionTypeCode(value) {
    this._actionTypeCode = value;
  },

  get_isInterface() {
    return this._isInterface;
  },

  set_isInterface(value) {
    this._isInterface = value;
  },

  get_isCustomAction() {
    return this._isCustomAction;
  },

  set_isCustomAction(value) {
    this._isCustomAction = value;
  },

  get_isMultistepAction() {
    return this._isMultistepAction;
  },

  set_isMultistepAction(value) {
    this._isMultistepAction = value;
  },

  get_isWindow() {
    return this._isWindow;
  },

  set_isWindow(value) {
    this._isWindow = value;
  },

  get_windowWidth() {
    return this._windowWidth;
  },

  set_windowWidth(value) {
    this._windowWidth = value;
  },

  get_windowHeight() {
    return this._windowHeight;
  },

  set_windowHeight(value) {
    this._windowHeight = value;
  },

  get_confirmPhrase() {
    return this._confirmPhrase;
  },

  set_confirmPhrase(value) {
    this._confirmPhrase = value;
  },

  get_previousAction() {
    return this._previousAction;
  },

  set_previousAction(value) {
    this._previousAction = value;
  },

  get_nextSuccessfulActionCode() {
    return this._nextSuccessfulActionCode;
  },

  set_nextSuccessfulActionCode(value) {
    this._nextSuccessfulActionCode = value;
  },

  get_nextFailedActionCode() {
    return this._nextFailedActionCode;
  },

  set_nextFailedActionCode(value) {
    this._nextFailedActionCode = value;
  },

  get_context() {
    return this._context;
  },
  set_context(value) {
    this._context = value;
  },

  get_isLoaded() {
    return this._actionTypeCode == ACTION_TYPE_CODE_READ && this.get_previousActionTypeCode() == ACTION_TYPE_CODE_NONE;
  },

  get_isSaved() {
    return this._actionTypeCode == ACTION_TYPE_CODE_READ && (this.get_previousActionTypeCode() == ACTION_TYPE_CODE_SAVE || this.get_previousActionTypeCode() == ACTION_TYPE_CODE_SAVE_AND_UP);
  },

  get_isUpdated() {
    return this._actionTypeCode == ACTION_TYPE_CODE_READ && (this.get_previousActionTypeCode() == ACTION_TYPE_CODE_UPDATE || this.get_previousActionTypeCode() == ACTION_TYPE_CODE_UPDATE_AND_UP);
  },

  get_isRestored() {
    return this._actionTypeCode == ACTION_TYPE_CODE_READ && this.get_previousActionTypeCode() == ACTION_TYPE_CODE_RESTORE;
  },

  get_needUp() {
    return this.get_previousActionTypeCode() == ACTION_TYPE_CODE_SAVE_AND_UP || this.get_previousActionTypeCode() == ACTION_TYPE_CODE_UPDATE_AND_UP;
  },

  get_previousActionTypeCode() {
    return this._previousAction && this._previousAction.get_isSuccessfullyExecuted() ? this._previousAction.get_actionTypeCode() : ACTION_TYPE_CODE_NONE;
  },

  get_isArchiving() {
    return this._actionTypeCode == ACTION_TYPE_CODE_ARCHIVE || this._actionTypeCode == ACTION_TYPE_CODE_MULTIPLE_ARCHIVE;
  },

  get_isRemoving() {
    return this._actionTypeCode == ACTION_TYPE_CODE_REMOVE || this._actionTypeCode == ACTION_TYPE_CODE_MULTIPLE_REMOVE;
  },

  get_isRestoring() {
    return this._actionTypeCode == ACTION_TYPE_CODE_RESTORE || this._actionTypeCode == ACTION_TYPE_CODE_MULTIPLE_RESTORE;
  },

  get_callerContext() {
    return this._callerContext;
  },

  set_callerContext(value) {
    this._callerContext = value;
  },

  get_externalCallerContext() {
    return this._externalCallerContext;
  },

  set_externalCallerContext(value) {
    this._externalCallerContext = value;
  },

  get_additionalData() {
    return this._additionalData;
  },
  set_additionalData(value) {
    this._additionalData = value;
  },

  get_startedByExternal() {
    return this._startedByExternal;
  },
  set_startedByExternal(value) {
    this._startedByExternal = value;
  },

  init(entityTypeCode, entities, parentEntityId, action, options, actionCode) {
    this._entityTypeCode = entityTypeCode;
    this._parentEntityId = parentEntityId;
    if (action.ActionType.IsMultiple) {
      this._entities = entities;
    } else {
      this._entityId = entities[0].Id;
      this._entityName = entities[0].Name;
    }

    if (options) {
      if (options.previousAction) {
        this._previousAction = options.previousAction;
      }
      if (options.forceOpenWindow) {
        this._isWindow = true;
      }
      if (options.context) {
        this._context = options.context;
      }
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

  const targetArgs = new Quantumart.QP8.BackendEventArgs();

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
  } else {
    throw new Error($l.Common.sourceEventArgsIvalidType);
  }
};

Quantumart.QP8.BackendEventArgs.registerClass('Quantumart.QP8.BackendEventArgs');

// #endregion

