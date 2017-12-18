window.EVENT_TYPE_ENTITY_EDITOR_IS_READY = 'OnEntityEditorIsReady';
window.EVENT_TYPE_ENTITY_EDITOR_DISPOSED = 'OnEntityEditorDisposed';
window.EVENT_TYPE_ENTITY_EDITOR_FIELD_CHANGED = 'OnEntityEditorFieldChanged';
window.EVENT_TYPE_ENTITY_EDITOR_ALL_FIELD_INVALIDATE = 'OnEntityEditorAllFieldInvalidate';

class BackendEntityEditorManager extends Quantumart.QP8.Observable {
  static getInstance() {
    if (!BackendEntityEditorManager._instance) {
      BackendEntityEditorManager._instance = new BackendEntityEditorManager();
    }

    return BackendEntityEditorManager._instance;
  }

  static destroyInstance() {
    if (BackendEntityEditorManager._instance) {
      BackendEntityEditorManager._instance.dispose();
      BackendEntityEditorManager._instance = null;
    }
  }

  static generateEditorGroupCode(entityTypeCode, entityId) {
    return `${entityTypeCode}_${entityId}`;
  }

  constructor() {
    // @ts-ignore
    super();
    this._editorGroups = {};
  }

  getEditorGroup(editorGroupCode) {
    return this._editorGroups[editorGroupCode];
  }

  createEditorGroup(editorGroupCode) {
    let editorGroup = this.getEditorGroup(editorGroupCode);
    if (!editorGroup) {
      editorGroup = {};
      this._editorGroups[editorGroupCode] = editorGroup;
    }

    return editorGroup;
  }

  refreshEditorGroup(entityTypeCode, entityId, options) {
    const editorGroup = this.getEditorGroup(
      BackendEntityEditorManager.generateEditorGroupCode(entityTypeCode, entityId)
    );

    if (editorGroup) {
      Object.keys(editorGroup).forEach(documentWrapperElementId => {
        this.refreshEditor(documentWrapperElementId, options);
      }, this);
    }
  }

  removeEditorGroup(editorGroupCode) {
    $q.removeProperty(this._editorGroups, editorGroupCode);
  }

  getEditor(editorCode) {
    const editorGroup = Object.values(this._editorGroups).find(val => val[editorCode]);
    return editorGroup[editorCode];
  }

  // eslint-disable-next-line max-params
  createEditor(documentWrapperElementId, entityTypeCode, entityId, actionTypeCode, options, hostOptions) {
    const editorGroupCode = BackendEntityEditorManager.generateEditorGroupCode(entityTypeCode, entityId);
    let finalOptions = Object.assign({}, options);

    if (hostOptions) {
      finalOptions = Object.assign({}, finalOptions, {
        isBindToExternal: hostOptions.isBindToExternal
      });

      if (hostOptions.contextQuery) {
        finalOptions = Object.assign({}, finalOptions, {
          contextQuery: hostOptions.contextQuery
        });
      }

      if (hostOptions.eventArgsAdditionalData) {
        if ((entityId === '0' || hostOptions.eventArgsAdditionalData.restoring)
          && hostOptions.eventArgsAdditionalData.initFieldValues
        ) {
          finalOptions = Object.assign({}, finalOptions, {
            initFieldValues: hostOptions.eventArgsAdditionalData.initFieldValues,
            restoring: hostOptions.eventArgsAdditionalData.restoring
          });
        }

        if (hostOptions.eventArgsAdditionalData.disabledFields) {
          finalOptions = Object.assign({}, finalOptions, {
            disabledFields: hostOptions.eventArgsAdditionalData.disabledFields
          });
        }

        if (hostOptions.eventArgsAdditionalData.hideFields) {
          finalOptions = Object.assign({}, finalOptions, {
            hideFields: hostOptions.eventArgsAdditionalData.hideFields
          });
        }

        if (hostOptions.eventArgsAdditionalData.contextQuery) {
          finalOptions = Object.assign({}, finalOptions, {
            contextQuery: hostOptions.eventArgsAdditionalData.contextQuery
          });
        }
      }
    }

    const editor = new Quantumart.QP8.BackendEntityEditor(
      editorGroupCode,
      documentWrapperElementId,
      entityTypeCode,
      entityId,
      actionTypeCode,
      finalOptions
    );

    editor.set_editorManagerComponent(this);

    const editorGroup = this.createEditorGroup(editorGroupCode);
    editorGroup[editorGroupCode] = editor;

    return editor;
  }

