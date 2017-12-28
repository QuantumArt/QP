import { BackendArticleSearchBlock } from '../BackendArticleSearchBlock';
import { FieldSearchBase, FieldSearchState } from './FieldSearchBase';
import { $c } from '../../ControlHelpers';
import { $q } from '../../Utils';

export class RelationFieldSearch extends FieldSearchBase {
  // eslint-disable-next-line max-params
  constructor(
    containerElement,
    parentEntityId,
    fieldID,
    contentID,
    fieldColumn,
    fieldName,
    fieldGroup,
    referenceFieldID,
    searchType
  ) {
    super(
      containerElement,
      parentEntityId,
      fieldID,
      contentID,
      fieldColumn,
      fieldName,
      fieldGroup,
      referenceFieldID
    );

    this._searchType = searchType;

    this._onLoadHandler = this._onLoad.bind(this);
    this._onIsNullCheckBoxChangeHandler = this._onIsNullCheckBoxChange.bind(this);
    this._onSelectorChangeHandler = this._onSelectorChange.bind(this);
    this._onListContentChangedHandler = this._onListContentChanged.bind(this);
    this._expandHierarchyHandler = this._expandHierarchy.bind(this);
  }

  _onLoadHandler = null;
  _onSelectorChangeHandler = null;
  _onListContentChangedHandler = null;

  initialize() {
    let serverContent;
    $q.getJsonFromUrl('POST', `${window.CONTROLLER_URL_ARTICLE_SEARCH_BLOCK}RelationSearch`, {
      elementIdPrefix: this._elementIdPrefix,
      fieldID: this._fieldID,
      parentEntityId: this._parentEntityId,
      IDs: this._selectedEntitiesIDs
    }, false, false, data => {
      if (data.success) {
        serverContent = data.view;
      } else {
        $q.alertError(data.message);
      }
    }, jqXHR => {
      serverContent = null;
      $q.processGenericAjaxError(jqXHR);
    }
    );

    if (serverContent) {
      const $containerElement = $(this._containerElement);
      $containerElement.html(serverContent);

      this._isNullCheckBoxElement = document.getElementById(`${this._elementIdPrefix}_isNullCheckBox`);
      this._inverseCheckBoxElement = document.getElementById(`${this._elementIdPrefix}_inverseCheckBox`);
      this._unionAllCheckBoxElement = document.getElementById(`${this._elementIdPrefix}_unionAllCheckBox`);
      this._textAreaElement = document.getElementById(`${this._elementIdPrefix}_relationTextArea`);
      this._entityContainerElement = $containerElement.find('#EntityContainer').get(0);
      this._textAreaContainerElement = $containerElement.find('#TextAreaContainer').get(0);

      $(this._isNullCheckBoxElement).on('change', this._onIsNullCheckBoxChangeHandler);
      $('.radioButtonsList input[type="radio"]', $containerElement).on('click', this._onSelectorChangeHandler);
      $('.expandParentsButton', $containerElement)
        .on('click', this._expandHierarchy(`${window.CONTROLLER_URL_ARTICLE}GetParentIds`));
      $('.expandChildsButton', $containerElement)
        .on('click', this._expandHierarchy(`${window.CONTROLLER_URL_ARTICLE}GetChildArticleIds`));
      $containerElement.on(window.JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, this._onListContentChangedHandler);

      $(document).ready(this._onLoadHandler);
    }
  }

  getSelectedIds() {
    let result;
    if (this._isEntity) {
      result = this._getSelectedEntities().map(item => item.Id);
    } else {
      result = this._getIds($(this._textAreaElement).val());
    }

    return result;
  }

