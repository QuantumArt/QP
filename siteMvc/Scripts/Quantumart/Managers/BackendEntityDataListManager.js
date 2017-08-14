Quantumart.QP8.BackendEntityDataListManager = function () {
  Quantumart.QP8.BackendEntityDataListManager.initializeBase(this);
};

Quantumart.QP8.BackendEntityDataListManager.prototype = {
  _listGroups: {},
  _valueStorage: {},

  generateListGroupCode: function (entityTypeCode, parentEntityId) {
    let listGroupCode = String.format('{0}_{1}', entityTypeCode, parentEntityId);

    return listGroupCode;
  },

  getListGroup: function (listGroupCode) {
    let listGroup = null;
    if (this._listGroups[listGroupCode]) {
      listGroup = this._listGroups[listGroupCode];
    }

    return listGroup;
  },

  createListGroup: function (listGroupCode) {
    let listGroup = this.getListGroup(listGroupCode);
    if (!listGroup) {
      listGroup = {};
      this._listGroups[listGroupCode] = listGroup;
    }

    return listGroup;
  },

  refreshListGroup: function (entityTypeCode, parentEntityId, testEntityId) {
    let listGroup = this.getListGroup(this.generateListGroupCode(entityTypeCode, parentEntityId));
    if (listGroup) {
      for (let listElementId in listGroup) {
        this.refreshList(listElementId, testEntityId);
      }
    }
  },

  getList: function (listElementId) {
    let list = null;

    for (let listGroupCode in this._listGroups) {
      let listGroup = this._listGroups[listGroupCode];
      if (listGroup[listElementId]) {
        list = listGroup[listElementId];
      }
    }

    return list;
  },

  createList: function (listElementId, entityTypeCode, parentEntityId, entityId, listType, options) {
    let listGroupCode = this.generateListGroupCode(entityTypeCode, parentEntityId);

    let list = null;
    if (listType == Quantumart.QP8.Enums.DataListType.DropDownList) {
      list = new Quantumart.QP8.BackendEntityDropDownList(listGroupCode, listElementId, entityTypeCode, parentEntityId, entityId, listType, options);
    } else if (listType == Quantumart.QP8.Enums.DataListType.CheckBoxList) {
      list = new Quantumart.QP8.BackendEntityCheckBoxList(listGroupCode, listElementId, entityTypeCode, parentEntityId, entityId, listType, options);
    } else if (listType == Quantumart.QP8.Enums.DataListType.SingleItemPicker) {
      list = new Quantumart.QP8.BackendEntitySingleItemPicker(listGroupCode, listElementId, entityTypeCode, parentEntityId, entityId, listType, options);
    } else if (listType == Quantumart.QP8.Enums.DataListType.MultipleItemPicker) {
      list = new Quantumart.QP8.BackendEntityMultipleItemPicker(listGroupCode, listElementId, entityTypeCode, parentEntityId, entityId, listType, options);
    }

    if (list) {
      list.set_listManagerComponent(this);
      list.initialize();

      let listGroup = this.createListGroup(listGroupCode);
      listGroup[listElementId] = list;
    } else {
      $q.alertError('Данный тип списка не поддерживается!');
    }

    return list;
  },

  refreshList: function (listElementId, testEntityId) {
    let list = this.getList(listElementId);
    if (list) {
      list.refreshList(testEntityId);
    }

    list = null;
  },

  removeList: function (listElementId) {
    let list = this.getList(listElementId);
    if (list) {
      let listGroupCode = list.get_listGroupCode();
      let listGroup = this.getListGroup(listGroupCode);

      delete this._listGroups[listGroupCode][listElementId];

      if ($q.getHashKeysCount(listGroup) == 0) {
        delete this._listGroups[listGroupCode];
      }
    }
  },

  destroyList: function (listElementId) {
    let list = this.getList(listElementId);
    if (list != null) {
      if (list.dispose) {
        list.dispose();
      }
    }
  },

  onActionExecuted: function (eventArgs) {
    let entityTypeCode = eventArgs.get_entityTypeCode();
    let parentEntityId = eventArgs.get_parentEntityId();
    let actionTypeCode = eventArgs.get_actionTypeCode();
    let testEntityId = 0;
    if (eventArgs.get_isSaved() || eventArgs.get_isUpdated()) {
      testEntityId = eventArgs.get_entityId();
    }

    if (eventArgs.get_isSaved()
      || eventArgs.get_isUpdated()
      || actionTypeCode == window.ACTION_TYPE_CODE_COPY
      || eventArgs.get_isRemoving()
      || eventArgs.get_isArchiving()
      || eventArgs.get_isRestoring()
    ) {
      this.refreshListGroup(entityTypeCode, parentEntityId, testEntityId);
    }

    if ((eventArgs.get_isArchiving() || eventArgs.get_isRemoving()) && entityTypeCode == window.ENTITY_TYPE_CODE_ARTICLE) {
      this.refreshListGroup(window.ENTITY_TYPE_CODE_ARCHIVE_ARTICLE, parentEntityId);
    } else if (eventArgs.get_isRestoring() && entityTypeCode == window.ENTITY_TYPE_CODE_ARCHIVE_ARTICLE) {
      this.refreshListGroup(window.ENTITY_TYPE_CODE_ARTICLE, parentEntityId);
    }
  },

  getValueStorage: function () {
    return this._valueStorage;
  },

  dispose: function () {
    Quantumart.QP8.BackendEntityDataListManager.callBaseMethod(this, 'dispose');

    if (this._listGroups) {
      for (let listGroupCode in this._listGroups) {
        let listGroup = this._listGroups[listGroupCode];
        Object.keys(listGroup).forEach(this.destroyList);
        delete this._listGroups[listGroupCode];
      }
    }

    $q.collectGarbageInIE();
  }
};

Quantumart.QP8.BackendEntityDataListManager._instance = null;
Quantumart.QP8.BackendEntityDataListManager.getInstance = function () {
  if (Quantumart.QP8.BackendEntityDataListManager._instance == null) {
    Quantumart.QP8.BackendEntityDataListManager._instance = new Quantumart.QP8.BackendEntityDataListManager();
  }

  return Quantumart.QP8.BackendEntityDataListManager._instance;
};

Quantumart.QP8.BackendEntityDataListManager.destroyInstance = function () {
  if (Quantumart.QP8.BackendEntityDataListManager._instance) {
    Quantumart.QP8.BackendEntityDataListManager._instance.dispose();
  }
};

Quantumart.QP8.BackendEntityDataListManager.registerClass('Quantumart.QP8.BackendEntityDataListManager', Quantumart.QP8.Observable);
