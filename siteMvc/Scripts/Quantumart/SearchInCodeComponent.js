import { ControlHelpers } from './ControlHelpers';

export class SearchInCodeComponent {
  /** @type {JQuery} */
  $filter = null;
  /** @type {JQuery} */
  $grid = null;
  /** @type {kendo.ui.Grid} */
  _kendoGrid = null;

  /**
   * @param {string} filterElementId
   * @param {string} gridElementId
   */
  constructor(filterElementId, gridElementId) {
    this._filterElementId = filterElementId;
    this._gridElementId = gridElementId;
    this._onApplyFilter = this._onApplyFilter.bind(this);
    this._onClearFilter = this._onClearFilter.bind(this);
  }

  initialize() {
    this.$filter = $(`#${this._filterElementId}`);
    this.$grid = $(`#${this._gridElementId}`);
    this._kendoGrid = this.$grid.getKendoGrid();

    /**
     * @param {kendo.data.DataSourceTransportParameterMapData} data
     * @returns Query params
     */
    this._kendoGrid.dataSource.transport.parameterMap = data => {
      const result = {
        page: data.page,
        pageSize: data.pageSize,
        templateId: Number($('.sic_templateSelector', this.$filter).find('select').val()) || null,
        pageId: Number($('.sic_pageSelector', this.$filter).find('.stateField').val()) || null,
        filterVal: $('.sic_filter input', this.$filter).val()
      };
      if (data.sort && data.sort.length) {
        result.orderBy = `${data.sort[0].field}-${data.sort[0].dir}`;
      }
      return result;
    };

    ControlHelpers.initAllEntityDataLists(this.$filter);

    $('.sic_search_button', this.$filter).click(this._onApplyFilter);
    $('.sic_reset_button', this.$filter).click(this._onClearFilter);
    if ($('.sic_templateSelector select', this.$filter).size() > 0) {
      $('.sic_templateSelector select', this.$filter).change(this._onApplyFilter);
    }

    if ($('.sic_pageSelector .singleItemPicker', this.$filter).size() > 0
      && $('.sic_pageSelector .singleItemPicker', this.$filter).data('entity_data_list_component')
    ) {
      $('.sic_pageSelector .singleItemPicker', this.$filter)
        .data('entity_data_list_component')
        .attachObserver(window.EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED, this._onApplyFilter);

      this._kendoGrid.dataSource.read();
    }
  }

  _isApplyFilterRequested = false;

  _onApplyFilter() {
    // prevent double AJAX requests
    if (!this._isApplyFilterRequested) {
      this._isApplyFilterRequested = true;
      setTimeout(() => {
        this._isApplyFilterRequested = false;

        if (this._kendoGrid.dataSource.page() === 1) {
          this._kendoGrid.dataSource.read();
        } else {
          this._kendoGrid.dataSource.page(1);
        }
      });
    }
  }

  _onClearFilter() {
    this.$filter.find('.sic_templateSelector select').val('0');

    if (this.$filter.find('.singleItemPicker').size() > 0) {
      this.$filter.find('.singleItemPicker').data('entity_data_list_component').deselectAllListItems();
    }

    this.$filter.find('.sic_filter input').val('');

    this._onApplyFilter();
  }

  dispose() {
    this.$grid.unbind('dataBinding');

    $('.sic_search_button', this.$filter).unbind();
    $('.sic_reset_button', this.$filter).unbind();

    ControlHelpers.destroyAllEntityDataLists(this.$filter);
  }
}


Quantumart.QP8.SearchInCodeComponent = SearchInCodeComponent;
