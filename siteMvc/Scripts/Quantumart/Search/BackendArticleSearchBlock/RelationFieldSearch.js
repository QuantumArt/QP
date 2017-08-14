Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch = function (containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID, searchType) {
  Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch.initializeBase(this, [containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID]);

  this._searchType = searchType;
  $q.bindProxies.call(this, [
    '_onLoad',
    '_onIsNullCheckBoxChange',
    '_onSelectorChange',
    '_onListContentChanged',
    '_expandHierarchy'
  ]);
};

Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch.prototype = {
  initialize: function () {
    var serverContent;
    $q.getJsonFromUrl('POST', `${window.CONTROLLER_URL_ARTICLE_SEARCH_BLOCK  }RelationSearch`, {
        elementIdPrefix: this._elementIdPrefix,
        fieldID: this._fieldID,
        parentEntityId: this._parentEntityId,
        IDs: this._selectedEntitiesIDs
      }, false, false, function (data, textStatus, jqXHR) {
        if (data.success) {
          serverContent = data.view;
        } else {
          $q.alertError(data.message);
        }
      }, function (jqXHR, textStatus, errorThrown) {
        serverContent = null;
        $q.processGenericAjaxError(jqXHR);
      }
    );

    if (serverContent) {
      var $containerElement = $(this._containerElement);
      $containerElement.html(serverContent);

      this._isNullCheckBoxElement = document.getElementById(`${this._elementIdPrefix  }_isNullCheckBox`);
      this._inverseCheckBoxElement = document.getElementById(`${this._elementIdPrefix  }_inverseCheckBox`);
      this._unionAllCheckBoxElement = document.getElementById(`${this._elementIdPrefix  }_unionAllCheckBox`);
      this._textAreaElement = document.getElementById(`${this._elementIdPrefix  }_relationTextArea`);
      this._entityContainerElement = $containerElement.find('#EntityContainer').get(0);
      this._textAreaContainerElement = $containerElement.find('#TextAreaContainer').get(0);

      $(this._isNullCheckBoxElement).on('change', this._onIsNullCheckBoxChangeHandler);
      $('.radioButtonsList input[type="radio"]', $containerElement).on('click', this._onSelectorChangeHandler);
      $('.expandParentsButton', $containerElement).on('click', this._expandHierarchy(`${window.CONTROLLER_URL_ARTICLE  }GetParentIds`));
      $('.expandChildsButton', $containerElement).on('click', this._expandHierarchy(`${window.CONTROLLER_URL_ARTICLE  }GetChildArticleIds`));
      $containerElement.on(window.JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, this._onListContentChangedHandler);

      $(document).ready(this._onLoadHandler);
    }
  },

  getSelectedIds: function () {
    var result;
    if (this._isEntity) {
      result = $.map(this._getSelectedEntities(), function (item) {
        return item.Id;
      });
    } else {
      result = this._getIds($(this._textAreaElement).val());
    }

    return result;
  },

  get_searchQuery: function () {
    return Quantumart.QP8.BackendArticleSearchBlock.createFieldSearchQuery(
      this._searchType,
      this._fieldID,
      this._fieldColumn,
      this._contentID,
      this._referenceFieldID,
      this.getSelectedIds(),
      $(this._isNullCheckBoxElement).is(':checked'),
      $(this._inverseCheckBoxElement).is(':checked'),
      $(this._unionAllCheckBoxElement).is(':checked')
    );
  },

  get_blockState: function () {
    return new Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState(this._searchType, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldID, {
      isNull: $(this._isNullCheckBoxElement).is(':checked'),
      inverse: $(this._inverseCheckBoxElement).is(':checked'),
      unionAll: $(this._unionAllCheckBoxElement).is(':checked'),
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
      result = this._getText(stateData.entities, function (e) {
 return $q.cutShort(e.Name, 10);
});
    } else {
      result = '';
    }

    if (stateData.inverse && result != '') {
      result = `${$l.SearchBlock.notText  }(${  result  })`;
    }

    return result;
  },

  restore_blockState: function (state, isRestoreByClose) {
    if (state) {
      if (this._isNullCheckBoxElement) {
        var $isNullCheckBoxElement = $(this._isNullCheckBoxElement);
        $isNullCheckBoxElement.prop('checked', state.isNull);
        $isNullCheckBoxElement.trigger('change');
      }

      if (this._inverseCheckBoxElement) {
        $(this._inverseCheckBoxElement).prop('checked', state.inverse);
      }

      if (this._unionAllCheckBoxElement) {
        $(this._unionAllCheckBoxElement).prop('checked', state.unionAll);
      }

      if (isRestoreByClose) {
        this._selectedEntitiesIDs = $.map(state.entities, function (item) {
          return item.Id;
        });

        this._replaceWithSelectedEntities();
      }

      $(this._textAreaElement).val(state.text);
      $(`.radioButtonsList input:radio[value=${  state.isEntity ? 0 : 1  }]`, this._containerElement).prop('checked', true).trigger('click');
    }
  },

  _onListContentChanged: function (eventArgs, data) {
    this._toggleLinkVisibility(data.value);
  },

  _toggleLinkVisibility: function (selectedIds) {
    $('.expandParentsButton > a, .expandChildsButton > a', $(this._containerElement)).toggleClass('disabled', !selectedIds.length);
  },

  _expandHierarchy: function (url) {
    var self = this;
    return function () {
      var selectedIds = self.getSelectedIds();
      if (selectedIds && selectedIds.length) {
        $q.getAjax(url, {
          ids: self.getSelectedIds(),
          fieldId: self._fieldID,
          filter: self._getEntityDataList()._filter
        }, function (data) {
          self._selectedEntitiesIDs = $q.addRemoveToArrUniq(selectedIds, data);
          self._replaceWithSelectedEntities();
        });
      }
    };
  },

  _getEntityDataList: function () {
    return $(this._entityDataListElement).data('entity_data_list_component');
  },

  _getSelectedEntities: function () {
    return this._getEntityDataList().getSelectedEntities();
  },

  _replaceWithSelectedEntities: function () {
    if (this._entityDataListElement) {
      this._getEntityDataList().selectEntities(this._selectedEntitiesIDs);
    }
  },

  _onIsNullCheckBoxChange: function () {
    var edlComponent = this._getEntityDataList();
    if ($(this._isNullCheckBoxElement).is(':checked')) {
      edlComponent.disableList();
      $(this._textAreaElement).prop('disabled', true);
    } else {
      edlComponent.enableList();
      $(this._textAreaElement).prop('disabled', false);
    }
  },

  _onLoad: function () {
    $c.initAllEntityDataLists(this._containerElement);
    this._entityDataListElement = $c.getAllEntityDataLists(this._containerElement).get(0);
    this._toggleLinkVisibility(this.getSelectedIds());
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
    $c.destroyAllEntityDataLists(this._containerElement);
    if (this._isNullCheckBoxElement) {
      $(this._isNullCheckBoxElement).off('change', this._onIsNullCheckBoxChangeHandler);
    }

    $q.dispose.call(this, [
      '_isNullCheckBoxElement',
      '_inverseCheckBoxElement',
      '_unionAllCheckBoxElement',
      '_entityDataListElement',
      '_entityContainerElement',
      '_textAreaContainerElement',
      '_textAreaElement',
      '_onIsNullCheckBoxChangeHandler',
      '_onLoadHandler'
    ]);

    Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch.callBaseMethod(this, 'dispose');
  },

  _searchType: 0,
  _isEntity: true,

  _onIsNullCheckBoxChangeHandler: null,

  _isNullCheckBoxElement: null,
  _inverseCheckBoxElement: null,
  _unionAllCheckBoxElement: null,
  _entityDataListElement: null,

  _entityContainerElement: null,
  _textAreaContainerElement: null,
  _textAreaElement: null
};

Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch.registerClass('Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch', Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase);
