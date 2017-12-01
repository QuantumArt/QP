/* eslint max-lines: 'off' */
/* eslint max-statements: 0 */

window.EVENT_TYPE_BACKEND_ACTION_EXECUTED = 'OnActionExecuted';
window.BACKEND_ACTION_EXECUTION_STATUS_NOT_STARTING = 0;
window.BACKEND_ACTION_EXECUTION_STATUS_SUCCESS = 1;
window.BACKEND_ACTION_EXECUTION_STATUS_FAILED = 2;
window.BACKEND_ACTION_EXECUTION_STATUS_ERROR = 3;

Quantumart.QP8.BackendActionExecutor = function () {
  Quantumart.QP8.BackendActionExecutor.initializeBase(this);
};

Quantumart.QP8.BackendActionExecutor.prototype = {
  executeNonInterfaceAction(eventArgs, callback) {
    const actionCode = eventArgs.get_actionCode();
    const isCustom = eventArgs.get_isCustomAction();
    let additionalUrlParameters = null;
    if (eventArgs.get_context() && eventArgs.get_context().additionalUrlParameters) {
      additionalUrlParameters = Object.assign({}, this._additionalUrlParameters,
        eventArgs.get_context().additionalUrlParameters
      );
    }

    if (eventArgs.get_startedByExternal()) {
      additionalUrlParameters = Object.assign({}, additionalUrlParameters, { boundToExternal: true });
    }

    if ($q.isNullOrWhiteSpace(actionCode)) {
      callback('', eventArgs);
    } else {
      const selectedAction = Quantumart.QP8.BackendActionExecutor.getSelectedAction(actionCode);
      if (!$q.isNull(selectedAction) && !selectedAction.IsInterface) {
        const entities = eventArgs.get_entities();
        const isMultiple = selectedAction.ActionType.IsMultiple;
        let confirmPhrase = selectedAction.ConfirmPhrase;

        if (confirmPhrase) {
          let nameString = isMultiple
            ? $o.getEntityNamesFromEntities(entities).join('", "') : eventArgs.get_entityName();
          nameString = `"${nameString}"`;
          confirmPhrase = String.format(confirmPhrase, nameString);
        }

        if (!confirmPhrase || (confirmPhrase && $q.confirmMessage(confirmPhrase))) {
          const entityIDs = isMultiple ? $o.getEntityIDsFromEntities(entities) : [eventArgs.get_entityId()];
          const actionUrl = Quantumart.QP8.BackendActionExecutor.generateActionUrl(
            isMultiple, entityIDs, eventArgs.get_parentEntityId(), '0', actionCode, { additionalUrlParameters }
          );

          if (actionUrl) {
            const postParams = {
              IDs: entityIDs,
              actionCode,
              parentEntityId: eventArgs.get_parentEntityId(),
              entityTypeCode: eventArgs.get_entityTypeCode(),
              level: selectedAction.ActionType.RequiredPermissionLevel
            };

            const normalCallback = function (data) {
              Quantumart.QP8.BackendActionExecutor.showResult(data);
              callback(data && data.Type === window.ACTION_MESSAGE_TYPE_ERROR
                ? window.BACKEND_ACTION_EXECUTION_STATUS_ERROR
                : window.BACKEND_ACTION_EXECUTION_STATUS_SUCCESS, eventArgs
              );
            };

            const errorCallback = function (jqXHR, textStatus) {
              if (textStatus === 'timeout') {
                $q.alertFail($l.Action.actionExecutingTimeoutMessage);
              } else {
                $q.alertFail($l.Action.actionExecutingErrorMessage);
              }

              callback(window.BACKEND_ACTION_EXECUTION_STATUS_FAILED, eventArgs);
            };

            let customData;
            const runAction = function () {
              if (isCustom) {
                $q.getCustomActionJson(customData.Url, postParams, normalCallback, errorCallback);
              } else {
                $q.getJsonFromUrl('POST', actionUrl, postParams, true, false, normalCallback, errorCallback);
              }
            };

            const normalPreActionCallback = function (data) {
              Quantumart.QP8.BackendActionExecutor.showResult(data);
              if (!data
                || (data.Type !== window.ACTION_MESSAGE_TYPE_CONFIRM && data.Type !== window.ACTION_MESSAGE_TYPE_ERROR)
                || (data.Type === window.ACTION_MESSAGE_TYPE_CONFIRM && $q.confirmMessage(data.Text))
              ) {
                runAction();
              } else {
                callback('', eventArgs);
              }
            };

            const preAction = function () {
              if (selectedAction.HasPreAction) {
                if (isCustom) {
                  $q.getCustomActionJson(customData.PreActionUrl, postParams, normalPreActionCallback, errorCallback);
                } else {
                  const preActionUrl = Quantumart.QP8.BackendActionExecutor.generateActionUrl(
                    isMultiple, entityIDs, eventArgs.get_parentEntityId(), '0', actionCode, {
                      isPreAction: true, additionalUrlParameters
                    });
                  $q.getJsonFromUrl(
                    'POST', preActionUrl, postParams, false, false, normalPreActionCallback, errorCallback
                  );
                }
              } else {
                runAction();
              }
            };

            const getCustomUrlCallback = function (data) {
              if (data && data.Type === window.ACTION_MESSAGE_TYPE_ERROR) {
                $a.showResult(data);
                callback('', eventArgs);
              } else {
                customData = data;
                preAction();
              }
            };

            // eslint-disable-next-line max-depth
            if (isCustom) {
              postParams.actionCode = actionCode;
              $q.getJsonFromUrl('POST', actionUrl, postParams, false, false, getCustomUrlCallback, errorCallback);
            } else {
              preAction();
            }
          } else {
            callback('', eventArgs);
          }
        } else {
          callback('', eventArgs);
        }
      }
    }
  },

  executeSpecialAction(eventArgs) {
    let actionStatus = window.BACKEND_ACTION_EXECUTION_STATUS_NOT_STARTING;
    const entityTypeCode = eventArgs.get_entityTypeCode();
    const actionTypeCode = eventArgs.get_actionTypeCode();
    const fileName = eventArgs.get_entityId();
    const urlParams = {
      id: eventArgs.get_parentEntityId(), fileName: encodeURIComponent(fileName), entityTypeCode, actionTypeCode
    };
    if (actionTypeCode === window.ACTION_TYPE_CODE_PREVIEW
      && (entityTypeCode === window.ENTITY_TYPE_CODE_SITE_FILE
        || entityTypeCode === window.ENTITY_TYPE_CODE_CONTENT_FILE
      )) {
      $c.preview(Quantumart.QP8.BackendLibrary.generateActionUrl('GetLibraryImageProperties', urlParams));
      actionStatus = window.BACKEND_ACTION_EXECUTION_STATUS_SUCCESS;
    } else if (actionTypeCode === window.ACTION_TYPE_CODE_DOWNLOAD
      && (entityTypeCode === window.ENTITY_TYPE_CODE_SITE_FILE
        || entityTypeCode === window.ENTITY_TYPE_CODE_CONTENT_FILE
      )) {
      $c.downloadFileWithChecking(
        Quantumart.QP8.BackendLibrary.generateActionUrl('TestLibraryFileDownload', urlParams), fileName
      );
      actionStatus = window.BACKEND_ACTION_EXECUTION_STATUS_SUCCESS;
    }
    if (actionTypeCode === window.ACTION_TYPE_CODE_CROP
      && (entityTypeCode === window.ENTITY_TYPE_CODE_SITE_FILE
        || entityTypeCode === window.ENTITY_TYPE_CODE_CONTENT_FILE
      )) {
      $c.crop(Quantumart.QP8.BackendLibrary.generateActionUrl('GetLibraryImageProperties', urlParams), urlParams);
      actionStatus = window.BACKEND_ACTION_EXECUTION_STATUS_SUCCESS;
    } else if (
      (actionTypeCode === window.ACTION_TYPE_CODE_ALL_FILES_UPLOADED
       || actionTypeCode === window.ACTION_TYPE_CODE_FILE_CROPPED
      ) && (entityTypeCode === window.ENTITY_TYPE_CODE_CONTENT_FILE
        || entityTypeCode === window.ENTITY_TYPE_CODE_SITE_FILE
      )) {
      actionStatus = window.BACKEND_ACTION_EXECUTION_STATUS_SUCCESS;
    }
    return actionStatus;
  },

  executeMultistepAction(eventArgs) {
    const dfr = new $.Deferred();
    const that = this;

    let additionalUrlParameters = null;
    if (eventArgs.get_startedByExternal()) {
      additionalUrlParameters = Object.assign({}, additionalUrlParameters, { boundToExternal: true });
    }

    const actionCode = eventArgs.get_actionCode();
    const selectedAction = Quantumart.QP8.BackendActionExecutor.getSelectedAction(actionCode);

    if (!$q.isNull(selectedAction)) {
      const entities = eventArgs.get_entities();

      const isMultiple = selectedAction.ActionType.IsMultiple;
      let confirmPhrase = selectedAction.ConfirmPhrase;

      if (confirmPhrase) {
        let nameString = isMultiple ? $o.getEntityNamesFromEntities(entities).join('", "') : eventArgs.get_entityName();
        nameString = `"${nameString}"`;
        confirmPhrase = String.format(confirmPhrase, nameString);
      }

      if (!confirmPhrase || (confirmPhrase && $q.confirmMessage(confirmPhrase))) {
        const parentEntityId = eventArgs.get_parentEntityId();
        const entityIDs = isMultiple ? $o.getEntityIDsFromEntities(entities) : [eventArgs.get_entityId()];
        const params = {};
        if (isMultiple) {
          params.IDs = entityIDs;
        }

        const errorCallback = function (jqXHR, textStatus) {
          if (textStatus === 'timeout') {
            $q.alertFail($l.Action.actionExecutingTimeoutMessage);
          } else {
            $q.alertFail($l.Action.actionExecutingErrorMessage);
          }
        };

        const runAction = function (urlParams) {
          const setupUrl = Quantumart.QP8.BackendActionExecutor.generateMultistepActionUrl(
            selectedAction, entityIDs, parentEntityId, {
              additionalUrlParameters,
              isSetup: true,
              urlParams
            });
          if (urlParams) {
            params.settingsParams = urlParams;
          }
          const tearDownUrl = Quantumart.QP8.BackendActionExecutor.generateMultistepActionUrl(
            selectedAction, entityIDs, parentEntityId, {
              additionalUrlParameters,
              isTearDown: true
            });
          const stepUrl = Quantumart.QP8.BackendActionExecutor.generateMultistepActionUrl(
            selectedAction, entityIDs, parentEntityId, {
              additionalUrlParameters
            });

          let toCancel = false;
          let progressWindow = new Quantumart.QP8.BackendMultistepActionWindow(
            selectedAction.Name, selectedAction.ShortName
          );
          const disposeProgressWindow = function () {
            progressWindow.detachObserver(window.EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CANCELING);
            progressWindow.detachObserver(window.EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CANCELED);
            progressWindow.detachObserver(window.EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CLOSED);
            progressWindow.dispose();
            progressWindow = null;
          };

          progressWindow.attachObserver(window.EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CANCELING,
            (eventType, sender, args) => {
              if ($q.confirmMessage($l.MultistepAction.cancelConfirmation)) {
                args.setCancel(true);
              }
            });
          progressWindow.attachObserver(window.EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CANCELED, () => {
            toCancel = true;
          });
          progressWindow.attachObserver(window.EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CLOSED, disposeProgressWindow);
          progressWindow.initialize();

          const errorCallback1 = function (jqXHR, textStatus, errorThrown) {
            errorCallback(jqXHR, textStatus, errorThrown);
            progressWindow.setError();
            dfr.rejectWith(that, [window.BACKEND_ACTION_EXECUTION_STATUS_FAILED]);
          };

          const errorCallback2 = function (jqXHR, textStatus, errorThrown) {
            $q.postDataToUrl(tearDownUrl, { isError: true }, true)
              .done(() => {
                errorCallback1(jqXHR, textStatus, errorThrown);
              })
              .fail(errorCallback1);
          };

          const errorHandler = function (msgResult) {
            Quantumart.QP8.BackendActionExecutor.showResult(msgResult);
            $q.postDataToUrl(tearDownUrl, { isError: true }, true)
              .done(() => {
                progressWindow.setError();
                dfr.rejectWith(that, [window.BACKEND_ACTION_EXECUTION_STATUS_FAILED]);
              })
              .fail(errorCallback1);
          };

          $q.getJsonFromUrl('POST', setupUrl, params, true, false).done(actionData => {
            if (actionData) {
              if (actionData.Type === window.ACTION_MESSAGE_TYPE_ERROR) {
                errorHandler(actionData);
              } else {
                progressWindow.startAction(actionData.Stages.length);

                let stageCounter = 0;
                let stageLength = 0;
                let stepCounter = 0;
                let stepLength = 0;
                let stage = null;

                if (actionData.Stages && actionData.Stages.length) {
                  stageLength = actionData.Stages.length;
                }

                const iterationCallback = function () {
                  if (toCancel) {
                    stageCounter = stageLength;
                  }

                  if (stageCounter < stageLength) {
                    if (stepCounter < stepLength) {
                      $q.getJsonFromUrl('POST', stepUrl,
                        {
                          stage: stageCounter,
                          step: stepCounter
                        }, true, false).done(stepData => {
                        if (stepData) {
                          if (stepData.Type === window.ACTION_MESSAGE_TYPE_ERROR) {
                            errorHandler(stepData);
                          } else {
                            progressWindow.completeStep(
                              stepData.ProcessedItemsCount,
                              stepData.AdditionalInfo,
                              actionData.ParentId || parentEntityId
                            );
                            stepCounter += 1;
                            if (stepCounter === stepLength) {
                              progressWindow.completeStage();
                              stageCounter += 1;
                            }

                            iterationCallback();
                          }
                        }
                      }).fail(errorCallback2);
                    } else {
                      if (stage && stepLength === 0) {
                        progressWindow.completeStage();
                        stageCounter += 1;
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
                  } else {
                    $q.postDataToUrl(tearDownUrl, { isError: false }, true).done(() => {
                      if (toCancel) {
                        progressWindow.setCancel();
                      } else {
                        progressWindow.setComplete();
                      }

                      dfr.resolveWith(that, [window.BACKEND_ACTION_EXECUTION_STATUS_SUCCESS]);
                    }).fail(errorCallback1);
                  }
                };

                iterationCallback();
              }
            }
          }).fail(errorCallback2);
        };

        if (selectedAction.HasSettings && !eventArgs.isSettingsSet) {
          const settingsActionUrl = Quantumart.QP8.BackendActionExecutor.generateMultistepActionUrl(
            selectedAction, entityIDs, parentEntityId, {
              additionalUrlParameters,
              hasSettings: true,
              isSettingsSet: eventArgs.isSettingsSet
            });

          $q.getJsonFromUrl('POST', settingsActionUrl.replace('Settings', 'PreSettings'), params, true, false)
            .done(settingsResult => {
              if (settingsResult && settingsResult.Type === window.ACTION_MESSAGE_TYPE_ERROR) {
                Quantumart.QP8.BackendActionExecutor.showResult(settingsResult);
                dfr.rejectWith(that, [window.BACKEND_ACTION_EXECUTION_STATUS_FAILED]);
                return;
              }
              const args = eventArgs;
              args.settingsActionUrl = settingsActionUrl;
              // eslint-disable-next-line no-new
              new Quantumart.QP8.BackendSettingsPopupWindow(args, settingsResult, runAction);
            }).fail(errorCallback, () => {
              dfr.rejectWith(that, [window.BACKEND_ACTION_EXECUTION_STATUS_FAILED]);
            });
        } else if (selectedAction.HasPreAction) {
          const preActionUrl = Quantumart.QP8.BackendActionExecutor.generateMultistepActionUrl(
            selectedAction, entityIDs, parentEntityId, {
              additionalUrlParameters,
              isPreAction: true
            });

          $q.getJsonFromUrl('POST', preActionUrl, params, true, false).done(preActionResult => {
            if (preActionResult) {
              if (preActionResult.Type === window.ACTION_MESSAGE_TYPE_ERROR) {
                Quantumart.QP8.BackendActionExecutor.showResult(preActionResult);
                dfr.rejectWith(that, [window.BACKEND_ACTION_EXECUTION_STATUS_FAILED]);
                return;
              } else if (preActionResult.Type === window.ACTION_MESSAGE_TYPE_INFO) {
                Quantumart.QP8.BackendActionExecutor.showResult(preActionResult);
                dfr.resolveWith(that);
                return;
              }
            }

            if (!preActionResult || preActionResult.Type !== window.ACTION_MESSAGE_TYPE_CONFIRM
              || $q.confirmMessage(preActionResult.Text)) {
              runAction();
            } else {
              dfr.resolveWith(that);
            }
          }).fail(errorCallback, () => {
            dfr.rejectWith(that, [window.BACKEND_ACTION_EXECUTION_STATUS_FAILED]);
          });
        } else {
          runAction();
        }
      } else {
        dfr.resolveWith(that);
      }
    }

    return dfr.promise();
  },

  dispose() {
    Quantumart.QP8.BackendActionExecutor.callBaseMethod(this, 'dispose');
    $q.collectGarbageInIE();
  }
};

Quantumart.QP8.BackendActionExecutor._instance = null;

Quantumart.QP8.BackendActionExecutor.getInstance = function () {
  if (Quantumart.QP8.BackendActionExecutor._instance === null) {
    Quantumart.QP8.BackendActionExecutor._instance = new Quantumart.QP8.BackendActionExecutor();
  }

  return Quantumart.QP8.BackendActionExecutor._instance;
};

Quantumart.QP8.BackendActionExecutor.destroyInstance = function () {
  if (Quantumart.QP8.BackendActionExecutor._instance) {
    Quantumart.QP8.BackendActionExecutor._instance.dispose();
    Quantumart.QP8.BackendActionExecutor._instance = null;
  }
};

Quantumart.QP8.BackendActionExecutor.getBackendAction = function (action) {
  let newAction = null;
  if ($q.isObject(action)) {
    newAction = action;
  } else if ($q.isString(action)) {
    newAction = Quantumart.QP8.BackendActionExecutor.getBackendActionByCode(action);
  }

  return newAction;
};

Quantumart.QP8.BackendActionExecutor.getBackendActionByCode = function (actionCode) {
  const cacheKey = `ActionByActionCode_${actionCode}`;
  let action = Quantumart.QP8.Cache.getItem(cacheKey);

  if (!action) {
    $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_BACKEND_ACTION}GetByCode`, { actionCode }, false, false)
      .done(data => {
        if (data.success) {
          ({ action } = data);
        } else {
          action = null;
          $q.alertFail(data.Text);
        }
      }).fail(jqXHR => {
        action = null;
        $q.processGenericAjaxError(jqXHR);
      });

    Quantumart.QP8.Cache.addItem(cacheKey, action);
  }

  return action;
};

Quantumart.QP8.BackendActionExecutor.getBackendActionById = function (actionId) {
  const cacheKey = `ActionByActionId_${actionId}`;
  let actionCode = Quantumart.QP8.Cache.getItem(cacheKey);

  if (!actionCode) {
    $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_BACKEND_ACTION}GetCodeById`, { actionId }, false, false)
      .done(data => {
        if (data.success) {
          ({ actionCode } = data);
        } else {
          actionCode = null;
          $q.alertError(data.Text);
        }
      }).fail(jqXHR => {
        actionCode = null;
        $q.processGenericAjaxError(jqXHR);
      });

    Quantumart.QP8.Cache.addItem(cacheKey, actionCode);
  }

  return Quantumart.QP8.BackendActionExecutor.getBackendActionByCode(actionCode);
};

