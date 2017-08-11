// -- Компонент Action Log + Filter
Quantumart.QP8.ActionLogComponent = function (filterElementId, gridElementId, actionTypes, entityTypes, actions) {
  this._filterElementId = filterElementId;
  this._gridElementId = gridElementId;
  this._onDataBindingHandler = jQuery.proxy(this._onDataBinding, this);
  this._onApplyFilterHandler = jQuery.proxy(this._onApplyFilter, this);
  this._onClearFilterHandler = jQuery.proxy(this._onClearFilter, this);
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

  initialize: function () {
    var $grid = jQuery("#" + this._gridElementId);
    var gridComponent = $grid.data("tGrid");
    var $filter = jQuery('#' + this._filterElementId);

    $grid.unbind("dataBinding", gridComponent.onDataBinding)
       .bind("dataBinding", this._onDataBindingHandler);

    jQuery('.alSearchButton', $filter).click(this._onApplyFilterHandler);
    jQuery('.alResetButton', $filter).click(this._onClearFilterHandler);


    this.$filterCombo = $filter.find(".alFilterCombo")
      .change(
        jQuery.proxy(this._onFilterSelected, this)
      );
    this.$tilesContainer = $filter.find(".alTilesContainer");

    gridComponent.ajaxRequest();

    gridComponent = null;
    $grid = null;
    $filter = null;
  },


  _onApplyFilter: function () {
    jQuery("#" + this._gridElementId)
      .data("tGrid")
      .ajaxRequest();
  },

  _onClearFilter: function () {
    var $filter = jQuery('#' + this._filterElementId);
    this._destroyAllTiles();
    jQuery(".alSearchButton", this.$filter).trigger('click');

    $filter = null;
  },

  _onDataBinding: function (e) {
    var filterData = this.get_filterData();
    if (!jQuery.isEmptyObject(filterData)) {
      e.data = jQuery.extend(e.data, { searchQuery: JSON.stringify(filterData) });
    }
  },

  get_filterData: function () {
    var filterData = {};
    for (var tileType in this._tiles) {
      if (tileType && this._tiles.hasOwnProperty(tileType)) {
        this._tiles[tileType].get_options().deriveFilterData(this._tiles[tileType], filterData);
      }
    }
    return filterData;
  },

  _onFilterSelected: function () {
    var $selected = this.$filterCombo.find("option:selected");
    if ($selected.val()) {
      this._createTile({ value: $selected.val(), text: $selected.text() });
    }
    this.$filterCombo.val("");
    $selected = null;
  },

  _onTileClose: function (eventType, sender, args) {
    this._destroyTile(args.type);
  },

  _createTile: function (options) {
    var that = this;
    if (options && options.value && !this._tiles.hasOwnProperty(options.value)) {
      var ft = +options.value || 0;
      var tileComponent = new Quantumart.QP8.ActionLogFilterTile(this.$tilesContainer,
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
          createFilter: function ($filterContainer) {
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
          deriveFilterData: function (tile, filterData) {
            var v = tile.get_value();
            if (v) {
              switch (tile.get_options().type) {
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
                  if (jQuery.isArray(v) && v.length > 0) {
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
      tileComponent.attachObserver(EVENT_TYPE_FILTER_TILE_CLOSE, jQuery.proxy(this._onTileClose, this));
      tileComponent.initialize();
      this._tiles[options.value] = tileComponent;
      tileComponent = null;
    }
  },

  _destroyTile: function (tileType) {
    if (tileType && this._tiles.hasOwnProperty(tileType)) {
      var tileComponent = this._tiles[tileType];
      tileComponent.detachObserver(EVENT_TYPE_FILTER_TILE_CLOSE);
      tileComponent.dispose();
      $q.removeProperty(this._tiles, tileType);
      tileComponent = null;
    }
  },

  _destroyAllTiles: function () {
    for (var tileType in this._tiles) {
      this._destroyTile(tileType);
    }
  },

  dispose: function () {
    this._destroyAllTiles();

    var $grid = jQuery("#" + this._gridElementId);
    var $filter = jQuery('#' + this._filterElementId);

    $grid.unbind("dataBinding");
    this._onDataBindingHandler = null;

    jQuery('.alSearchButton', $filter).unbind();
    jQuery('.alResetButton', $filter).unbind();
    this._onApplyFilterHandler = null;
    this._onDataBindingHandler = null;

    this.$filterCombo.off("change");
    this.$filterCombo = null;

    this.$tilesContainer = null;

    $grid = null;
    $filter = null;
    $q.collectGarbageInIE();
  }
};
Quantumart.QP8.ActionLogComponent.registerClass("Quantumart.QP8.ActionLogComponent");
