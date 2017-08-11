Quantumart.QP8.BackendAggregationList = function (componentElem) {
  this._componentElem = componentElem;
  this._containerElem = jQuery('.aggregationListContainer', componentElem);
  this._resultElem = jQuery('.aggregationListResult', componentElem);
};

Quantumart.QP8.BackendAggregationList.DATA_KEY_COMPONENT = "aggregation_list_component";
Quantumart.QP8.BackendAggregationList.getComponent = function (componentElem) {
  if (componentElem) {
    return $q.toJQuery(componentElem).data(Quantumart.QP8.BackendAggregationList.DATA_KEY_COMPONENT);
  }
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

  initialize: function () {
    var aggrList = this._componentElem;
    jQuery(aggrList).data(Quantumart.QP8.BackendAggregationList.DATA_KEY_COMPONENT, this);
    var result = this._resultElem;
    var containerElem = this._containerElem;
    this._items = ko.observableArray(aggrList.data('aggregation_list_data'));
    this._fields = aggrList.data('aggregation_list_item_fields').split(',');
    this._addItemHandler = jQuery.proxy(this.addItem, this);
    this._removeItemHandler = jQuery.proxy(this.removeItem, this);
    if (aggrList.data('additional_names') != undefined) {
 this._additionalNames = aggrList.data('additional_names').split(',');
}
    this._viewModel = {
      items: this._items,
      addItem: this._addItemHandler,
      removeItem: this._removeItemHandler,
      onItemChanged: jQuery.proxy(this._onItemChanged, this)
    };

    for (var i in this._additionalNames) {
        var curName = this._additionalNames[i];
        this._viewModel[curName] = ko.observableArray(aggrList.data('additional_' + curName).split(','));
    }

    ko.applyBindingsToNode(this._containerElem.get(0), { template: { name: aggrList.attr('id').replace('_aggregationlist', '_template') } }, this._viewModel);
    this._tableHeader = jQuery('thead', this._componentElem);
    this._tableBody = jQuery('tbody', this._componentElem);
    this.checkHeaders();
    this._componentElem.data('component', this);
  },

  get_items: function () {
    return this._items();
  },

  set_items: function (items) {
    if (this._items()) {
      this._items.removeAll();
      if (!$q.isNullOrEmpty(items) && $q.isArray(items)) {
        var self = this;
        jQuery.each(items, function () {
 self._items.push(jQuery.extend({}, this));
});
      }
    }
  },

  addItem: function () {
    var item = {};
    for (var i in this._fields) {
      item[this._fields[i]] = "";
    }
    item.Invalid = false;
    this._items.push(item);
    this._setAsChanged();
    this.checkHeaders();
  },

  removeItem: function (item) {
    this._items.remove(item);
    this._setAsChanged();
    this.checkHeaders();
  },

  _onItemChanged: function () {
    this._setAsChanged();
    return true;
  },

  _setAsChanged: function () {
    var $field = jQuery(this._resultElem);
    $field.addClass(CHANGED_FIELD_CLASS_NAME);
    var fieldName = jQuery(this._componentElem).data("field_name");
    $field.trigger(JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, { fieldName: fieldName, value: this._items(), contentFieldName: $field.closest("dl").data("field_name") });
    $field = null;
  },

  checkHeaders: function () {
    if (this._tableBody.children('tr').size() == 0) {
     this._tableHeader.hide();
    } else {
     this._tableHeader.show();
    }
  },

  saveAggregationListData: function () {
    var aggrList = this._componentElem;
    this._resultElem.val(JSON.stringify(aggrList.data('aggregation_list_data')));
  },

  destroyAggregationList: function () {
    var containerElem = this._containerElem;
    ko.cleanNode(containerElem.get(0));

    if (this._componentElem) {
      jQuery(this._componentElem).removeData(Quantumart.QP8.BackendAggregationList.DATA_KEY_COMPONENT);
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