  refreshEditor(documentWrapperElementId, options) {
    const editor = this.getEditor(documentWrapperElementId);
    if (editor) {
      editor.refreshEditor(options);
    }
  }

  removeEditor(documentWrapperElementId) {
    const editor = this.getEditor(documentWrapperElementId);
    if (editor) {
      const editorGroupCode = editor.get_editorGroupCode();
      const editorGroup = this.getEditorGroup(editorGroupCode);

      $q.removeProperty(editorGroup, documentWrapperElementId);
      if ($q.getHashKeysCount(editorGroup) === 0) {
        this.removeEditorGroup(editorGroupCode);
      }
    }
  }

  onActionExecuted(eventArgs) {
    const entityTypeCode = eventArgs.get_entityTypeCode();
    const actionTypeCode = eventArgs.get_actionTypeCode();
    const actionCode = eventArgs.get_actionCode();
    const entityId = eventArgs.get_entityId();

    if (actionCode === window.ACTION_CODE_MULTIPLE_PUBLISH_ARTICLES) {
      const entityIds = eventArgs.get_isMultipleEntities()
        ? $o.getEntityIDsFromEntities(eventArgs.get_entities())
        : [entityId];

      const that = this;
      $.each(entityIds, (index, id) => {
        that.refreshEditorGroup(window.ENTITY_TYPE_CODE_ARTICLE, id);
      });
    } else if (actionTypeCode === window.ACTION_TYPE_CODE_CHANGE_LOCK) {
      this.refreshEditorGroup(entityTypeCode, entityId);
    } else if (eventArgs.get_isRestored() && entityTypeCode === window.ENTITY_TYPE_CODE_ARTICLE_VERSION) {
      const confirmMessageText = String.format(
        $l.EntityEditor.autoRefreshConfirmMessageAfterArticleRestoring,
        entityId
      );

      this.refreshEditorGroup(window.ENTITY_TYPE_CODE_ARTICLE, eventArgs.get_parentEntityId(), { confirmMessageText });
    } else if (actionCode === window.ACTION_CODE_ENABLE_ARTICLES_PERMISSIONS
      && entityTypeCode === window.ENTITY_TYPE_CODE_CONTENT
    ) {
      this.refreshEditorGroup(entityTypeCode, entityId);
    }
  }

  onEntityEditorReady(documentWrapperElementId) {
    const eventArgs = new Sys.EventArgs();
    eventArgs.documentWrapperElementId = documentWrapperElementId;
    this.notify(window.EVENT_TYPE_ENTITY_EDITOR_IS_READY, eventArgs);
  }

  onEntityEditorDisposed(documentWrapperElementId) {
    const eventArgs = new Sys.EventArgs();
    eventArgs.documentWrapperElementId = documentWrapperElementId;
    this.notify(window.EVENT_TYPE_ENTITY_EDITOR_DISPOSED, eventArgs);
  }

  onFieldValueChanged(args) {
    const eventArgs = new Sys.EventArgs();
    eventArgs.fieldChangeInfo = args;
    this.notify(window.EVENT_TYPE_ENTITY_EDITOR_FIELD_CHANGED, eventArgs);
  }

  onAllFieldInvalidate(documentWrapperElementId) {
    const eventArgs = new Sys.EventArgs();
    eventArgs.documentWrapperElementId = documentWrapperElementId;
    this.notify(window.EVENT_TYPE_ENTITY_EDITOR_ALL_FIELD_INVALIDATE, eventArgs);
  }

  dispose() {
    super.dispose();
    if (this._editorGroups) {
      Object.values(this._editorGroups).forEach(editorGroup => {
        Object.keys(editorGroup).forEach(documentWrapperElementId => {
          const editor = this.getEditor(documentWrapperElementId);
          if (editor && editor.dispose) {
            editor.dispose();
          }
        }, this);
      }, this);
    }

    this._editorGroups = null;
    $q.collectGarbageInIE();
  }
}

Quantumart.QP8.BackendEntityEditorManager = BackendEntityEditorManager;
