// ****************************************************************************
// *** Компонент "Действие"                                 ***
// ****************************************************************************

//#region event types of backend action
// === Типы событий менеджера действий ===
var EVENT_TYPE_BACKEND_ACTION_EXECUTED = "OnActionExecuted";
//#endregion

//#region statuses of backend action execution
// === Типы статусов выполнения действия ===
var BACKEND_ACTION_EXECUTION_STATUS_NOT_STARTING = 0;
var BACKEND_ACTION_EXECUTION_STATUS_SUCCESS = 1;
var BACKEND_ACTION_EXECUTION_STATUS_FAILED = 2;
var BACKEND_ACTION_EXECUTION_STATUS_ERROR = 3;
//#endregion

//#region class BackendActionExecutor
// === Класс "Действие" ===
Quantumart.QP8.BackendActionExecutor = function () {
  Quantumart.QP8.BackendActionExecutor.initializeBase(this);
};

Quantumart.QP8.BackendActionExecutor.prototype = {
  executeNonInterfaceAction: function (eventArgs, callback) {
    var actionCode = eventArgs.get_actionCode();
    var isCustom = eventArgs.get_isCustomAction();

    var additionalUrlParameters = null;
    if (eventArgs.get_context() && eventArgs.get_context().additionalUrlParameters) {
      additionalUrlParameters = jQuery.extend(this._additionalUrlParameters, eventArgs.get_context().additionalUrlParameters);
    }
    if (eventArgs.get_startedByExternal() === true) {
      additionalUrlParameters = jQuery.extend(additionalUrlParameters, { boundToExternal: true });
    }

    if (!$q.isNullOrWhiteSpace(actionCode)) {
      var selectedAction = Quantumart.QP8.BackendActionExecutor.getSelectedAction(actionCode);
      if (!$q.isNull(selectedAction) && !selectedAction.IsInterface) {
        var entities = eventArgs.get_entities();

        var isMultiple = selectedAction.ActionType.IsMultiple;
        var confirmPhrase = selectedAction.ConfirmPhrase;

        if (confirmPhrase) {
            var nameString = (isMultiple) ? $o.getEntityNamesFromEntities(entities).join('", "') : eventArgs.get_entityName();
            nameString = '"' + nameString + '"';
          confirmPhrase = String.format(confirmPhrase, nameString);
        }

        if (!confirmPhrase || (confirmPhrase && confirm(confirmPhrase))) {
          var entityIDs = (isMultiple) ? $o.getEntityIDsFromEntities(entities) : [eventArgs.get_entityId()];
          var actionUrl = Quantumart.QP8.BackendActionExecutor.generateActionUrl(isMultiple, entityIDs, eventArgs.get_parentEntityId(), "0", actionCode, {additionalUrlParameters:  additionalUrlParameters});
          if (actionUrl) {
            var params = {};
            if (isMultiple || isCustom) {
              params.IDs = entityIDs;
            }

            //#region Functions
            var normalPreActionCallback = function (data, textStatus, jqXHR) {
              Quantumart.QP8.BackendActionExecutor.showResult(data);
              if (!data || data.Type != ACTION_MESSAGE_TYPE_CONFIRM && data.Type != ACTION_MESSAGE_TYPE_ERROR || data.Type == ACTION_MESSAGE_TYPE_CONFIRM && confirm(data.Text)) {
                runAction();
              }
              else {
                callback("", eventArgs);
              }
            };

            // выполнить действие
            var runAction = function () {

              if (isCustom) {
                $q.getCustomActionJson(customData.Url, normalCallback, errorCallback);
              }
              else {
                $q.getJsonFromUrl('POST', actionUrl, params, true, false, normalCallback, errorCallback);
              }
            }

            var normalCallback = function (data, textStatus, jqXHR) {
              Quantumart.QP8.BackendActionExecutor.showResult(data)
              callback((data && data.Type == ACTION_MESSAGE_TYPE_ERROR) ? BACKEND_ACTION_EXECUTION_STATUS_ERROR : BACKEND_ACTION_EXECUTION_STATUS_SUCCESS, eventArgs);
            };

            var errorCallback = function (jqXHR, textStatus, errorThrown) {
              if (textStatus === 'timeout') {
                alert($l.Action.actionExecutingTimeoutMessage);
              }
              else {
                alert($l.Action.actionExecutingErrorMessage);
              }
              callback(BACKEND_ACTION_EXECUTION_STATUS_FAILED, eventArgs);
            };

            var preAction = function () {
              if (selectedAction.HasPreAction) {
                if (!isCustom) {
                  var preActionUrl = Quantumart.QP8.BackendActionExecutor.generateActionUrl(isMultiple, entityIDs, eventArgs.get_parentEntityId(), "0", actionCode, { isPreAction: true, additionalUrlParameters: additionalUrlParameters });
                  $q.getJsonFromUrl('POST', preActionUrl, params, false, false, normalPreActionCallback, errorCallback);
                }
                else {
                  $q.getCustomActionJson(customData.PreActionUrl, normalPreActionCallback, errorCallback);
                }
              }
              else {
                runAction();
              }
            };
            //#endregion

            var customData;
            var getCustomUrlCallback = function (data) {
              if (data && data.Type == ACTION_MESSAGE_TYPE_ERROR)
              {
                $a.showResult(data)
                callback("", eventArgs);
              }
              else
              {
                customData = data;
                preAction();
              }
            };

            if (isCustom) {
              params.actionCode = actionCode;
              $q.getJsonFromUrl('POST', actionUrl, params, false, false, getCustomUrlCallback, errorCallback);
            }
            else
              preAction();

          }
          else {
            callback("", eventArgs);
          }
        }
        else {
          callback("", eventArgs);
        }
      }
    }
    else {
      callback("", eventArgs);
    }
  },

  executeSpecialAction: function (eventArgs) {
    var status = BACKEND_ACTION_EXECUTION_STATUS_NOT_STARTING;
    var entityTypeCode = eventArgs.get_entityTypeCode();
    var actionTypeCode = eventArgs.get_actionTypeCode();
    var fileName = eventArgs.get_entityId();
    var urlParams = { id: eventArgs.get_parentEntityId(), fileName: encodeURIComponent(fileName), entityTypeCode: entityTypeCode, actionTypeCode: actionTypeCode };
    if (actionTypeCode == ACTION_TYPE_CODE_PREVIEW && (entityTypeCode == ENTITY_TYPE_CODE_SITE_FILE || entityTypeCode == ENTITY_TYPE_CODE_CONTENT_FILE)) {
      $c.preview(Quantumart.QP8.BackendLibrary.generateActionUrl("GetLibraryImageProperties", urlParams));
      status = BACKEND_ACTION_EXECUTION_STATUS_SUCCESS;
    }
    else if (actionTypeCode == ACTION_TYPE_CODE_DOWNLOAD && (entityTypeCode == ENTITY_TYPE_CODE_SITE_FILE || entityTypeCode == ENTITY_TYPE_CODE_CONTENT_FILE)) {
      $c.downloadFileWithChecking(Quantumart.QP8.BackendLibrary.generateActionUrl("TestLibraryFileDownload", urlParams), fileName);
      status = BACKEND_ACTION_EXECUTION_STATUS_SUCCESS;
    }
    if (actionTypeCode == ACTION_TYPE_CODE_CROP && (entityTypeCode == ENTITY_TYPE_CODE_SITE_FILE || entityTypeCode == ENTITY_TYPE_CODE_CONTENT_FILE)) {
      $c.crop(Quantumart.QP8.BackendLibrary.generateActionUrl("GetLibraryImageProperties", urlParams), urlParams);
      status = BACKEND_ACTION_EXECUTION_STATUS_SUCCESS;
    }
    else if ((actionTypeCode == ACTION_TYPE_CODE_ALL_FILES_UPLOADED || actionTypeCode == ACTION_TYPE_CODE_FILE_CROPPED) && (entityTypeCode == ENTITY_TYPE_CODE_CONTENT_FILE || entityTypeCode == ENTITY_TYPE_CODE_SITE_FILE)) {
        status = BACKEND_ACTION_EXECUTION_STATUS_SUCCESS;
    }
    return status;
  },

  //#region Long Time Operations
  executeMultistepAction: function (eventArgs) {
    var dfr = new jQuery.Deferred(),
      that = this;

    var additionalUrlParameters = null;
    if (eventArgs.get_startedByExternal() === true) {
      additionalUrlParameters = jQuery.extend(additionalUrlParameters, { boundToExternal: true });
    }

    var actionCode = eventArgs.get_actionCode();
    var selectedAction = Quantumart.QP8.BackendActionExecutor.getSelectedAction(actionCode);

    if (!$q.isNull(selectedAction)) {
      var entities = eventArgs.get_entities();

      var isMultiple = selectedAction.ActionType.IsMultiple;
      var confirmPhrase = selectedAction.ConfirmPhrase;

      if (confirmPhrase) {
          var nameString = (isMultiple) ? $o.getEntityNamesFromEntities(entities).join('", "') : eventArgs.get_entityName();
          nameString = '"' + nameString + '"';
        confirmPhrase = String.format(confirmPhrase, nameString);
      }

      if (!confirmPhrase || (confirmPhrase && confirm(confirmPhrase))) {
        var parentEntityId = eventArgs.get_parentEntityId();
        var entityIDs = (isMultiple) ? $o.getEntityIDsFromEntities(entities) : [eventArgs.get_entityId()];
        var params = {};
        if (isMultiple) {
          params.IDs = entityIDs;
        }

        var errorCallback = function (jqXHR, textStatus, errorThrown) {
          if (textStatus === 'timeout') {
            alert($l.Action.actionExecutingTimeoutMessage);
          }
          else {
            alert($l.Action.actionExecutingErrorMessage);
          }
        };

        var runAction = function (urlParams) {
          var setupUrl = Quantumart.QP8.BackendActionExecutor.generateMultistepActionUrl(selectedAction, entityIDs, parentEntityId, {
            additionalUrlParameters:  additionalUrlParameters,
            isSetup: true,
            urlParams: urlParams
          });
          if (urlParams)
              params.settingsParams = urlParams;
          var tearDownUrl = Quantumart.QP8.BackendActionExecutor.generateMultistepActionUrl(selectedAction, entityIDs, parentEntityId, {
            additionalUrlParameters:  additionalUrlParameters,
            isTearDown: true
          });
          var stepUrl = Quantumart.QP8.BackendActionExecutor.generateMultistepActionUrl(selectedAction, entityIDs, parentEntityId, {
            additionalUrlParameters: additionalUrlParameters
          });

          var toCancel = false;
          var progressWindow = new Quantumart.QP8.BackendMultistepActionWindow(selectedAction.Name, selectedAction.ShortName);
          var disposeProgressWindow = function () {
            progressWindow.detachObserver(EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CANCELING);
            progressWindow.detachObserver(EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CANCELED);
            progressWindow.detachObserver(EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CLOSED);
            progressWindow.dispose();
            progressWindow = null;
          };

          progressWindow.attachObserver(EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CANCELING, function (eventType, sender, eventArgs) {
            if (confirm($l.MultistepAction.cancelConfirmation)) {
              eventArgs.setCancel(true);
            }
          });
          progressWindow.attachObserver(EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CANCELED, function () { toCancel = true; });
          progressWindow.attachObserver(EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CLOSED, disposeProgressWindow);
          progressWindow.initialize();

          var errorCallback1 = function (jqXHR, textStatus, errorThrown) {
            errorCallback(jqXHR, textStatus, errorThrown);
            progressWindow.setError();
            dfr.rejectWith(that, [BACKEND_ACTION_EXECUTION_STATUS_FAILED]);
          };

          var errorCallback2 = function (jqXHR, textStatus, errorThrown) {
            $q.postDataToUrl(tearDownUrl, { isError: true }, true)
              .done(function () { errorCallback1(jqXHR, textStatus, errorThrown) })
              .fail(errorCallback1);
          };

          var errorHandler = function (msgResult) {
            Quantumart.QP8.BackendActionExecutor.showResult(msgResult);
              $q.postDataToUrl(tearDownUrl, { isError: true }, true)
              .done(function () {
                progressWindow.setError();
                dfr.rejectWith(that, [BACKEND_ACTION_EXECUTION_STATUS_FAILED]);
              })
              .fail(errorCallback1);
          };

          $q.getJsonFromUrl('POST', setupUrl, params, true, false)
            .done(function (actionData) {
              if (actionData) {
                if (actionData.Type == ACTION_MESSAGE_TYPE_ERROR) {
                  errorHandler(actionData);
                }
                else {
                  progressWindow.startAction(actionData.Stages.length);

                  // Счетчики
                  var stageCounter = 0,
                  stageLength = 0,
                  stepCounter = 0,
                  stepLength = 0;
                  var stage = null;

                  if (actionData.Stages && actionData.Stages.length) {
                    stageLength = actionData.Stages.length;
                  }

                  var iterationCallback = function () {

                    if (toCancel === true) {
                      stageCounter = stageLength;
                    };

                    if (stageCounter < stageLength) {
                      if (stepCounter < stepLength) {
                        $q.getJsonFromUrl('POST', stepUrl,
                        {
                          stage: stageCounter,
                          step: stepCounter
                        }, true, false)
                        .done(function (stepData) {
                          if (stepData) {
                            if (stepData.Type == ACTION_MESSAGE_TYPE_ERROR) {
                              errorHandler(stepData);
                            }
                            else {
                                progressWindow.completeStep(stepData.ProcessedItemsCount, stepData.AdditionalInfo, actionData.ParentId || parentEntityId);
                              stepCounter++;
                              if (stepCounter == stepLength) {
                                progressWindow.completeStage();
                                stageCounter++;
                              }
                              iterationCallback();
                            }
                          }
                        })
                        .fail(errorCallback2);
                      }
                      else {
                        if (stage && stepLength == 0) {
                          progressWindow.completeStage();
                          stageCounter++;
                        }

                        stage = actionData.Stages[stageCounter];
                        stepCounter = 0;
                        stepLength = 0;
                        if (stage) {
                          stepLength = stage.StepCount;
                          progressWindow.startStage(stage.Name, stage.StepCount, stage.ItemCount);
                        }
                        iterationCallback();
                      }
                    }
                    else {
                        $q.postDataToUrl(tearDownUrl, { isError: false }, true)
                      .done(function () {
                        if (toCancel === true) {
                          progressWindow.setCancel();
                        }
                        else {
                          progressWindow.setComplete();
                        }
                        dfr.resolveWith(that, [BACKEND_ACTION_EXECUTION_STATUS_SUCCESS]);
                      })
                      .fail(errorCallback1);
                    }
                  };

                  iterationCallback();
                }
              }
            })
            .fail(errorCallback2);
        }
        if (selectedAction.HasSettings && !eventArgs.isSettingsSet) {
            var settingsActionUrl = Quantumart.QP8.BackendActionExecutor.generateMultistepActionUrl(selectedAction, entityIDs, parentEntityId, {
                additionalUrlParameters: additionalUrlParameters,
                hasSettings: true,
                isSettingsSet: eventArgs.isSettingsSet
            });
            var settingsResult = null;
            $q.getJsonFromUrl('POST', settingsActionUrl.replace('Settings', 'PreSettings'), params, true, false)
            .done(function (settingsResult) {
              if (settingsResult && settingsResult.Type == ACTION_MESSAGE_TYPE_ERROR) {
                Quantumart.QP8.BackendActionExecutor.showResult(settingsResult);
                dfr.rejectWith(that, [BACKEND_ACTION_EXECUTION_STATUS_FAILED]);
                return;
              }
                eventArgs.settingsActionUrl = settingsActionUrl;
                var popup = new Quantumart.QP8.BackendSettingsPopupWindow(eventArgs, settingsResult, runAction);
            }).fail(errorCallback, function () {
                dfr.rejectWith(that, [BACKEND_ACTION_EXECUTION_STATUS_FAILED]);
            });
        }
        else if (selectedAction.HasPreAction) {
          var preActionUrl = Quantumart.QP8.BackendActionExecutor.generateMultistepActionUrl(selectedAction, entityIDs, parentEntityId, {
            additionalUrlParameters:  additionalUrlParameters,
            isPreAction: true
          });
          var preActionResult = null;
          $q.getJsonFromUrl('POST', preActionUrl, params, true, false)
            .done(function (preActionResult) {
              if (preActionResult) {
                if (preActionResult.Type == ACTION_MESSAGE_TYPE_ERROR) {
                  Quantumart.QP8.BackendActionExecutor.showResult(preActionResult);
                  dfr.rejectWith(that, [BACKEND_ACTION_EXECUTION_STATUS_FAILED]);
                  return;
                }
                else if (preActionResult.Type == ACTION_MESSAGE_TYPE_INFO) {
                  Quantumart.QP8.BackendActionExecutor.showResult(preActionResult);
                  dfr.resolveWith(that);
                  return;
                }
              }
              if (!preActionResult || preActionResult.Type != ACTION_MESSAGE_TYPE_CONFIRM || confirm(preActionResult.Text)) {
                runAction();
              }
              else {
                dfr.resolveWith(that);
              }
            })
            .fail(errorCallback, function () {
              dfr.rejectWith(that, [BACKEND_ACTION_EXECUTION_STATUS_FAILED]);
            });
        }
        else {
          runAction();
        }
      }
      else {
        dfr.resolveWith(that);
      }
    }

    return dfr.promise();
  },
  //#endregion

  dispose: function () {
    Quantumart.QP8.BackendActionExecutor.callBaseMethod(this, "dispose");

    Quantumart.QP8.BackendActionExecutor._instance = null;

    $q.collectGarbageInIE();
  }
};

