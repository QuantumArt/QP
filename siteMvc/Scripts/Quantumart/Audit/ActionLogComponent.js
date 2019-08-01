import { ActionLogDatetimeFilter } from './ActionLogDatetimeFilter';
import { ActionLogFilterBase } from './ActionLogFilterBase';
import { ActionLogFilterTile } from './ActionLogFilterTile';
import { ActionLogItemListFilter } from './ActionLogItemListFilter';
import { ActionLogTextFilter } from './ActionLogTextFilter';
import { ActionLogUserFilter } from './ActionLogUserFilter';

/**
 * @typedef {{ value: string; text: string; }} SelectOption
 */

export class ActionLogComponent {
  /** @type {JQuery} */
  $grid = null;
  /** @type {JQuery} */
  $filter = null;
  /** @type {JQuery} */
  $filterCombo = null;
  /** @type {JQuery} */
  $tilesContainer = null;
  /** @type {kendo.ui.Grid} */
  _kendoGrid = null;
  /** @type {{ [x: string]: ActionLogFilterTile }} */
  _tiles = {};

  /**
   * @param {string} filterElementId
   * @param {string} gridElementId
   * @param {SelectOption[]} actionTypes
   * @param {SelectOption[]} entityTypes
   * @param {SelectOption[]} actions
   */
  constructor(filterElementId, gridElementId, actionTypes, entityTypes, actions) {
    this._filterElementId = filterElementId;
    this._gridElementId = gridElementId;
    this._actionTypes = actionTypes;
    this._entityTypes = entityTypes;
    this._actions = actions;

    this._onApplyFilter = this._onApplyFilter.bind(this);
    this._onClearFilter = this._onClearFilter.bind(this);
    this._onFilterSelected = this._onFilterSelected.bind(this);
    this._onTileClose = this._onTileClose.bind(this);
  }

  initialize() {
    this.$grid = $(`#${this._gridElementId}`);
    this.$filter = $(`#${this._filterElementId}`);

    this._kendoGrid = this.$grid.getKendoGrid();

    /**
     * @param {kendo.data.DataSourceTransportParameterMapData} data
     * @returns Query params
     */
    this._kendoGrid.dataSource.transport.parameterMap = data => {
      const result = {
        page: data.page,
        pageSize: data.pageSize,
        searchQuery: JSON.stringify(this._getFilterData())
      };
      if (data.sort && data.sort.length) {
        result.orderBy = `${data.sort[0].field}-${data.sort[0].dir}`;
      }
      return result;
    };

    $('.alSearchButton', this.$filter).click(this._onApplyFilter);
    $('.alResetButton', this.$filter).click(this._onClearFilter);

    this.$filterCombo = this.$filter
      .find('.alFilterCombo')
      .change(this._onFilterSelected);

    this.$tilesContainer = this.$filter.find('.alTilesContainer');

    this._kendoGrid.dataSource.read();
  }

  _onApplyFilter() {
    if (this._kendoGrid.dataSource.page() === 1) {
      this._kendoGrid.dataSource.read();
    } else {
      this._kendoGrid.dataSource.page(1);
    }
  }

  _onClearFilter() {
    this._destroyAllTiles();
    this._onApplyFilter();
  }

  _getFilterData() {
    const filterData = {};
    Object.keys(this._tiles).forEach(tileType => {
      const tileComponent = this._tiles[tileType];
      tileComponent.getOptions().deriveFilterData(tileComponent, filterData);
    });
    return filterData;
  }

  _onFilterSelected() {
    const $selected = this.$filterCombo.find('option:selected');
    if ($selected.val()) {
      this._createTile({ value: $selected.val(), text: $selected.text() });
    }
    this.$filterCombo.val('');
  }

  _onTileClose(_eventType, _sender, args) {
    this._destroyTile(args.type);
  }

