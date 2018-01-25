import { BackendEntityEditor } from './BackendEntityEditor';
import { Observable } from '../Common/Observable';
import { $a, BackendActionParameters } from '../BackendActionExecutor';
import { $q } from '../Utils';

window.EVENT_TYPE_AUTO_SAVER_ACTION_EXECUTING = 'OnAutoSaverRestoringActionExecuting';

export class EntityEditorAutoSaver extends Observable {
  constructor(options) {
    super();

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
    const root = EntityEditorAutoSaver._keyNameRoot;
    this._keyPrefix = `${root}.${this._currentCustomerCode}.${this._currentUserId}`;
  }

  _restoringStateCount = 0;
  isRun = false;
  restoring = false;
  _currentCustomerCode = '';
  _currentUserId = '';
  _keyPrefix = '';

  start() {
    this.isRun = true;
    this.restoring = false;
  }

  restore() {
    this.isRun = false;
    this.restoring = true;

    const keyRegExp = new RegExp(`^${this._keyPrefix}`);
    const autoSaverKeys = [];
    const savedStates = [];

    Object.keys(localStorage).forEach(key => {
      if (keyRegExp.test(key)) {
        autoSaverKeys.push(key);
        savedStates.push($.parseJSON(localStorage.getItem(key)));
      }
    });

    for (let i = 0; i < autoSaverKeys.length; i++) {
      localStorage.removeItem(autoSaverKeys[i]);
    }

    this._checkForRestoring(savedStates).done($.proxy(function (approvedStates) {
      let eventArgs, editorState, action, params;
      if (approvedStates.length > 0 && $q.confirmMessage($l.EntityEditorAutoSaver.restoreConfirmationRequest)) {
        this._restoringStateCount = approvedStates.length;
        for (let j = 0; j < approvedStates.length; j++) {
          editorState = approvedStates[j];
          action = $a.getBackendActionByCode(editorState.actionCode);
          params = new BackendActionParameters({
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
    }, this)).fail($.proxy(function () {
      this._restoringStateCount = 0;
      this.start();
    }, this));
  }

  onEntityEditorReady(documentWrapperElementId) {
    const editor = BackendEntityEditor.getComponent($(`#${documentWrapperElementId}`));

    if (editor && editor.allowAutoSave() && (this.restoring || !editor.isFieldsValid())) {
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
  }

  onEntityEditorDisposed(documentWrapperElementId) {
    if (this.isRun) {
      localStorage.removeItem(this._createKey(documentWrapperElementId));
    }
  }

  onFieldChanged(fieldChangeInfo) {
    if (this.isRun) {
      const editor = BackendEntityEditor.getComponent($(`#${fieldChangeInfo.documentWrapperElementId}`));
      if (editor && editor.allowAutoSave()) {
        const key = this._createKey(fieldChangeInfo.documentWrapperElementId);
        let editorState = $.parseJSON(localStorage.getItem(key));
        if ($q.isNullOrEmpty(editorState)) {
          editorState = this._getEditorComponentState(fieldChangeInfo.documentWrapperElementId);
        } else {
          const [fieldState] = $.grep(editorState.fieldValues, fv => fv.fieldName === fieldChangeInfo.fieldName);
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
  }

  // Обработчик события изменения состояния редактора, при котором необходимо полностью перечитать состояние
  onAllFieldInvalidate(documentWrapperElementId) {
    if (this.isRun) {
      const editor = BackendEntityEditor.getComponent($(`#${documentWrapperElementId}`));

      if (editor && editor.allowAutoSave()) {
        const key = this._createKey(documentWrapperElementId);
        const editorState = $.parseJSON(localStorage.getItem(key));

        if (editorState) {
          localStorage.setItem(key, JSON.stringify(this._getEditorComponentState(documentWrapperElementId)));
        } else {
          localStorage.setItem(this._createKey(documentWrapperElementId),
            JSON.stringify(this._getEditorComponentState(documentWrapperElementId)));
        }
      }
    }
  }

  _getEditorComponentState(documentWrapperElementId) {
    const editor = BackendEntityEditor.getComponent($(`#${documentWrapperElementId}`));

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
    return undefined;
  }

  // сортировка на предмет возможности восстановления
  _checkForRestoring(stateRecords) {
    // eslint-disable-next-line new-cap
    const dfr = $.Deferred();
    if ($q.isArray(stateRecords) && !$q.isNullOrEmpty(stateRecords)) {
      const requestData = {
        recordHeaders: JSON.stringify(stateRecords.map(rh => ({
          RecordId: rh.recordId,
          ActionCode: rh.actionCode,
          EntityTypeCode: rh.entityTypeCode,
          EntityId: rh.entityId,
          ParentEntityId: rh.parentEntityId,
          ModifiedTicks: rh.modifiedDateTime
        })))
      };

      $q.getJsonFromUrl(
        'POST',
        `${window.CONTROLLER_URL_ENTITY_OBJECT}AutosaveRestoringCheck`,
        requestData,
        true,
        false
      ).done(data => {
        if (data.success) {
          if ($q.isNullOrEmpty(data.approvedRecordIDs)) {
            dfr.resolve([]);
          } else {
            dfr.resolve($.grep(stateRecords, rh => data.approvedRecordIDs.indexOf(rh.recordId) > -1));
          }
        } else {
          $q.alertError(data.Text);
          dfr.reject();
        }
      }).fail(jqXHR => {
        $q.processGenericAjaxError(jqXHR);
        dfr.reject();
      });
    } else {
      dfr.resolve([]);
    }

    return dfr.promise();
  }

  _createKey(documentWrapperElementId) {
    return `${this._keyPrefix}.${documentWrapperElementId}`;
  }

  // eslint-disable-next-line no-empty-function
  dispose() { }
}

EntityEditorAutoSaver._keyNameRoot = 'Quantumart.QP8.EntityEditorAutoSaver';


Quantumart.QP8.EntityEditorAutoSaver = EntityEditorAutoSaver;