Quantumart.QP8.BackendActionExecutor.getSelectedAction = function (action) {
  let selectedAction = null;
  if ($q.isObject(action)) {
    selectedAction = action;
  } else if ($q.isString(action)) {
    selectedAction = Quantumart.QP8.BackendActionExecutor.getBackendActionByCode(action);
  }

  return selectedAction;
};

Quantumart.QP8.BackendActionExecutor.generateActionUrl = function ( // eslint-disable-line max-params
  isMultiple, entityIDs, parentEntityId, tabId, action, options) {
  const parentEntityIdForUrl = $q.isNullOrEmpty(parentEntityId) ? 0 : parentEntityId;

  let url = '';
  const selectedAction = Quantumart.QP8.BackendActionExecutor.getSelectedAction(action);
  const extraQueryString = options && options.additionalUrlParameters
    ? $q.hashToQueryString(options.additionalUrlParameters) : '';

  if (!$q.isNull(selectedAction)) {
    const isInterface = selectedAction.IsInterface;
    if (options && options.controllerActionUrl) {
      url = options.controllerActionUrl;
    } else if (selectedAction.ControllerActionUrl) {
      url = selectedAction.ControllerActionUrl;
    } else {
      url = isInterface ? '~/Diagnostics/Index/' : '';
    }

    if (options && options.isPreAction) {
      url = url.replace(/\/$/, 'PreAction/');
    }

    if (options && options.hasSettings && !options.isSettingsSet) {
      url = url.replace(/\/$/, '/Settings/');
    }

    url = url.replace(/^~\//, window.APPLICATION_ROOT_URL);
    if (url) {
      if (isMultiple) {
        url += String.format('{0}/{1}/', tabId, parentEntityIdForUrl);
      } else {
        const entityId = entityIDs.length > 0 ? entityIDs[0] : 0;
        url += String.format('{0}/{1}/{2}/', tabId, parentEntityIdForUrl, entityId);
      }

      if (extraQueryString.length > 0) {
        url += `?${extraQueryString}`;
      }
    }
  }

  return url;
};

Quantumart.QP8.BackendActionExecutor.generateBackendActionUrl = function (
  entityId, parentEntityId, tabId, action, options) {
  return Quantumart.QP8.BackendActionExecutor.generateActionUrl(
    false, [entityId], parentEntityId, tabId, action, options
  );
};

Quantumart.QP8.BackendActionExecutor.generateBackendMultipleActionUrl = function (
  entityIDs, parentEntityId, tabId, action, options) {
  return Quantumart.QP8.BackendActionExecutor.generateActionUrl(
    true, entityIDs, parentEntityId, tabId, action, options
  );
};

Quantumart.QP8.BackendActionExecutor.generateMultistepActionUrl = function (
  selectedAction, entityIDs, parentEntityId, options) {
  const parentEntityIdForUrl = $q.isNullOrEmpty(parentEntityId) ? 0 : parentEntityId;
  let url = '';
  const extraQueryString = options && options.additionalUrlParameters
    ? $q.hashToQueryString(options.additionalUrlParameters) : '';

  if (!$q.isNull(selectedAction)) {
    if (selectedAction.IsMultistep || $q.isNullOrEmpty(selectedAction.AdditionalControllerActionUrl)) {
      url = selectedAction.ControllerActionUrl;
    } else {
      url = selectedAction.AdditionalControllerActionUrl;
    }
    if (options) {
      if (options.isPreAction) {
        url = url.replace(/\/$/, '/PreAction/');
      } else if (options.isSetup) {
        url = url.replace(/\/$/, '/Setup/');
      } else if (options.isTearDown) {
        url = url.replace(/\/$/, '/TearDown/');
      } else if (options && options.hasSettings && !options.isSettingsSet) {
        url = url.replace(/\/$/, '/Settings/');
      } else {
        url = url.replace(/\/$/, '/Step/');
      }
    } else {
      url = url.replace(/\/$/, '/Step/');
    }

    url = url.replace(/^~\//, window.APPLICATION_ROOT_URL);

    if (url) {
      if (selectedAction.ActionType.IsMultiple) {
        url += String.format('{0}/{1}/', '0', parentEntityIdForUrl);
      } else {
        const entityId = entityIDs.length > 0 ? entityIDs[0] : 0;
        url += String.format('{0}/{1}/{2}/', '0', parentEntityIdForUrl, entityId);
      }

      if (extraQueryString.length > 0) {
        url += `?${extraQueryString}`;
      }
    }
  }

  return url;
};

Quantumart.QP8.BackendActionExecutor.showResult = function (data) {
  if (data && data.Type) {
    const messageType = data.Type;

    if (messageType === window.ACTION_MESSAGE_TYPE_DOWNLOAD) {
      $c.downloadFile(data.Url);
    } else if (messageType === window.ACTION_MESSAGE_TYPE_INFO
                || messageType === window.ACTION_MESSAGE_TYPE_WARNING
                || messageType === window.ACTION_MESSAGE_TYPE_ERROR) {
      const messageText = data.Text;
      if (!$q.isNullOrWhiteSpace(messageText)) {
        $q.alertFail(messageText);
      }
    }
  }
};

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

  const eventArgs = new Quantumart.QP8.BackendEventArgs();
  Quantumart.QP8.BackendActionExecutor.fillEventArgsFromAction(eventArgs, action);

  return eventArgs;
};

Quantumart.QP8.BackendActionExecutor.getEventArgsFromActionWithParams = function (action, params) {
  if (!$q.isObject(action)) {
    throw new Error($l.Common.actionNotSpecified);
  }
  const eventArgs = new Quantumart.QP8.BackendEventArgs();
  Quantumart.QP8.BackendActionExecutor.fillEventArgsFromAction(eventArgs, action);
  eventArgs.init(
    params.get_entityTypeCode(), params.get_entities(), params.get_parentEntityId(),
    action, params.getOptions(), params.get_actionCode()
  );
  return eventArgs;
};

Quantumart.QP8.BackendActionExecutor.getActionViewByViewTypeCode = function (action, viewTypeCode) {
  const actionViews = action.Views;
  let actionView = null;

  if (!$q.isNullOrEmpty(actionViews)) {
    [actionView] = $.grep(actionViews, view => view.ViewType.Code === viewTypeCode);
  }

  return actionView;
};


Quantumart.QP8.BackendActionExecutor.registerClass('Quantumart.QP8.BackendActionExecutor', Quantumart.QP8.Observable);

window.$a = Quantumart.QP8.BackendActionExecutor;

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
  _entityName: '',
  _parentEntityId: 0,
  _entityTypeCode: '',
  _previousAction: null,
  _entities: null,
  _forceOpenWindow: false,
  _context: null,
  _actionCode: null,
  _isGroup: false,

  get_entityId() { // eslint-disable-line camelcase
    return this._entityId;
  },
  get_entityName() { // eslint-disable-line camelcase
    return this._entityName;
  },
  get_parentEntityId() { // eslint-disable-line camelcase
    return this._parentEntityId;
  },
  get_entityTypeCode() { // eslint-disable-line camelcase
    return this._entityTypeCode;
  },
  get_context() { // eslint-disable-line camelcase
    return this._context;
  },
  get_entities() { // eslint-disable-line camelcase
    return $q.isArray(this._entities) && this._entities.length > 0
      ? this._entities : [{ Id: this._entityId, Name: this._entityName }];
  },
  get_actionCode() { // eslint-disable-line camelcase
    return this._actionCode;
  },

  correct(action) {
    const currentAction = $a.getBackendAction(action);

    if (currentAction.IsWindow) {
      this._forceOpenWindow = true;
    }

    if (action.ActionType.IsMultiple) {
      this._entityId = 0;
      this._entityName = '';
    }

    if (action.Code === window.ACTION_CODE_CONTENT_PERMISSIONS_FOR_CHILD
      || action.Code === window.ACTION_CODE_ARTICLE_PERMISSIONS_FOR_CHILD) {
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
      Object.assign(this._context, { additionalUrlParameters: { fieldId: this._entityId } });
      this._entityId = 0;
    }

    if (this._entityTypeCode !== currentAction.EntityType.Code
      && currentAction.ActionType.Code !== window.ASSEMBLE_PARENT_ACTION_TYPE) {
      this._entityTypeCode = currentAction.EntityType.Code;
      if (this._entityTypeCode !== window.ENTITY_TYPE_CODE_SITE_FILE
        && this._entityTypeCode !== window.ENTITY_TYPE_CODE_CONTENT_FILE) {
        if (this._isGroup) {
          this._context = this._context || {};
          Object.assign(this._context, { additionalUrlParameters: { groupId: this._entityId } });
        } else {
          this._parentEntityId = this._entityId;
        }

        this._entityId = 0;
        this._entityName = '';
      }
    }
  },

  getOptions() {
    return {
      previousAction: this._previousAction,
      forceOpenWindow: this._forceOpenWindow,
      context: this._context
    };
  }
};

Quantumart.QP8.BackendActionParameters.registerClass('Quantumart.QP8.BackendActionParameters');