Quantumart.QP8.BackendActionExecutor._instance = null; // экземпляр класса

// Возвращает экземпляр класса "Действие"
Quantumart.QP8.BackendActionExecutor.getInstance = function Quantumart$QP8$BackendActionExecutor$getInstance() {
  if (Quantumart.QP8.BackendActionExecutor._instance == null) {
    Quantumart.QP8.BackendActionExecutor._instance = new Quantumart.QP8.BackendActionExecutor();
  }

  return Quantumart.QP8.BackendActionExecutor._instance;
};

// Уничтожает экземпляр класса "Действие"
Quantumart.QP8.BackendActionExecutor.destroyInstance = function Quantumart$QP8$BackendActionExecutor$destroyInstance() {
  if (Quantumart.QP8.BackendActionExecutor._instance) {
    Quantumart.QP8.BackendActionExecutor._instance.dispose();
  }
};

// Возвращает действие
Quantumart.QP8.BackendActionExecutor.getBackendAction = function (action) {
  var newAction = null;
  if ($q.isObject(action)) {
    newAction = action;
  }
  else if ($q.isString(action)) {
    newAction = Quantumart.QP8.BackendActionExecutor.getBackendActionByCode(action);
  }

  return newAction;
};

// Возвращает действие по его коду
Quantumart.QP8.BackendActionExecutor.getBackendActionByCode = function (actionCode) {
  var cacheKey = "ActionByActionCode_" + actionCode;
  var action = $cache.getItem(cacheKey);

  if (!action) {
    $q.getJsonFromUrl(
      "GET",
      CONTROLLER_URL_BACKEND_ACTION + "GetByCode",
      { "actionCode": actionCode },
      false,
      false)
    .done(function(data){
      if (data.success) {
        action = data.action;
      }
      else {
        action = null;
        alert(data.Text);
      }
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        action = null;
        $q.processGenericAjaxError(jqXHR);
    });

    $cache.addItem(cacheKey, action);
  }

  return action;
};

