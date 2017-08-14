window.EVENT_TYPE_AUTO_SAVER_ACTION_EXECUTING = 'OnAutoSaverRestoringActionExecuting';

Quantumart.QP8.EntityEditorAutoSaver = function (options) {
  Quantumart.QP8.EntityEditorAutoSaver.initializeBase(this);

  if (options) {
    if (options.currentCustomerCode) {
      this._currentCustomerCode = options.currentCustomerCode;
    }

    if (options.currentUserId) {
      this._currentUserId = options.currentUserId;
    }
  }

  this.isRun = false;
  this.restoring = false;
  this._keyPrefix = `${Quantumart.QP8.EntityEditorAutoSaver._keyNameRoot}.${this._currentCustomerCode}.${this._currentUserId}`;
};

Quantumart.QP8.EntityEditorAutoSaver._keyNameRoot = 'Quantumart.QP8.EntityEditorAutoSaver';
Quantumart.QP8.EntityEditorAutoSaver.prototype = {
  _restoringStateCount: 0,
  isRun: false,
  restoring: false,
  _currentCustomerCode: '',
  _currentUserId: '',
  _keyPrefix: '',

  start: function () {
    this.isRun = true;
    this.restoring = false;
  },

  restore: function () {
    let keyRegExp, autoSaverKeys, savedStates, key, i;

    this.isRun = false;
    this.restoring = true;

    keyRegExp = new RegExp(`^${this._keyPrefix}`);
    autoSaverKeys = [];
    savedStates = [];

    for (key in localStorage) {
      if (keyRegExp.test(key)) {
        autoSaverKeys.push(key);
        savedStates.push(jQuery.parseJSON(localStorage.getItem(key)));
      }
    }

    for (i = 0; i < autoSaverKeys.length; i++) {
      localStorage.removeItem(autoSaverKeys[i]);
    }

    this._checkForRestoring(savedStates).done(jQuery.proxy(function (approvedStates) {
      let eventArgs, editorState, action, params;
      if (approvedStates.length > 0 && $q.confirmMessage($l.EntityEditorAutoSaver.restoreConfirmationRequest)) {
        this._restoringStateCount = approvedStates.length;
        for (let i = 0; i < approvedStates.length; i++) {
          editorState = approvedStates[i];
          action = $a.getBackendActionByCode(editorState.actionCode);
          params = new Quantumart.QP8.BackendActionParameters({
            entityTypeCode: editorState.entityTypeCode,
            entityId: editorState.entityId,
            parentEntityId: editorState.parentEntityId
          });

          params.correct(action);
          eventArgs = $a.getEventArgsFromActionWithParams(action, params);
          eventArgs.set_additionalData({
            restoring: true,
            initFieldValues: editorState.fieldValues,
            disabledFields: editorState.disabledFields,
            hideFields: editorState.hideFields
          });

          params = null;
          action = null;
          eventArgs._isWindow = false;
          this.notify(window.EVENT_TYPE_AUTO_SAVER_ACTION_EXECUTING, eventArgs);
          eventArgs = null;
        }
      } else {
        this._restoringStateCount = 0;
        this.start();
      }
    }, this)).fail(jQuery.proxy(function () {
      this._restoringStateCount = 0;
      this.start();
    }, this));
  },

  onEntityEditorReady: function (documentWrapperElementId) {
    let editor = Quantumart.QP8.BackendEntityEditor.getComponent($(`#${documentWrapperElementId}`));

    if (editor && editor.allowAutoSave() && (this.restoring === true || editor.isFieldsValid() !== true)) {
      localStorage.setItem(
        this._createKey(documentWrapperElementId),
        JSON.stringify(this._getEditorComponentState(documentWrapperElementId))
      );

      if (this.restoring) {
        this._restoringStateCount -= 1;
        if (this._restoringStateCount < 1) {
          this.start();
        }
      }
    }
  },

  onEntityEditorDisposed: function (documentWrapperElementId) {
    if (this.isRun === true) {
      localStorage.removeItem(this._createKey(documentWrapperElementId));
    }
  },

  onFieldChanged: function (fieldChangeInfo) {
    if (this.isRun === true) {
      let editor = Quantumart.QP8.BackendEntityEditor.getComponent($(`#${fieldChangeInfo.documentWrapperElementId}`));

      if (editor && editor.allowAutoSave()) {
        let key = this._createKey(fieldChangeInfo.documentWrapperElementId);
        let editorState = jQuery.parseJSON(localStorage.getItem(key));

        if (!editorState) {
          editorState = this._getEditorComponentState(fieldChangeInfo.documentWrapperElementId);
        } else {
          let fieldState = jQuery.grep(editorState.fieldValues, (v) => {
            return v.fieldName === fieldChangeInfo.fieldName;
          })[0];

          if (fieldState) {
            fieldState.value = fieldChangeInfo.value;
          } else {
            editorState.fieldValues.push({
              fieldName: fieldChangeInfo.fieldName,
              value: fieldChangeInfo.value
            });
          }
        }

        editorState.contextQuery = editor.get_contextQuery();
        localStorage.setItem(key, JSON.stringify(editorState));
      }
    }
  },

  // Обработчик события изменения состояния редактора, при котором необходимо полностью перечитать состояние
  onAllFieldInvalidate: function (documentWrapperElementId) {
    if (this.isRun === true) {
      let editor = Quantumart.QP8.BackendEntityEditor.getComponent($(`#${documentWrapperElementId}`));

      if (editor && editor.allowAutoSave()) {
        let key = this._createKey(documentWrapperElementId);
        let editorState = jQuery.parseJSON(localStorage.getItem(key));

        if (editorState) {
          localStorage.setItem(key, JSON.stringify(this._getEditorComponentState(documentWrapperElementId)));
        } else {
          localStorage.setItem(this._createKey(documentWrapperElementId), JSON.stringify(this._getEditorComponentState(documentWrapperElementId)));
        }
      }
    }
  },

  _getEditorComponentState: function (documentWrapperElementId) {
    let editor = Quantumart.QP8.BackendEntityEditor.getComponent($(`#${documentWrapperElementId}`));

    if (editor) {
      return {
        wrapperElementId: documentWrapperElementId,
        recordId: new Date().getTime(),
        actionCode: editor.get_actionCode(),
        entityTypeCode: editor.get_entityTypeCode(),
        entityId: editor.get_entityId(),
        parentEntityId: editor.get_parentEntityId(),
        modifiedDateTime: editor.get_modifiedDateTime(),
        contextQuery: editor.get_contextQuery(),
        fieldValues: editor.get_fieldValues(),
        disableFields: editor.get_disabledFields(),
        hideFields: editor.get_hideFields()
      };
    }
  },

  // сортировка на предмет возможности восстановления
  _checkForRestoring: function (stateRecords) {
    let dfr = new jQuery.Deferred();
    if ($q.isArray(stateRecords) && !$q.isNullOrEmpty(stateRecords)) {
      let requestData = {
        recordHeaders: JSON.stringify(jQuery.map(stateRecords, (r) => {
          return {
            RecordId: r.recordId,
            ActionCode: r.actionCode,
            EntityTypeCode: r.entityTypeCode,
            EntityId: r.entityId,
            ParentEntityId: r.parentEntityId,
            ModifiedTicks: r.modifiedDateTime
          };
        }))
      };

      $q.getJsonFromUrl(
        'POST',
        `${window.CONTROLLER_URL_ENTITY_OBJECT}AutosaveRestoringCheck`,
        requestData,
        true,
        false
      ).done((data) => {
        if (data.success) {
          if (!$q.isNullOrEmpty(data.approvedRecordIDs)) {
            dfr.resolve(

              // Оставить только те записи, которые прошли проверку на сервере
              jQuery.grep(stateRecords, (r) => {
                return _.indexOf(data.approvedRecordIDs, r.recordId) > -1;
              })
            );
          } else {
            dfr.resolve([]);
          }
        } else {
          $q.alertError(data.Text);
          dfr.reject();
        }
      }).fail((jqXHR) => {
        $q.processGenericAjaxError(jqXHR);
        dfr.reject();
      });
    } else {
      dfr.resolve([]);
    }

    return dfr.promise();
  },

  _createKey: function (documentWrapperElementId) {
    return `${this._keyPrefix}.${documentWrapperElementId}`;
  },

  dispose: function () { }
};

Quantumart.QP8.EntityEditorAutoSaver.registerClass('Quantumart.QP8.EntityEditorAutoSaver', Quantumart.QP8.Observable);