  /**
   * @param {SelectOption} options
   */
  _createTile(options) {
    const that = this;
    if (options && options.value && !Object.prototype.hasOwnProperty.call(this._tiles, options.value)) {
      const ft = Number(options.value) || 0;
      const tileComponent = new ActionLogFilterTile(this.$tilesContainer,
        {
          title: options.text,
          type: ft,
          windowSize: (function () {
            switch (Number(options.value) || 0) {
              case $e.ActionLogFilteredColumns.EntityStringId:
              case $e.ActionLogFilteredColumns.EntityTitle:
              case $e.ActionLogFilteredColumns.ParentEntityId:
              case $e.ActionLogFilteredColumns.EntityTypeName:
                return { width: 350, height: 65 };
              case $e.ActionLogFilteredColumns.ActionTypeName:
              case $e.ActionLogFilteredColumns.ActionName:
                return { width: 400, height: 65 };
              case $e.ActionLogFilteredColumns.ExecutionTime:
                return { width: 350, height: 112 };
              default:
                return { width: 350, height: 125 };
            }
          }()),
          createFilter($filterContainer) {
            switch (ft) {
              case $e.ActionLogFilteredColumns.EntityStringId:
              case $e.ActionLogFilteredColumns.EntityTitle:
              case $e.ActionLogFilteredColumns.ParentEntityId:
                return new ActionLogTextFilter($filterContainer);
              case $e.ActionLogFilteredColumns.ExecutionTime:
                return new ActionLogDatetimeFilter($filterContainer);
              case $e.ActionLogFilteredColumns.ActionTypeName:
                return new ActionLogItemListFilter($filterContainer, that._actionTypes);
              case $e.ActionLogFilteredColumns.ActionName:
                return new ActionLogItemListFilter($filterContainer, that._actions);
              case $e.ActionLogFilteredColumns.EntityTypeName:
                return new ActionLogItemListFilter($filterContainer, that._entityTypes);
              case $e.ActionLogFilteredColumns.UserLogin:
                return new ActionLogUserFilter($filterContainer);
              default:
                return new ActionLogFilterBase($filterContainer);
            }
          },

          /**
           * @param {ActionLogFilterTile} tile
           * @param {*} filterData
           */
          deriveFilterData(tile, filterData) {
            const tileValue = tile.getValue();
            if (tileValue) {
              switch (tile.getOptions().type) {
                case $e.ActionLogFilteredColumns.EntityStringId:
                  filterData.entityStringId = tileValue; // eslint-disable-line no-param-reassign
                  break;
                case $e.ActionLogFilteredColumns.EntityTitle:
                  filterData.entityTitle = tileValue; // eslint-disable-line no-param-reassign
                  break;
                case $e.ActionLogFilteredColumns.ParentEntityId:
                  filterData.parentEntityId = tileValue; // eslint-disable-line no-param-reassign
                  break;
                case $e.ActionLogFilteredColumns.ExecutionTime:
                  filterData.from = tileValue.from; // eslint-disable-line no-param-reassign
                  filterData.to = tileValue.to; // eslint-disable-line no-param-reassign
                  break;
                case $e.ActionLogFilteredColumns.ActionTypeName:
                  filterData.actionTypeCode = tileValue; // eslint-disable-line no-param-reassign
                  break;
                case $e.ActionLogFilteredColumns.ActionName:
                  filterData.actionCode = tileValue; // eslint-disable-line no-param-reassign
                  break;
                case $e.ActionLogFilteredColumns.EntityTypeName:
                  filterData.entityTypeCode = tileValue; // eslint-disable-line no-param-reassign
                  break;
                case $e.ActionLogFilteredColumns.UserLogin:
                  if ($.isArray(tileValue) && tileValue.length > 0) {
                    filterData.userIDs = tileValue; // eslint-disable-line no-param-reassign
                  }
                  break;
                default:
                  break;
              }
            }
          }
        }
      );
      tileComponent.attachObserver(window.EVENT_TYPE_FILTER_TILE_CLOSE, this._onTileClose);
      tileComponent.initialize();
      this._tiles[options.value] = tileComponent;
    }
  }

  _destroyTile(tileType) {
    if (tileType && Object.prototype.hasOwnProperty.call(this._tiles, tileType)) {
      const tileComponent = this._tiles[tileType];
      tileComponent.detachObserver(window.EVENT_TYPE_FILTER_TILE_CLOSE);
      tileComponent.dispose();
      delete this._tiles[tileType];
    }
  }

  _destroyAllTiles() {
    Object.keys(this._tiles).forEach(tileType => {
      this._destroyTile(tileType);
    });
  }

  dispose() {
    this._destroyAllTiles();

    $('.alSearchButton', this.$filter).unbind();
    $('.alResetButton', this.$filter).unbind();

    this.$filterCombo.off('change');
  }
}

Quantumart.QP8.ActionLogComponent = ActionLogComponent;
