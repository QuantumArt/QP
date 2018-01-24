Quantumart.QP8.ActionLogComponent = function (filterElementId, gridElementId, actionTypes, entityTypes, actions) {
  this._filterElementId = filterElementId;
  this._gridElementId = gridElementId;
  this._onDataBindingHandler = $.proxy(this._onDataBinding, this);
  this._onApplyFilterHandler = $.proxy(this._onApplyFilter, this);
  this._onClearFilterHandler = $.proxy(this._onClearFilter, this);
  this._actionTypes = actionTypes;
  this._entityTypes = entityTypes;
  this._actions = actions;
};

Quantumart.QP8.ActionLogComponent.prototype = {
  _filterElementId: '',
  _gridElementId: '',
  $filterCombo: null,
  $tilesContainer: null,
  _actionTypes: null,
  _entityTypes: null,
  _tiles: {},

  initialize() {
    const $grid = $(`#${this._gridElementId}`);
    const gridComponent = $grid.data('tGrid');
    const $filter = $(`#${this._filterElementId}`);

    $grid
      .unbind('dataBinding', gridComponent.onDataBinding)
      .bind('dataBinding', this._onDataBindingHandler);

    $('.alSearchButton', $filter).click(this._onApplyFilterHandler);
    $('.alResetButton', $filter).click(this._onClearFilterHandler);


    this.$filterCombo = $filter
      .find('.alFilterCombo')
      .change($.proxy(this._onFilterSelected, this));

    this.$tilesContainer = $filter.find('.alTilesContainer');
    gridComponent.ajaxRequest();
  },

  _onApplyFilter() {
    $(`#${this._gridElementId}`)
      .data('tGrid')
      .ajaxRequest();
  },

  _onClearFilter() {
    this._destroyAllTiles();
    $('.alSearchButton', this.$filter).trigger('click');
  },

  _onDataBinding(e) {
    const filterData = this.getFilterData();
    if (filterData) {
      // eslint-disable-next-line no-param-reassign
      e.data = Object.assign({}, e.data, { searchQuery: JSON.stringify(filterData) });
    }
  },

  getFilterData() {
    const filterData = {};
    Object.keys(this._tiles).forEach(tileType => {
      if (tileType && Object.prototype.hasOwnProperty.call(this._tiles, tileType)) {
        this._tiles[tileType].getOptions().deriveFilterData(this._tiles[tileType], filterData);
      }
    });
    return filterData;
  },

  _onFilterSelected() {
    const $selected = this.$filterCombo.find('option:selected');
    if ($selected.val()) {
      this._createTile({ value: $selected.val(), text: $selected.text() });
    }

    this.$filterCombo.val('');
  },

  _onTileClose(eventType, sender, args) {
    this._destroyTile(args.type);
  },

  _createTile(options) {
    const that = this;
    if (options && options.value && !Object.prototype.hasOwnProperty.call(this._tiles, options.value)) {
      const ft = +options.value || 0;
      const tileComponent = new Quantumart.QP8.ActionLogFilterTile(this.$tilesContainer,
        {
          title: options.text,
          type: ft,
          windowSize: (function () {
            switch (+options.value || 0) {
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
                return new Quantumart.QP8.ActionLogTextFilter($filterContainer);
              case $e.ActionLogFilteredColumns.ExecutionTime:
                return new Quantumart.QP8.ActionLogDatetimeFilter($filterContainer);
              case $e.ActionLogFilteredColumns.ActionTypeName:
                return new Quantumart.QP8.ActionLogItemListFilter($filterContainer, that._actionTypes);
              case $e.ActionLogFilteredColumns.ActionName:
                return new Quantumart.QP8.ActionLogItemListFilter($filterContainer, that._actions);
              case $e.ActionLogFilteredColumns.EntityTypeName:
                return new Quantumart.QP8.ActionLogItemListFilter($filterContainer, that._entityTypes);
              case $e.ActionLogFilteredColumns.UserLogin:
                return new Quantumart.QP8.ActionLogUserFilter($filterContainer);
              default:
                return new Quantumart.QP8.ActionLogFilterBase($filterContainer);
            }
          },
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
      tileComponent.attachObserver(window.EVENT_TYPE_FILTER_TILE_CLOSE, $.proxy(this._onTileClose, this));
      tileComponent.initialize();
      this._tiles[options.value] = tileComponent;
    }
  },

  _destroyTile(tileType) {
    if (tileType && Object.prototype.hasOwnProperty.call(this._tiles, tileType)) {
      const tileComponent = this._tiles[tileType];
      tileComponent.detachObserver(window.EVENT_TYPE_FILTER_TILE_CLOSE);
      tileComponent.dispose();
      $q.removeProperty(this._tiles, tileType);
    }
  },

  _destroyAllTiles() {
    Object.keys(this._tiles).forEach(tileType => {
      this._destroyTile(tileType);
    }, this);
  },

  dispose() {
    this._destroyAllTiles();

    const $grid = $(`#${this._gridElementId}`);
    const $filter = $(`#${this._filterElementId}`);

    $grid.unbind('dataBinding');
    this._onDataBindingHandler = null;

    $('.alSearchButton', $filter).unbind();
    $('.alResetButton', $filter).unbind();
    this._onApplyFilterHandler = null;
    this._onDataBindingHandler = null;

    this.$filterCombo.off('change');
    this.$filterCombo = null;
    this.$tilesContainer = null;

    $q.collectGarbageInIE();
  }
};

Quantumart.QP8.ActionLogComponent.registerClass('Quantumart.QP8.ActionLogComponent');
