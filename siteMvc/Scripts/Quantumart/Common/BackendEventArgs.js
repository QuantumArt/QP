/* eslint camelcase: 0 */
import { $q } from '../Utils';


export class BackendEventArgs {
  // eslint-disable-next-line no-useless-constructor, FIXME
  constructor() {
    // empty constructor
  }

  _entityTypeCode = '';
  _entityTypeName = '';
  _parentEntityId = 0;
  _actionTypeCode = '';
  _actionCode = '';
  _actionName = '';
  _isInterface = false;
  _isCustomAction = false;
  _isMultistepAction = false;
  _isWindow = false;
  _windowWidth = null;
  _windowHeight = null;
  _confirmPhrase = '';
  _previousAction = null;
  _nextSuccessfulActionCode = null;
  _nextFailedActionCode = null;
  _context = null;
  _additionalData = null;

  _entityId = 0;
  _entityName = '';
  _entities = [];
  _isMultipleEntities = false;

  _callerContext = null;
  _externalCallerContext = null;
  _startedByExternal = false;

  get_entityId() {
    return this._entityId;
  }

  set_entityId(value) {
    this._entityId = value;
  }

  get_entityName() {
    return this._entityName;
  }

  set_entityName(value) {
    this._entityName = value;
  }

  get_entities() {
    return this._entities;
  }

  set_entities(value) {
    this._entities = value;
  }

  get_isMultipleEntities() {
    return this._isMultipleEntities;
  }

  set_isMultipleEntities(value) {
    this._isMultipleEntities = value;
  }

  get_entityTypeCode() {
    return this._entityTypeCode;
  }

  set_entityTypeCode(value) {
    this._entityTypeCode = value;
  }

  get_entityTypeName() {
    return this._entityTypeName;
  }

  set_entityTypeName(value) {
    this._entityTypeName = value;
  }

  get_parentEntityId() {
    return this._parentEntityId;
  }

  set_parentEntityId(value) {
    this._parentEntityId = value;
  }

  get_actionCode() {
    return this._actionCode;
  }

  set_actionCode(value) {
    this._actionCode = value;
  }

  get_actionName() {
    return this._actionName;
  }

  set_actionName(value) {
    this._actionName = value;
  }

  get_actionTypeCode() {
    return this._actionTypeCode;
  }

  set_actionTypeCode(value) {
    this._actionTypeCode = value;
  }

  get_isInterface() {
    return this._isInterface;
  }

  set_isInterface(value) {
    this._isInterface = value;
  }

  get_isCustomAction() {
    return this._isCustomAction;
  }

  set_isCustomAction(value) {
    this._isCustomAction = value;
  }

  get_isMultistepAction() {
    return this._isMultistepAction;
  }

  set_isMultistepAction(value) {
    this._isMultistepAction = value;
  }

  get_isWindow() {
    return this._isWindow;
  }

  set_isWindow(value) {
    this._isWindow = value;
  }

  get_windowWidth() {
    return this._windowWidth;
  }

  set_windowWidth(value) {
    this._windowWidth = value;
  }

  get_windowHeight() {
    return this._windowHeight;
  }

  set_windowHeight(value) {
    this._windowHeight = value;
  }

  get_confirmPhrase() {
    return this._confirmPhrase;
  }

  set_confirmPhrase(value) {
    this._confirmPhrase = value;
  }

  get_previousAction() {
    return this._previousAction;
  }

  set_previousAction(value) {
    this._previousAction = value;
  }

  get_nextSuccessfulActionCode() {
    return this._nextSuccessfulActionCode;
  }

  set_nextSuccessfulActionCode(value) {
    this._nextSuccessfulActionCode = value;
  }

  get_nextFailedActionCode() {
    return this._nextFailedActionCode;
  }

  set_nextFailedActionCode(value) {
    this._nextFailedActionCode = value;
  }

  get_context() {
    return this._context;
  }
  set_context(value) {
    this._context = value;
  }

