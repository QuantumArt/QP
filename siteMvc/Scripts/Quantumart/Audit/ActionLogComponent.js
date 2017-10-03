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
    const $filter = $(`#${this._filterElementId}`);
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
    for (const tileType in this._tiles) {
      if (tileType && Object.prototype.hasOwnProperty.call(this._tiles, tileType)) {
        this._tiles[tileType].getOptions().deriveFilterData(this._tiles[tileType], filterData);
      }
    }

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
      let tileComponent = new Quantumart.QP8.ActionLogFilterTile(this.$tilesContainer,
        {
          title: options.text,
          type: ft,
          windowSize: (function () {
            switch (+options.value || 0) {
              case $e.ActionLogFilteredColumns.EntityStringId:
              case $e.ActionLogFilteredColumns.EntityTitle:
              case $e.ActionLogFilteredColumns.ParentEntityId:
              case $e.ActionLogFilteredColumns.EntityTypeName:
                return { w: 350, h: 65 };
              case $e.ActionLogFilteredColumns.ActionTypeName:
              case $e.ActionLogFilteredColumns.ActionName:
                return { w: 400, h: 65 };
              case $e.ActionLogFilteredColumns.ExecutionTime:
                return { w: 350, h: 112 };
              default:
                return { w: 350, h: 125 };
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
            const v = tile.getValue();
            if (v) {
              switch (tile.getOptions().type) {
                case $e.ActionLogFilteredColumns.EntityStringId:
                  filterData.entityStringId = v;
                  break;
                case $e.ActionLogFilteredColumns.EntityTitle:
                  filterData.entityTitle = v;
                  break;
                case $e.ActionLogFilteredColumns.ParentEntityId:
                  filterData.parentEntityId = v;
                  break;
                case $e.ActionLogFilteredColumns.ExecutionTime:
                  filterData.from = v.from;
                  filterData.to = v.to;
                  break;
                case $e.ActionLogFilteredColumns.ActionTypeName:
                  filterData.actionTypeCode = v;
                  break;
                case $e.ActionLogFilteredColumns.ActionName:
                  filterData.actionCode = v;
                  break;
                case $e.ActionLogFilteredColumns.EntityTypeName:
                  filterData.entityTypeCode = v;
                  break;
                case $e.ActionLogFilteredColumns.UserLogin:
                  if ($.isArray(v) && v.length > 0) {
                    filterData.userIDs = v;
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
      tileComponent = null;
    }
  },

  _destroyTile(tileType) {
    if (tileType && Object.prototype.hasOwnProperty.call(this._tiles, tileType)) {
      let tileComponent = this._tiles[tileType];
      tileComponent.detachObserver(window.EVENT_TYPE_FILTER_TILE_CLOSE);
      tileComponent.dispose();
      $q.removeProperty(this._tiles, tileType);
      tileComponent = null;
    }
  },

  _destroyAllTiles() {
    for (const tileType in this._tiles) {
      this._destroyTile(tileType);
    }
  },

  dispose() {
    this._destroyAllTiles();

    let $grid = $(`#${this._gridElementId}`);
    let $filter = $(`#${this._filterElementId}`);

    $grid.unbind('dataBinding');
    this._onDataBindingHandler = null;

    $('.alSearchButton', $filter).unbind();
    $('.alResetButton', $filter).unbind();
    this._onApplyFilterHandler = null;
    this._onDataBindingHandler = null;

    this.$filterCombo.off('change');
    this.$filterCombo = null;

    this.$tilesContainer = null;

    $grid = null;
    $filter = null;
    $q.collectGarbageInIE();
  }
};
Quantumart.QP8.ActionLogComponent.registerClass('Quantumart.QP8.ActionLogComponent');
