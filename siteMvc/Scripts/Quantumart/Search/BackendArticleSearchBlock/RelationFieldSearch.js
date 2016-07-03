//#region class BackendArticleSearchBlock.RelationFieldSearch
// === Класс блока поиска по числовому полю
Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch = function (containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID, searchType) {
  Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch.initializeBase(this, [containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID]);

  this._searchType = searchType;
  Quantumart.QP8.Utils.bindProxies.call(this, [
    '_onLoad',
    '_onIsNullCheckBoxChange',
    '_onSelectorChange',
    '_expandParentHierarchy',
    '_expandChildHierarchy'
  ]);
};

Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch.prototype = {
  initialize: function () {
    var serverContent;
    $q.getJsonFromUrl('POST', CONTROLLER_URL_ARTICLE_SEARCH_BLOCK + 'RelationSearch', {
        'elementIdPrefix': this._elementIdPrefix,
        'fieldID': this._fieldID,
        'parentEntityId': this._parentEntityId,
        'IDs': this._selectedEntitiesIDs
      }, false, false, function (data, textStatus, jqXHR) {
        if (data.success) {
          serverContent = data.view;
        } else {
          window.alert(data.message);
        }
      }, function (jqXHR, textStatus, errorThrown) {
        serverContent = null;
        $q.processGenericAjaxError(jqXHR);
      }
    );

    if (!$q.isNullOrWhiteSpace(serverContent)) {
      var isNullCheckBoxID = this._elementIdPrefix + '_isNullCheckBox';

      // полученную с сервера разметку добавить на страницу
      var $containerElement = $(this._containerElement);
      $containerElement.html(serverContent);

      // назначить обработчик события change чекбоксу
      var $isNullCheckBoxElement = $containerElement.find('#' + isNullCheckBoxID);
      $isNullCheckBoxElement.bind('change', this._onIsNullCheckBoxChangeHandler);

      // запомнить ссылку на dom-элементы
      this._isNullCheckBoxElement = $isNullCheckBoxElement.get(0);

      this._entityContainerElement = $containerElement.find('#EntityContainer').get(0);
      this._textAreaContainerElement = $containerElement.find('#TextAreaContainer').get(0);
      this._textAreaElement = $containerElement.find('#' + this._elementIdPrefix + '_relationTextArea').get(0);

      var inverseCheckBoxID = this._elementIdPrefix + '_inverseCheckBox';
      this._inverseCheckBoxElement = $containerElement.find('#' + inverseCheckBoxID).get(0);

      $('.radioButtonsList input[type="radio"]', $containerElement).on('click', this._onSelectorChangeHandler);
      $('.expandParentButton', $containerElement).on('click', this._expandParentHierarchyHandler);
      $('.expandChildsButton', $containerElement).on('click', this._expandChildHierarchyHandler);
      $(document).ready(this._onLoadHandler);

      $isNullCheckBoxElement = null;
      $containerElement = null;
    }
  },

  getSelectedIds: function() {
    var result;
    if (this._isEntity) {
      result = $.map(this._getSelectedEntities(), function(item) {
        return item.Id;
      });
    } else {
      result = this._getIds($(this._textAreaElement).val());
    }

    return result;
  },

  getParentArticlesHierarchy: function(ids) {
    var url = window.CONTROLLER_URL_ARTICLE + "GetParentIds2"
    var params = {
      ids: ids,
      fieldId: this._fieldID,
      filter: this._getEntityDataList()._filter
    }

    return Quantumart.QP8.Utils.getJsonSync(url, params);
  },

  getChildArticlesHierarchy: function(ids) {
    var url = window.CONTROLLER_URL_ARTICLE + "GetChildArticleIds"
    var params = {
      ids: ids,
      fieldId: this._fieldID,
      filter: this._getEntityDataList()._filter
    }

    return Quantumart.QP8.Utils.getJsonSync(url, params);
  },

  _expandParentHierarchy: function() {
    var selectedIds = this.getSelectedIds();
    this._selectedEntitiesIDs = this.getParentArticlesHierarchy(selectedIds);
    this._saveSelectedEntities();
  },

  _expandChildHierarchy: function() {
    var selectedIds = this.getSelectedIds();
    this._selectedEntitiesIDs = this.getChildArticlesHierarchy(selectedIds);
    this._saveSelectedEntities();
  },

  get_searchQuery: function () {
    return Quantumart.QP8.BackendArticleSearchBlock.createFieldSearchQuery(this._searchType, this._fieldID, this._fieldColumn, this._contentID, this._referenceFieldID, this.getSelectedIds(), this.get_IsNull(), this.get_Inverse());
  },

  get_blockState: function () {
    return new Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState(this._searchType, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldID, {
      isNull: this.get_IsNull(),
      inverse: this.get_Inverse(),
      entities: this._getSelectedEntities(),
      text: $(this._textAreaElement).val(),
      isEntity: this._isEntity
    });
  },

  _selectedEntitiesIDs: null,

  set_blockState: function (state) {
    if (state && !$q.isNullOrEmpty(state.entities)) {
      this._selectedEntitiesIDs = $.map(state.entities, function (item) {
        return item.Id;
      });
    } else {
      this._selectedEntitiesIDs = null;
    }
  },

  get_filterDetails: function () {
    var stateData = this.get_blockState().data;
    var result, builder;
    if (stateData.isNull) {
      result = $l.SearchBlock.isNullCheckBoxLabelText;
    } else if (!stateData.isEntity) {
      var ids = this._getIds(stateData.text);
      result = this._getText(ids);
    } else if (!$q.isNullOrEmpty(stateData.entities)) {
      result = this._getText(stateData.entities, function (e) { return $q.cutShort(e.Name, 10); });
    } else {
      result = '';
    }

    if (stateData.inverse && result != '') {
      result = $l.SearchBlock.notText + '(' + result + ')';
    }

    return result;
  },

  restore_blockState: function(state, isRestoreByClose) {
    if (state) {
      if (this._isNullCheckBoxElement) {
        var $isNullCheckBoxElement = $(this._isNullCheckBoxElement);
        $isNullCheckBoxElement.prop('checked', state.isNull);
        $isNullCheckBoxElement.trigger('change');
        $isNullCheckBoxElement = null;
      }

      if (this._inverseCheckBoxElement) {
        $(this._inverseCheckBoxElement).prop('checked', state.inverse);
      }

      if (isRestoreByClose) {
        this._selectedEntitiesIDs = $.map(state.entities, function(item) {
          return item.Id;
        });

        this._saveSelectedEntities();
      }

      $(this._textAreaElement).val(state.text);
      $('.radioButtonsList input:radio[value=' + (state.isEntity ? 0 : 1) + ']', this._containerElement).prop('checked', true).trigger('click');
    }
  },

  _getEntityDataList: function() {
    return $(this._entityDataListElement).data('entity_data_list_component');
  },

  _getSelectedEntities: function () {
    return this._getEntityDataList().getSelectedEntities();
  },

  _saveSelectedEntities: function() {
    if (this._entityDataListElement) {
      this._getEntityDataList().selectEntities(this._selectedEntitiesIDs);
    }
  },

  _onIsNullCheckBoxChange: function () {
    var edlComponent = this._getEntityDataList();
    if (this.get_IsNull()) {
      edlComponent.disableList();
      $(this._textAreaElement).prop('disabled', true);
    } else {
      edlComponent.enableList();
      $(this._textAreaElement).prop('disabled', false);
    }
  },

  _onLoad: function () {
    $c.initAllEntityDataLists(this._containerElement)
    this._entityDataListElement = $c.getAllEntityDataLists(this._containerElement).get(0);
  },

  _onSelectorChange: function (e) {
    this._isEntity = $(e.currentTarget).val() == 0;
    if (this._isEntity) {
      $(this._entityContainerElement).show();
      $(this._textAreaContainerElement).hide();
      this._getEntityDataList().refreshList();
    } else {
      $(this._entityContainerElement).hide();
      $(this._textAreaContainerElement).show();
    }
  },

  onOpen: function () {
    $c.fixAllEntityDataListsOverflow(this._containerElement);
  },

  dispose: function () {
    $c.destroyAllEntityDataLists(this._containerElement)

    // отвязать обработчик события change чекбоксу
    if (this._isNullCheckBoxElement) {
      var $isNullCheckBoxElement = $(this._isNullCheckBoxElement);
      $isNullCheckBoxElement.unbind('change', this._onIsNullCheckBoxChangeHandler);
      $isNullCheckBoxElement = null;
    }

    this._isNullCheckBoxElement = null;
    this._inverseCheckBoxElement = null;
    this._entityDataListElement = null;
    this._entityContainerElement = null;
    this._textAreaContainerElement = null;
    this._textAreaElement = null;
    this._onIsNullCheckBoxChangeHandler = null;
    this._onLoadHandler = null;

    Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch.callBaseMethod(this, 'dispose');
  },

  get_IsNull: function () {
    if (this._isNullCheckBoxElement) {
      return $(this._isNullCheckBoxElement).is(':checked');
    } else {
      return false;
    }
  },

  get_Inverse: function () {
    if (this._inverseCheckBoxElement) {
      return $(this._inverseCheckBoxElement).is(':checked');
    } else {
      return false;
    }
  },

  _searchType: 0, //тип поиска (Many2Nany или One2Many)
  _isEntity: true,

  _onIsNullCheckBoxChangeHandler: null,

  _isNullCheckBoxElement: null,
  _inverseCheckBoxElement: null,
  _entityDataListElement: null,

  _entityContainerElement: null,
  _textAreaContainerElement: null,
  _textAreaElement: null
};

Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch.registerClass('Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch', Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase);
//#endregion
