class BackendEntityDataListManager extends Quantumart.QP8.Observable {
  static getInstance() {
    if (!BackendEntityDataListManager._instance) {
      BackendEntityDataListManager._instance = new BackendEntityDataListManager();
    }

    return BackendEntityDataListManager._instance;
  }

  static destroyInstance() {
    if (BackendEntityDataListManager._instance) {
      BackendEntityDataListManager._instance.dispose();
      BackendEntityDataListManager._instance = null;
    }
  }

  static generateListGroupCode(entityTypeCode, parentEntityId) {
    return `${entityTypeCode}_${parentEntityId}`;
  }

  constructor() {
    super();
    this._listGroups = {};
    this._valueStorage = {};
  }

  getListGroup(listGroupCode) {
    return this._listGroups[listGroupCode];
  }

  createListGroup(listGroupCode) {
    if (!this.getListGroup(listGroupCode)) {
      this._listGroups[listGroupCode] = {};
    }

    return this._listGroups[listGroupCode];
  }

  refreshListGroup(entityTypeCode, parentEntityId, testEntityId) {
    const listGroup = this.getListGroup(
      BackendEntityDataListManager.generateListGroupCode(entityTypeCode, parentEntityId)
    );

    if (listGroup) {
      Object.keys(listGroup).forEach(listElementId => this.refreshList(listElementId, testEntityId), this);
    }
  }

  getList(listElementId) {
    const listGroup = Object.values(this._listGroups).find(val => val[listElementId]);
    return listGroup[listElementId];
  }

  // eslint-disable-next-line max-params
  createList(listElementId, entityTypeCode, parentEntityId, entityId, listType, options) {
    const listGroupCode = BackendEntityDataListManager.generateListGroupCode(entityTypeCode, parentEntityId);

    let list;
    if (listType === Quantumart.QP8.Enums.DataListType.DropDownList) {
      list = new Quantumart.QP8.BackendEntityDropDownList(
        listGroupCode,
        listElementId,
        entityTypeCode,
        parentEntityId,
        entityId,
        listType,
        options
      );
    } else if (listType === Quantumart.QP8.Enums.DataListType.CheckBoxList) {
      list = new Quantumart.QP8.BackendEntityCheckBoxList(
        listGroupCode,
        listElementId,
        entityTypeCode,
        parentEntityId,
        entityId,
        listType,
        options
      );
    } else if (listType === Quantumart.QP8.Enums.DataListType.SingleItemPicker) {
      list = new Quantumart.QP8.BackendEntitySingleItemPicker(
        listGroupCode,
        listElementId,
        entityTypeCode,
        parentEntityId,
        entityId,
        listType,
        options
      );
    } else if (listType === Quantumart.QP8.Enums.DataListType.MultipleItemPicker) {
      list = new Quantumart.QP8.BackendEntityMultipleItemPicker(
        listGroupCode,
        listElementId,
        entityTypeCode,
        parentEntityId,
        entityId,
        listType,
        options
      );
    } else {
      $q.alertError('Данный тип списка не поддерживается!');
      throw new Error('Данный тип списка не поддерживается!');
    }

    list.set_listManagerComponent(this);
    list.initialize();

    const listGroup = this.createListGroup(listGroupCode);
    listGroup[listElementId] = list;

    return list;
  }

  refreshList(listElementId, testEntityId) {
    const list = this.getList(listElementId);
    if (list) {
      list.refreshList(testEntityId);
    }
  }

  removeList(listElementId) {
    const list = this.getList(listElementId);
    if (list) {
      const listGroupCode = list.get_listGroupCode();
      const listGroup = this.getListGroup(listGroupCode);

      delete this._listGroups[listGroupCode][listElementId];
      if ($q.getHashKeysCount(listGroup) === 0) {
        delete this._listGroups[listGroupCode];
      }
    }
  }

  onActionExecuted(eventArgs) {
    const entityTypeCode = eventArgs.get_entityTypeCode();
    const parentEntityId = eventArgs.get_parentEntityId();
    const actionTypeCode = eventArgs.get_actionTypeCode();
    let testEntityId = 0;
    if (eventArgs.get_isSaved() || eventArgs.get_isUpdated()) {
      testEntityId = eventArgs.get_entityId();
    }

    if (eventArgs.get_isSaved()
      || eventArgs.get_isUpdated()
      || actionTypeCode === window.ACTION_TYPE_CODE_COPY
      || eventArgs.get_isRemoving()
      || eventArgs.get_isArchiving()
      || eventArgs.get_isRestoring()
    ) {
      this.refreshListGroup(entityTypeCode, parentEntityId, testEntityId);
    }

    if ((eventArgs.get_isArchiving() || eventArgs.get_isRemoving())
      && entityTypeCode === window.ENTITY_TYPE_CODE_ARTICLE
    ) {
      this.refreshListGroup(window.ENTITY_TYPE_CODE_ARCHIVE_ARTICLE, parentEntityId);
    } else if (eventArgs.get_isRestoring()
      && entityTypeCode === window.ENTITY_TYPE_CODE_ARCHIVE_ARTICLE
    ) {
      this.refreshListGroup(window.ENTITY_TYPE_CODE_ARTICLE, parentEntityId);
    }
  }

  getValueStorage() {
    return this._valueStorage;
  }

  dispose() {
    super.dispose();
    if (this._listGroups) {
      Object.values(this._listGroups).forEach(listGroup => {
        Object.keys(listGroup).forEach(listElementId => {
          const list = this.getList(listElementId);
          if (list && list.dispose) {
            list.dispose();
          }
        }, this);
      }, this);
    }

    this._listGroups = null;
    this._valueStorage = null;
    $q.collectGarbageInIE();
  }
}

Quantumart.QP8.BackendEntityDataListManager = BackendEntityDataListManager;