  getSearchQuery() {
    return BackendArticleSearchBlock.createFieldSearchQuery(
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
  }

  getBlockState() {
    return new FieldSearchState(
      this._searchType, this._fieldID, this._contentID, this._fieldColumn
      , this._fieldName, this._fieldGroup, this._referenceFieldID, {
        isNull: $(this._isNullCheckBoxElement).is(':checked'),
        inverse: $(this._inverseCheckBoxElement).is(':checked'),
        unionAll: $(this._unionAllCheckBoxElement).is(':checked'),
        entities: this._getSelectedEntities(),
        text: $(this._textAreaElement).val(),
        isEntity: this._isEntity
      });
  }

  _selectedEntitiesIDs = null;

  setBlockState(state) {
    if (state && !$q.isNullOrEmpty(state.entities)) {
      this._selectedEntitiesIDs = $.map(state.entities, item => item.Id);
    } else {
      this._selectedEntitiesIDs = null;
    }
  }

  getFilterDetails() {
    const stateData = this.getBlockState().data;
    let result;
    if (stateData.isNull) {
      result = $l.SearchBlock.isNullCheckBoxLabelText;
    } else if (!stateData.isEntity) {
      const ids = this._getIds(stateData.text);
      result = this._getText(ids);
    } else if ($q.isNullOrEmpty(stateData.entities)) {
      result = '';
    } else {
      result = this._getText(stateData.entities, e => $q.cutShort(e.Name, 10));
    }

    $q.warnIfEqDiff(result, '');
    if (stateData.inverse && result !== '') {
      result = `${$l.SearchBlock.notText}(${result})`;
    }

    return result;
  }

  restoreBlockState(state, isRestoreByClose) {
    if (state) {
      if (this._isNullCheckBoxElement) {
        const $isNullCheckBoxElement = $(this._isNullCheckBoxElement);
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
        this._selectedEntitiesIDs = $.map(state.entities, item => item.Id);

        this._replaceWithSelectedEntities();
      }

      $(this._textAreaElement).val(state.text);
      $(`.radioButtonsList input:radio[value=${state.isEntity ? 0 : 1}]`, this._containerElement)
        .prop('checked', true).trigger('click');
    }
  }

  _onListContentChanged(eventArgs, data) {
    this._toggleLinkVisibility(data.value);
  }

  _toggleLinkVisibility(selectedIds) {
    $('.expandParentsButton > a, .expandChildsButton > a', $(this._containerElement))
      .toggleClass('disabled', !selectedIds.length);
  }

  _expandHierarchy(url) {
    const that = this;
    return function () {
      const selectedIds = that.getSelectedIds();
      if (selectedIds && selectedIds.length) {
        $q.getAjax(url, {
          ids: that.getSelectedIds(),
          fieldId: that._fieldID,
          filter: that._getEntityDataList()._filter
        }, data => {
          that._selectedEntitiesIDs = Array.distinct(selectedIds.concat(data));
          that._replaceWithSelectedEntities();
        });
      }
    };
  }

  _getEntityDataList() {
    return $(this._entityDataListElement).data('entity_data_list_component');
  }

  _getSelectedEntities() {
    return this._getEntityDataList().getSelectedEntities();
  }

  _replaceWithSelectedEntities() {
    if (this._entityDataListElement) {
      this._getEntityDataList().selectEntities(this._selectedEntitiesIDs);
    }
  }

  _onIsNullCheckBoxChange() {
    const edlComponent = this._getEntityDataList();
    if ($(this._isNullCheckBoxElement).is(':checked')) {
      edlComponent.disableList();
      $(this._textAreaElement).prop('disabled', true);
    } else {
      edlComponent.enableList();
      $(this._textAreaElement).prop('disabled', false);
    }
  }

  _onLoad() {
    $c.initAllEntityDataLists(this._containerElement);
    this._entityDataListElement = $c.getAllEntityDataLists(this._containerElement).get(0);
    this._toggleLinkVisibility(this.getSelectedIds());
  }

  _onSelectorChange(e) {
    this._isEntity = +$(e.currentTarget).val() === 0;
    if (this._isEntity) {
      $(this._entityContainerElement).show();
      $(this._textAreaContainerElement).hide();
      this._getEntityDataList().refreshList();
    } else {
      $(this._entityContainerElement).hide();
      $(this._textAreaContainerElement).show();
    }
  }

  onOpen() {
    $c.fixAllEntityDataListsOverflow(this._containerElement);
  }

  dispose() {
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

    super.dispose();
  }

  _searchType = 0;
  _isEntity = true;

  _onIsNullCheckBoxChangeHandler = null;

  _isNullCheckBoxElement = null;
  _inverseCheckBoxElement = null;
  _unionAllCheckBoxElement = null;
  _entityDataListElement = null;

  _entityContainerElement = null;
  _textAreaContainerElement = null;
  _textAreaElement = null;
}


import('../BackendArticleSearchBlock').then(() => {
  Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch = RelationFieldSearch;
});