// Возвращает действие по его id
Quantumart.QP8.BackendActionExecutor.getBackendActionById = function (actionId) {
  var cacheKey = "ActionByActionId_" + actionId;
  var actionCode = $cache.getItem(cacheKey);

  if (!actionCode) {
    $q.getJsonFromUrl(
      "GET",
      CONTROLLER_URL_BACKEND_ACTION + "GetCodeById",
      { "actionId": actionId },
      false,
      false)
    .done(function(data){
      if (data.success) {
        actionCode = data.actionCode;
      }
      else {
        actionCode = null;
        alert(data.Text);
      }
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        actionCode = null;
        $q.processGenericAjaxError(jqXHR);
    });

    $cache.addItem(cacheKey, actionCode);
  }

  return Quantumart.QP8.BackendActionExecutor.getBackendActionByCode(actionCode);
};

Quantumart.QP8.BackendActionExecutor.getSelectedAction = function Quantumart$QP8$BackendActionExecutor$getSelectedAction(action) {

  var selectedAction = null;
  if ($q.isObject(action)) {
    selectedAction = action;
  }
  else if ($q.isString(action)) {
    selectedAction = Quantumart.QP8.BackendActionExecutor.getBackendActionByCode(action);
  }
  return selectedAction;
};

Quantumart.QP8.BackendActionExecutor.generateActionUrl = function (isMultiple, entityIDs, parentEntityId, tabId, action, options) {

  if ($q.isNullOrEmpty(parentEntityId)) {
    parentEntityId = 0;
  }

  var url = "";
  var selectedAction = Quantumart.QP8.BackendActionExecutor.getSelectedAction(action);
  var extraQueryString = (options && options.additionalUrlParameters) ? $q.hashToQueryString(options.additionalUrlParameters) : "";

  if (!$q.isNull(selectedAction)) {
    var isInterface = selectedAction.IsInterface;
    var isCustom = selectedAction.IsCustom;
    if (options && options.controllerActionUrl) {
      url = options.controllerActionUrl;
    }
    else if (selectedAction.ControllerActionUrl) {
      url = selectedAction.ControllerActionUrl;
    }
    else {
      url = (isInterface) ? "~/Diagnostics/Index/" : "";
    }

    if (options && options.isPreAction) {
      url = url.replace(/\/$/, "PreAction/")
    }
    if (options && options.hasSettings && !options.isSettingsSet) {
        url = url.replace(/\/$/, "/Settings/")
    }

    url = url.replace(/^~\//, APPLICATION_ROOT_URL);

    if (url) {
      if (isMultiple) {
        url += String.format("{0}/{1}/", tabId, parentEntityId);
      }
      else {
        var entityId = (entityIDs.length > 0) ? entityIDs[0] : 0;
        url += String.format("{0}/{1}/{2}/", tabId, parentEntityId, entityId);
      }

      if (extraQueryString.length > 0) {
        url += "?" + extraQueryString;
      }
    }
  }

  return url;
};

// Генерирует URL действия
Quantumart.QP8.BackendActionExecutor.generateBackendActionUrl = function (entityId, parentEntityId, tabId, action, options) {
  return Quantumart.QP8.BackendActionExecutor.generateActionUrl(false, [entityId], parentEntityId, tabId, action, options);
};

// Генерирует URL множественного действия
Quantumart.QP8.BackendActionExecutor.generateBackendMultipleActionUrl = function (entityIDs, parentEntityId, tabId, action, options) {
  return Quantumart.QP8.BackendActionExecutor.generateActionUrl(true, entityIDs, parentEntityId, tabId, action, options);
};

Quantumart.QP8.BackendActionExecutor.generateMultistepActionUrl = function (selectedAction, entityIDs, parentEntityId, options) {
  if ($q.isNullOrEmpty(parentEntityId)) {
    parentEntityId = 0;
  }

  var url = "";
  var extraQueryString = (options && options.additionalUrlParameters) ? $q.hashToQueryString(options.additionalUrlParameters) : "";

  if (!$q.isNull(selectedAction)) {
      if (!(selectedAction.IsMultistep || $q.isNullOrEmpty(selectedAction.AdditionalControllerActionUrl))) {
          url = selectedAction.AdditionalControllerActionUrl;
      }
      else {
          url = selectedAction.ControllerActionUrl;
      }
    if (options) {
      if (options.isPreAction) {
        url = url.replace(/\/$/, "/PreAction/");
      }
      else if (options.isSetup) {
              url = url.replace(/\/$/, "/Setup/");
      }
      else if (options.isTearDown) {
        url = url.replace(/\/$/, "/TearDown/");
      }
      else if (options && options.hasSettings && !options.isSettingsSet) {
          url = url.replace(/\/$/, "/Settings/");
      }
      else {
        url = url.replace(/\/$/, "/Step/")
      }
    }
    else {
      url = url.replace(/\/$/, "/Step/")
    }

    url = url.replace(/^~\//, APPLICATION_ROOT_URL);

    if (url) {
            if (selectedAction.ActionType.IsMultiple) {
            url += String.format("{0}/{1}/", "0", parentEntityId);
        }
        else {
            var entityId = (entityIDs.length > 0) ? entityIDs[0] : 0;
            url += String.format("{0}/{1}/{2}/", "0", parentEntityId, entityId);
        }

      if (extraQueryString.length > 0) {
        url += "?" + extraQueryString;
      }
    }
  }

  return url;
};

Quantumart.QP8.BackendActionExecutor.showResult = function (data) {
  if (data && data.Type) {
    var messageType = data.Type;

    if (messageType == ACTION_MESSAGE_TYPE_DOWNLOAD)
    {
      $c.downloadFile(data.Url);
    }
    else if (messageType == ACTION_MESSAGE_TYPE_INFO
                || messageType == ACTION_MESSAGE_TYPE_WARNING
                || messageType == ACTION_MESSAGE_TYPE_ERROR) {

      var messageText = data.Text;
      if (!$q.isNullOrWhiteSpace(messageText)) {
        alert(messageText);
      }
    }
  }
};

// Заполняет агрументы события на основе действия
Quantumart.QP8.BackendActionExecutor.fillEventArgsFromAction = function (eventArgs, action) {
  if (!$q.isObject(eventArgs)) {
    throw new Error($l.Common.targetEventArgsNotSpecified);
  }

  if (!$q.isObject(action)) {
    throw new Error($l.Common.actionNotSpecified);
  }

  eventArgs.set_entityTypeCode(action.EntityType.Code);
  eventArgs.set_entityTypeName(action.EntityType.Name);
  eventArgs.set_actionCode(action.Code);
  eventArgs.set_actionName(action.Name);
  eventArgs.set_actionTypeCode(action.ActionType.Code);
  eventArgs.set_isInterface(action.IsInterface);
  eventArgs.set_isCustomAction(action.IsCustom);
  eventArgs.set_isMultistepAction(action.IsMultistep);
  eventArgs.set_isWindow(action.IsWindow);
  eventArgs.set_windowWidth(action.WindowWidth);
  eventArgs.set_windowHeight(action.WindowHeight);
  eventArgs.set_confirmPhrase(action.ConfirmPhrase);
  eventArgs.set_nextSuccessfulActionCode(action.NextSuccessfulActionCode);
  eventArgs.set_nextFailedActionCode(action.NextFailedActionCode);
  eventArgs.set_isMultipleEntities(action.ActionType.IsMultiple);
};

// Возвращает агрументы события на основе действия
Quantumart.QP8.BackendActionExecutor.getEventArgsFromAction = function (action) {
  if (!$q.isObject(action)) {
    throw new Error($l.Common.actionNotSpecified);
  }

  var eventArgs = new Quantumart.QP8.BackendEventArgs();
  Quantumart.QP8.BackendActionExecutor.fillEventArgsFromAction(eventArgs, action)

  return eventArgs;
};

Quantumart.QP8.BackendActionExecutor.getEventArgsFromActionWithParams = function (action, params) {
  if (!$q.isObject(action)) {
    throw new Error($l.Common.actionNotSpecified);
  }
  var eventArgs = new Quantumart.QP8.BackendEventArgs();
  Quantumart.QP8.BackendActionExecutor.fillEventArgsFromAction(eventArgs, action);
  eventArgs.init(params.get_entityTypeCode(), params.get_entities(), params.get_parentEntityId(), action, params.get_options(), params.get_actionCode());
  return eventArgs;
};

Quantumart.QP8.BackendActionExecutor.getActionViewByViewTypeCode = function (action, viewTypeCode) {
    var actionViews = action.Views;
    var actionView = null;

    if (!$q.isNullOrEmpty(actionViews)) {
        actionView = jQuery.grep(actionViews, function (actionView) { return actionView.ViewType.Code == viewTypeCode; })[0];
    }
    return actionView;
};


Quantumart.QP8.BackendActionExecutor.registerClass("Quantumart.QP8.BackendActionExecutor", Quantumart.QP8.Observable);

window.$a = Quantumart.QP8.BackendActionExecutor;
//#endregion

Quantumart.QP8.BackendActionParameters = function (options) {
  Quantumart.QP8.BackendActionParameters.initializeBase(this);

  if ($q.isObject(options.eventArgs)) {
    if (options.eventArgs.get_isMultipleEntities()) {
      this._entities = options.eventArgs.get_entities();
    } else {
      this._entityId = options.eventArgs.get_entityId();
      this._entityName = options.eventArgs.get_entityName();
    }

    this._parentEntityId = options.eventArgs.get_parentEntityId();
    this._entityTypeCode = options.eventArgs.get_entityTypeCode();
    this._previousAction = options.eventArgs.get_previousAction();
    this._context = options.eventArgs.get_context();
    this._forceOpenWindow = options.eventArgs.get_isWindow();
  } else {
    if (options.entities) {
      this._entities = options.entities;
    } else {
      this._entityId = options.entityId;
      this._entityName = options.entityName;
    }

    if (options.isGroup) {
      this._isGroup = options.isGroup;
    }

    this._parentEntityId = options.parentEntityId;
    this._entityTypeCode = options.entityTypeCode;

    if ($q.isObject(options.previousAction)) {
      this._previousAction = options.previousAction;
    }

    if ($q.isBoolean(options.forceOpenWindow)) {
      this._forceOpenWindow = options.forceOpenWindow;
    }

    if ($q.isObject(options.context)) {
      this._context = options.context;
    }
  }
};

Quantumart.QP8.BackendActionParameters.prototype = {
  _entityId: 0,
  _entityName: "",
  _parentEntityId: 0,
  _entityTypeCode: "",
  _previousAction: null,
  _entities: null,
  _forceOpenWindow: false,
  _context: null,
  _actionCode: null,
  _isGroup: false,

  get_entityId: function () { return this._entityId },
  get_entityName: function () { return this._entityName },
  get_parentEntityId: function () { return this._parentEntityId },
  get_entityTypeCode: function () { return this._entityTypeCode },
  get_context: function () { return this._context },
  get_entities: function () { return ($q.isArray(this._entities) && this._entities.length > 0) ? this._entities : [{ Id: this._entityId, Name: this._entityName}]; },
  get_actionCode: function(){ return this._actionCode;},

  correct: function (action) {
    var currentAction = $a.getBackendAction(action);

    if (currentAction.IsWindow) {
      this._forceOpenWindow = true;
    }

    if (action.ActionType.IsMultiple) {
      this._entityId = 0;
      this._entityName = "";
    }

    if (action.Code === window.ACTION_CODE_CONTENT_PERMISSIONS_FOR_CHILD || action.Code === window.ACTION_CODE_ARTICLE_PERMISSIONS_FOR_CHILD) {
      this._parentEntityId = this._entityId;
      this._entityId = 0;
      if (action.Code === window.ACTION_CODE_CONTENT_PERMISSIONS_FOR_CHILD) {
        this._actionCode = window.ACTION_CODE_CONTENT_PERMISSIONS;
      } else if (action.Code === window.ACTION_CODE_ARTICLE_PERMISSIONS_FOR_CHILD) {
        this._actionCode = window.ACTION_CODE_ARTICLE_PERMISSIONS;
      }
    }

    if (action.Code === window.ACTION_CODE_ADD_NEW_ADJACENT_FIELD) {
      this._context = this._context || {};
      $.extend(this._context, { additionalUrlParameters: { fieldId: this._entityId } });
      this._entityId = 0;
    }

    if (this._entityTypeCode !== currentAction.EntityType.Code && currentAction.ActionType.Code !== window.ASSEMBLE_PARENT_ACTION_TYPE) {
      this._entityTypeCode = currentAction.EntityType.Code;
      if (this._entityTypeCode !== window.ENTITY_TYPE_CODE_SITE_FILE && this._entityTypeCode !== window.ENTITY_TYPE_CODE_CONTENT_FILE) {
        if (!this._isGroup) {
          this._parentEntityId = this._entityId;
        } else {
          this._context = this._context || {};
          $.extend(this._context, { additionalUrlParameters: { groupId: this._entityId } });
        }

        this._entityId = 0;
        this._entityName = "";
      }
    }
  },

  get_options: function () {
    return {
      previousAction: this._previousAction,
      forceOpenWindow: this._forceOpenWindow,
      context: this._context
    };
  }
};

Quantumart.QP8.BackendActionParameters.registerClass("Quantumart.QP8.BackendActionParameters");
