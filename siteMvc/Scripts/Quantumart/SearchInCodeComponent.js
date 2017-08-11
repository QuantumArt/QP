// -- Компонент Search in code
Quantumart.QP8.SearchInCodeComponent = function (filterElementId, gridElementId) {
    this._filterElementId = filterElementId;
    this._gridElementId = gridElementId;
    this._onDataBindingHandler = jQuery.proxy(this._onDataBinding, this);
    this._onApplyFilterHandler = jQuery.proxy(this._onApplyFilter, this);
    this._onClearFilterHandler = jQuery.proxy(this._onClearFilter, this);
};

Quantumart.QP8.SearchInCodeComponent.prototype = {
    _filterElementId: '',
    _gridElementId: '',

    _extendGridData: function (data) {
        var $filter = jQuery('#' + this._filterElementId);

        var filter = {
            templateId: jQuery('.sic_templateSelector', $filter).find('select').val(),
            pageId: jQuery('.sic_pageSelector', $filter).find('.stateField').val(),
            filterVal: jQuery('.sic_filter input', $filter).val()
        };

        $filter = null;
        return jQuery.extend(data, filter);
    },

    _onDataBinding: function (e) {
        var $grid = jQuery('#' + this._gridElementId);
        e.data = this._extendGridData(e.data);
        $grid = null;
    },

    _onApplyFilter: function () {
        var $grid = jQuery("#" + this._gridElementId);
        var gridComponent = $grid.data("tGrid");

        gridComponent.ajaxRequest();

        gridComponent = null;
        $grid = null;
    },

    _onClearFilter: function () {
        var $filter = jQuery('#' + this._filterElementId);
        $filter.find('.sic_templateSelector select').val('0');
        if ($filter.find('.singleItemPicker').size() > 0) {
            $filter.find('.singleItemPicker').data('entity_data_list_component').deselectAllListItems();
        }
        $filter.find('.sic_filter input').val('');

        jQuery(".sic_search_button", this.$filter).trigger('click');

        $filter = null;
    },

    initialize: function () {
        var $grid = jQuery("#" + this._gridElementId);
        var gridComponent = $grid.data("tGrid");
        var $filter = jQuery('#' + this._filterElementId);

        Quantumart.QP8.ControlHelpers.initAllEntityDataLists($filter);

        $grid
			.unbind("dataBinding", gridComponent.onDataBinding)
			.bind("dataBinding", this._onDataBindingHandler);

        jQuery('.sic_search_button', $filter)
			.click(this._onApplyFilterHandler);
        jQuery('.sic_reset_button', $filter)
			.click(this._onClearFilterHandler);
        if (jQuery('.sic_templateSelector select', $filter)) {
 jQuery('.sic_templateSelector select', $filter).change(jQuery.proxy(this._onApplyFilter, this)); 
}
        if (jQuery('.sic_pageSelector .singleItemPicker', $filter).size() > 0 && jQuery('.sic_pageSelector .singleItemPicker', $filter).data('entity_data_list_component')) {
            jQuery('.sic_pageSelector .singleItemPicker', $filter).data('entity_data_list_component').attachObserver(EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED, jQuery.proxy(this._onApplyFilter, this));
            gridComponent.ajaxRequest();
        }
        gridComponent = null;
        $grid = null;
        $filter = null;
    },

    dispose: function () {
        var $grid = jQuery("#" + this._gridElementId);
        var $filter = jQuery('#' + this._filterElementId);

        $grid.unbind("dataBinding");
        this._onDataBindingHandler = null;

        jQuery('.sic_search_button', $filter).unbind();
        jQuery('.sic_reset_button', $filter).unbind();
        this._onApplyFilterHandler = null;
        this._onDataBindingHandler = null;

        $grid = null;
        Quantumart.QP8.ControlHelpers.destroyAllEntityDataLists($filter);
        $filter = null;
        $q.collectGarbageInIE();
    }
};