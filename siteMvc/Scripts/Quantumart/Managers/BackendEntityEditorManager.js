window.EVENT_TYPE_ENTITY_EDITOR_IS_READY = 'OnEntityEditorIsReady';
window.EVENT_TYPE_ENTITY_EDITOR_DISPOSED = 'OnEntityEditorDisposed';
window.EVENT_TYPE_ENTITY_EDITOR_FIELD_CHANGED = 'OnEntityEditorFieldChanged';
window.EVENT_TYPE_ENTITY_EDITOR_ALL_FIELD_INVALIDATE = 'OnEntityEditorAllFieldInvalidate';

Quantumart.QP8.BackendEntityEditorManager = function () {
  Quantumart.QP8.BackendEntityEditorManager.initializeBase(this);
};

Quantumart.QP8.BackendEntityEditorManager.prototype = {
  _editorGroups: {},

  generateEditorGroupCode: function (entityTypeCode, entityId) {
    let editorGroupCode = String.format('{0}_{1}', entityTypeCode, entityId);

    return editorGroupCode;
  },

  getEditorGroup: function (editorGroupCode) {
    let editorGroup = null;
    if (this._editorGroups[editorGroupCode]) {
      editorGroup = this._editorGroups[editorGroupCode];
    }

    return editorGroup;
  },

  createEditorGroup: function (editorGroupCode) {
    let editorGroup = this.getEditorGroup(editorGroupCode);
    if (!editorGroup) {
      editorGroup = {};
      this._editorGroups[editorGroupCode] = editorGroup;
    }

    return editorGroup;
  },

  refreshEditorGroup: function (entityTypeCode, entityId, options) {
    let editorGroup = this.getEditorGroup(this.generateEditorGroupCode(entityTypeCode, entityId));
    if (editorGroup) {
      for (let documentWrapperElementId in editorGroup) {
        this.refreshEditor(documentWrapperElementId, options);
      }
    }

    editorGroup = null;
  },

  removeEditorGroup: function (editorGroupCode) {
    $q.removeProperty(this._editorGroups, editorGroupCode);
  },

  getEditor: function (editorCode) {
    let editor = null;

    for (let editorGroupCode in this._editorGroups) {
      let editorGroup = this._editorGroups[editorGroupCode];
      if (editorGroup[editorCode]) {
        editor = editorGroup[editorCode];
        break;
      }
    }

    return editor;
  },

  createEditor: function (documentWrapperElementId, entityTypeCode, entityId, actionTypeCode, options, hostOptions) {
    let editorGroupCode = this.generateEditorGroupCode(entityTypeCode, entityId);

    let finalOptions = {};
    Object.assign(finalOptions, options);
    if (hostOptions) {
      if (hostOptions.contextQuery) {
        Object.assign(finalOptions, { contextQuery: hostOptions.contextQuery });
      }
      if (hostOptions.eventArgsAdditionalData) {
        if ((entityId == 0 || hostOptions.eventArgsAdditionalData.restoring === true) && hostOptions.eventArgsAdditionalData.initFieldValues) {
          Object.assign(finalOptions, {
            initFieldValues: hostOptions.eventArgsAdditionalData.initFieldValues,
            restoring: hostOptions.eventArgsAdditionalData.restoring
          });
        }
        if (hostOptions.eventArgsAdditionalData.disabledFields) {
          Object.assign(finalOptions, { disabledFields: hostOptions.eventArgsAdditionalData.disabledFields });
        }
        if (hostOptions.eventArgsAdditionalData.hideFields) {
          Object.assign(finalOptions, { hideFields: hostOptions.eventArgsAdditionalData.hideFields });
        }
        if (hostOptions.eventArgsAdditionalData.contextQuery) {
          Object.assign(finalOptions, { contextQuery: hostOptions.eventArgsAdditionalData.contextQuery });
        }
      }

      finalOptions.isBindToExternal = hostOptions.isBindToExternal;
    }

    let editor = new Quantumart.QP8.BackendEntityEditor(editorGroupCode, documentWrapperElementId, entityTypeCode, entityId, actionTypeCode, finalOptions);
    editor.set_editorManagerComponent(this);

    let editorGroup = this.createEditorGroup(editorGroupCode);
    editorGroup[editorGroupCode] = editor;

    return editor;
  },

  refreshEditor: function (documentWrapperElementId, options) {
    let editor = this.getEditor(documentWrapperElementId);
    if (editor) {
      editor.refreshEditor(options);
    }

    editor = null;
  },

  removeEditor: function (documentWrapperElementId) {
    let editor = this.getEditor(documentWrapperElementId);
    if (editor) {
      let editorGroupCode = editor.get_editorGroupCode();
      let editorGroup = this.getEditorGroup(editorGroupCode);

      $q.removeProperty(editorGroup, documentWrapperElementId);

      if ($q.getHashKeysCount(editorGroup) == 0) {
        this.removeEditorGroup(editorGroupCode);
      }
    }
  },

  destroyEditor: function (documentWrapperElementId) {
    let editor = this.getEditor(documentWrapperElementId);
    if (editor != null) {
      if (editor.dispose) {
        editor.dispose();
      }
      editor = null;
    }
  },

  onActionExecuted: function (eventArgs) {
    let entityTypeCode = eventArgs.get_entityTypeCode();
    let actionTypeCode = eventArgs.get_actionTypeCode();
    let actionCode = eventArgs.get_actionCode();
    let entityId = eventArgs.get_entityId();

    if (actionCode == window.ACTION_CODE_MULTIPLE_PUBLISH_ARTICLES) {
      let entityIds = eventArgs.get_isMultipleEntities() ? $o.getEntityIDsFromEntities(eventArgs.get_entities()) : [entityId];
      let self = this;
      jQuery.each(entityIds, (index, id) => {
        self.refreshEditorGroup(window.ENTITY_TYPE_CODE_ARTICLE, id);
      });
    } else if (actionTypeCode == window.ACTION_TYPE_CODE_CHANGE_LOCK) {
      this.refreshEditorGroup(entityTypeCode, entityId);
    } else if (eventArgs.get_isRestored() && entityTypeCode == window.ENTITY_TYPE_CODE_ARTICLE_VERSION) {
      let confirmMessageText = String.format($l.EntityEditor.autoRefreshConfirmMessageAfterArticleRestoring, entityId);
      this.refreshEditorGroup(window.ENTITY_TYPE_CODE_ARTICLE, eventArgs.get_parentEntityId(), { confirmMessageText: confirmMessageText });
    } else if (actionCode == window.ACTION_CODE_ENABLE_ARTICLES_PERMISSIONS && entityTypeCode == window.ENTITY_TYPE_CODE_CONTENT) {
      this.refreshEditorGroup(entityTypeCode, entityId);
    }
  },

  onEntityEditorReady: function (documentWrapperElementId) {
    let eventArgs = new Sys.EventArgs();
    eventArgs.documentWrapperElementId = documentWrapperElementId;
    this.notify(window.EVENT_TYPE_ENTITY_EDITOR_IS_READY, eventArgs);
  },

  onEntityEditorDisposed: function (documentWrapperElementId) {
    let eventArgs = new Sys.EventArgs();
    eventArgs.documentWrapperElementId = documentWrapperElementId;
    this.notify(window.EVENT_TYPE_ENTITY_EDITOR_DISPOSED, eventArgs);
  },

  onFieldValueChanged: function (args) {
    let eventArgs = new Sys.EventArgs();
    eventArgs.fieldChangeInfo = args;
    this.notify(window.EVENT_TYPE_ENTITY_EDITOR_FIELD_CHANGED, eventArgs);
  },

  onAllFieldInvalidate: function (documentWrapperElementId) {
    let eventArgs = new Sys.EventArgs();
    eventArgs.documentWrapperElementId = documentWrapperElementId;
    this.notify(window.EVENT_TYPE_ENTITY_EDITOR_ALL_FIELD_INVALIDATE, eventArgs);
  },

  dispose: function () {
    Quantumart.QP8.BackendEntityEditorManager.callBaseMethod(this, 'dispose');

    if (this._editorGroups) {
      for (let editorGroupCode in this._editorGroups) {
        let editorGroup = this._editorGroups[editorGroupCode];
        Object.keys(editorGroup).forEach(this.destroyEditor);
        delete this._editorGroups[editorGroupCode];
      }
    }

    Quantumart.QP8.BackendEntityEditorManager._instance = null;
    $q.collectGarbageInIE();
  }
};

Quantumart.QP8.BackendEntityEditorManager._instance = null;

Quantumart.QP8.BackendEntityEditorManager.getInstance = function () {
  if (Quantumart.QP8.BackendEntityEditorManager._instance == null) {
    Quantumart.QP8.BackendEntityEditorManager._instance = new Quantumart.QP8.BackendEntityEditorManager();
  }

  return Quantumart.QP8.BackendEntityEditorManager._instance;
};

Quantumart.QP8.BackendEntityEditorManager.destroyInstance = function () {
  if (Quantumart.QP8.BackendEntityEditorManager._instance) {
    Quantumart.QP8.BackendEntityEditorManager._instance.dispose();
  }
};

Quantumart.QP8.BackendEntityEditorManager.registerClass('Quantumart.QP8.BackendEntityEditorManager', Quantumart.QP8.Observable);