  get_isLoaded() {
    return this._actionTypeCode === window.ACTION_TYPE_CODE_READ
      && this.get_previousActionTypeCode() === window.ACTION_TYPE_CODE_NONE;
  }

  get_isSaved() {
    return this._actionTypeCode === window.ACTION_TYPE_CODE_READ
      && (
        this.get_previousActionTypeCode() === window.ACTION_TYPE_CODE_SAVE
        || this.get_previousActionTypeCode() === window.ACTION_TYPE_CODE_SAVE_AND_UP
      );
  }

  get_isUpdated() {
    return this._actionTypeCode === window.ACTION_TYPE_CODE_READ
      && (
        this.get_previousActionTypeCode() === window.ACTION_TYPE_CODE_UPDATE
        || this.get_previousActionTypeCode() === window.ACTION_TYPE_CODE_UPDATE_AND_UP
      );
  }

  get_isRestored() {
    return this._actionTypeCode === window.ACTION_TYPE_CODE_READ
      && this.get_previousActionTypeCode() === window.ACTION_TYPE_CODE_RESTORE;
  }

  get_needUp() {
    return this.get_previousActionTypeCode() === window.ACTION_TYPE_CODE_SAVE_AND_UP
      || this.get_previousActionTypeCode() === window.ACTION_TYPE_CODE_UPDATE_AND_UP;
  }

  get_previousActionTypeCode() {
    return this._previousAction && this._previousAction.get_isSuccessfullyExecuted()
      ? this._previousAction.get_actionTypeCode() : window.ACTION_TYPE_CODE_NONE;
  }

  get_isArchiving() {
    return this._actionTypeCode === window.ACTION_TYPE_CODE_ARCHIVE
      || this._actionTypeCode === window.ACTION_TYPE_CODE_MULTIPLE_ARCHIVE;
  }

  get_isRemoving() {
    return this._actionTypeCode === window.ACTION_TYPE_CODE_REMOVE
      || this._actionTypeCode === window.ACTION_TYPE_CODE_MULTIPLE_REMOVE;
  }

  get_isRestoring() {
    return this._actionTypeCode === window.ACTION_TYPE_CODE_RESTORE
      || this._actionTypeCode === window.ACTION_TYPE_CODE_MULTIPLE_RESTORE;
  }

  get_callerContext() {
    return this._callerContext;
  }

  set_callerContext(value) {
    this._callerContext = value;
  }

  get_externalCallerContext() {
    return this._externalCallerContext;
  }

  set_externalCallerContext(value) {
    this._externalCallerContext = value;
  }

  get_additionalData() {
    return this._additionalData;
  }

  set_additionalData(value) {
    this._additionalData = value;
  }

  get_startedByExternal() {
    return this._startedByExternal;
  }

  set_startedByExternal(value) {
    this._startedByExternal = value;
  }

  // eslint-disable-next-line max-params
  init(
    entityTypeCode,
    entities,
    parentEntityId,
    action,
    options,
    actionCode
  ) {
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
}


BackendEventArgs.getEventArgsFromOtherEventArgs = function (sourceArgs) {
  if (!$q.isObject(sourceArgs)) {
    throw new Error($l.Common.sourceEventArgsNotSpecified);
  }

  const targetArgs = new BackendEventArgs();
  BackendEventArgs.fillEventArgsFromOtherEventArgs(targetArgs, sourceArgs);

  return targetArgs;
};

BackendEventArgs.fillEventArgsFromOtherEventArgs = function (targetArgs, sourceArgs) {
  if (!$q.isObject(targetArgs)) {
    throw new Error($l.Common.targetEventArgsNotSpecified);
  }

  if (!$q.isObject(sourceArgs)) {
    throw new Error($l.Common.sourceEventArgsNotSpecified);
  }

  if (sourceArgs instanceof BackendEventArgs) {
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


Quantumart.QP8.BackendEventArgs = BackendEventArgs;
