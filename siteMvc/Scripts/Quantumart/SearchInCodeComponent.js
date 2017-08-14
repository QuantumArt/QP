Quantumart.QP8.SearchInCodeComponent = function (filterElementId, gridElementId) {
  this._filterElementId = filterElementId;
  this._gridElementId = gridElementId;
  this._onDataBindingHandler = $.proxy(this._onDataBinding, this);
  this._onApplyFilterHandler = $.proxy(this._onApplyFilter, this);
  this._onClearFilterHandler = $.proxy(this._onClearFilter, this);
};

Quantumart.QP8.SearchInCodeComponent.prototype = {
  _filterElementId: '',
  _gridElementId: '',

  _extendGridData: function (data) {
    var $filter = $(`#${this._filterElementId}`);
    var filter = {
      templateId: $('.sic_templateSelector', $filter).find('select').val(),
      pageId: $('.sic_pageSelector', $filter).find('.stateField').val(),
      filterVal: $('.sic_filter input', $filter).val()
    };

    return Object.assign({}, data, filter);
  },

  _onDataBinding: function (e) {
    var $grid = $(`#${this._gridElementId}`);
    e.data = this._extendGridData(e.data);
  },

  _onApplyFilter: function () {
    var $grid = $(`#${this._gridElementId}`);
    var gridComponent = $grid.data('tGrid');
    gridComponent.ajaxRequest();
  },

  _onClearFilter: function () {
    var $filter = $(`#${this._filterElementId}`);
    $filter.find('.sic_templateSelector select').val('0');
    if ($filter.find('.singleItemPicker').size() > 0) {
      $filter.find('.singleItemPicker').data('entity_data_list_component').deselectAllListItems();
    }

    $filter.find('.sic_filter input').val('');
    $('.sic_search_button', this.$filter).trigger('click');
  },

  initialize: function () {
    var $grid = $(`#${this._gridElementId}`);
    var gridComponent = $grid.data('tGrid');
    var $filter = $(`#${this._filterElementId}`);

    Quantumart.QP8.ControlHelpers.initAllEntityDataLists($filter);

    $grid
      .unbind('dataBinding', gridComponent.onDataBinding)
      .bind('dataBinding', this._onDataBindingHandler);

    $('.sic_search_button', $filter)
      .click(this._onApplyFilterHandler);
    $('.sic_reset_button', $filter)
      .click(this._onClearFilterHandler);
    if ($('.sic_templateSelector select', $filter)) {
      $('.sic_templateSelector select', $filter).change($.proxy(this._onApplyFilter, this));
    }
    if ($('.sic_pageSelector .singleItemPicker', $filter).size() > 0 && $('.sic_pageSelector .singleItemPicker', $filter).data('entity_data_list_component')) {
      $('.sic_pageSelector .singleItemPicker', $filter).data('entity_data_list_component').attachObserver(window.EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED, $.proxy(this._onApplyFilter, this));
      gridComponent.ajaxRequest();
    }
  },

  dispose: function () {
    var $grid = $(`#${this._gridElementId}`);
    var $filter = $(`#${this._filterElementId}`);

    $grid.unbind('dataBinding');
    this._onDataBindingHandler = null;

    $('.sic_search_button', $filter).unbind();
    $('.sic_reset_button', $filter).unbind();
    this._onApplyFilterHandler = null;
    this._onDataBindingHandler = null;

    Quantumart.QP8.ControlHelpers.destroyAllEntityDataLists($filter);
    $q.collectGarbageInIE();
  }
};
