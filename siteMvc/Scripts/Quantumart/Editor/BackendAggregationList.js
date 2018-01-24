Quantumart.QP8.BackendAggregationList = function (componentElem) {
  this._componentElem = componentElem;
  this._containerElem = $('.aggregationListContainer', componentElem);
  this._resultElem = $('.aggregationListResult', componentElem);
};

Quantumart.QP8.BackendAggregationList.DATA_KEY_COMPONENT = 'aggregation_list_component';
Quantumart.QP8.BackendAggregationList.getComponent = function (componentElem) {
  if (componentElem) {
    return $q.toJQuery(componentElem).data(Quantumart.QP8.BackendAggregationList.DATA_KEY_COMPONENT);
  }

  return undefined;
};

Quantumart.QP8.BackendAggregationList.prototype = {
  _componentElem: null,
  _resultElem: null,
  _containerElem: null,
  _items: null,
  _fields: null,
  _addItemHandler: null,
  _removeItemHandler: null,
  _tableHeader: null,
  _tableBody: null,
  _additionalNames: null,
  _viewModel: null,

  initialize() {
    const aggrList = this._componentElem;
    $(aggrList).data(Quantumart.QP8.BackendAggregationList.DATA_KEY_COMPONENT, this);
    this._items = ko.observableArray(aggrList.data('aggregation_list_data'));
    this._fields = aggrList.data('aggregation_list_item_fields').split(',');
    this._addItemHandler = $.proxy(this.addItem, this);
    this._removeItemHandler = $.proxy(this.removeItem, this);
    if (!$q.isNull(aggrList.data('additional_names'))) {
      this._additionalNames = aggrList.data('additional_names').split(',');
    }
    this._viewModel = {
      items: this._items,
      addItem: this._addItemHandler,
      removeItem: this._removeItemHandler,
      onItemChanged: $.proxy(this._onItemChanged, this)
    };

    if (this._additionalNames) {
      this._additionalNames.forEach(curName => {
        this._viewModel[curName] = ko.observableArray(aggrList.data(`additional_${curName}`).split(','));
      }, this);
    }

    ko.applyBindingsToNode(
      this._containerElem.get(0),
      { template: { name: aggrList.attr('id').replace('_aggregationlist', '_template') } },
      this._viewModel
    );
    this._tableHeader = $('thead', this._componentElem);
    this._tableBody = $('tbody', this._componentElem);
    this.checkHeaders();
    this._componentElem.data('component', this);
  },

  getItems() {
    return this._items();
  },

  setItems(items) {
    if (this._items()) {
      this._items.removeAll();
      if (!$q.isNullOrEmpty(items) && $q.isArray(items)) {
        const that = this;
        $.each(items, function () {
          that._items.push(Object.assign({}, this));
        });
      }
    }
  },

  addItem() {
    const item = {};
    this._fields.forEach(field => {
      item[field] = '';
      return undefined;
    });

    item.Invalid = false;
    this._items.push(item);
    this._setAsChanged();
    this.checkHeaders();
  },

  removeItem(item) {
    this._items.remove(item);
    this._setAsChanged();
    this.checkHeaders();
  },

  _onItemChanged() {
    this._setAsChanged();
    return true;
  },

  _setAsChanged() {
    let $field = $(this._resultElem);
    $field.addClass(window.CHANGED_FIELD_CLASS_NAME);
    const fieldName = $(this._componentElem).data('field_name');
    $field.trigger(window.JQ_CUSTOM_EVENT_ON_FIELD_CHANGED,
      { fieldName, value: this._items(), contentFieldName: $field.closest('dl').data('field_name') }
    );
    $field = null;
  },

  checkHeaders() {
    if (this._tableBody.children('tr').size() === 0) {
      this._tableHeader.hide();
    } else {
      this._tableHeader.show();
    }
  },

  saveAggregationListData() {
    const aggrList = this._componentElem;
    this._resultElem.val(JSON.stringify(aggrList.data('aggregation_list_data')));
  },

  destroyAggregationList() {
    const containerElem = this._containerElem;
    ko.cleanNode(containerElem.get(0));

    if (this._componentElem) {
      $(this._componentElem).removeData(Quantumart.QP8.BackendAggregationList.DATA_KEY_COMPONENT);
      this._componentElem = null;
    }
    this._containerElem = null;
    this._resultElem = null;
    this._items = null;
    this._fields = null;
    this._addItemHandler = null;
    this._removeItemHandler = null;
    this._tableHeader = null;
    this._tableBody = null;
    this._viewModel = null;
  }
};
